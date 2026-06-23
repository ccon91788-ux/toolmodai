<?php
class ApiController extends Controller
{
    private const ALLOWED_CHANNELS = ['stable', 'beta'];

    private function ensureFilesSchema(Database $db)
    {
        $queries = [
            "ALTER TABLE files ADD COLUMN version VARCHAR(50) NULL AFTER file_name",
            "ALTER TABLE files ADD COLUMN sha256 CHAR(64) NULL AFTER file_url",
            "ALTER TABLE files ADD COLUMN channel VARCHAR(20) NOT NULL DEFAULT 'stable' AFTER sha256",
            "ALTER TABLE files ADD COLUMN is_mandatory TINYINT(1) NOT NULL DEFAULT 0 AFTER channel",
            "ALTER TABLE files ADD COLUMN file_size BIGINT NULL AFTER is_mandatory",
            "ALTER TABLE files ADD COLUMN published_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER description"
        ];

        foreach ($queries as $sql) {
            try {
                $db->query($sql);
                $db->execute();
            } catch (\Throwable $e) {
                // Ignore duplicate-column errors on migrated databases.
            }
        }

        try {
            $db->query("UPDATE files SET version = file_name WHERE version IS NULL OR version = ''");
            $db->execute();
        } catch (\Throwable $e) {
            // Ignore if schema is still old.
        }

        try {
            $db->query("UPDATE files SET channel = 'stable' WHERE channel IS NULL OR channel = ''");
            $db->execute();
        } catch (\Throwable $e) {
            // Ignore if schema is still old.
        }
    }

    private function resolveChannel(): string
    {
        $channel = isset($_GET['channel']) ? strtolower(trim((string) $_GET['channel'])) : 'stable';
        if (!in_array($channel, self::ALLOWED_CHANNELS, true)) {
            return 'stable';
        }

        return $channel;
    }

    private function getLatestFile(Database $db, string $channel)
    {
        $db->query("SELECT * FROM files WHERE channel = :channel ORDER BY id DESC LIMIT 1");
        $db->bind(':channel', $channel);
        $file = $db->single();
        if ($file) {
            return $file;
        }

        $db->query("SELECT * FROM files ORDER BY id DESC LIMIT 1");
        return $db->single();
    }

    private function setCorsHeaders()
    {
        $allowedOrigin = 'https://zefoxtools.io.vn';
        header('Content-Type: application/json; charset=utf-8');
        header('Access-Control-Allow-Origin: ' . $allowedOrigin);
        header('Access-Control-Allow-Methods: POST, GET, OPTIONS');
        header('Access-Control-Allow-Headers: Content-Type, Authorization');
        header('Cache-Control: no-store, no-cache, must-revalidate, max-age=0');
        header('Pragma: no-cache');
        header('Expires: 0');
    }

    public function update()
    {
        $this->setCorsHeaders();

        $db = new Database();
        $this->ensureFilesSchema($db);
        $channel = $this->resolveChannel();
        $file = $this->getLatestFile($db, $channel);

        if ($file) {
            $version = !empty($file->version) ? $file->version : $file->file_name;
            echo json_encode([
                'success' => true,
                'version' => $version,
                'url' => $file->file_url,
                'sha256' => $file->sha256 ?? '',
                'description' => $file->description,
                'channel' => $file->channel ?? 'stable',
                'mandatory' => (bool) ($file->is_mandatory ?? 0),
                'size' => isset($file->file_size) ? (int) $file->file_size : 0,
                'publishedAt' => $file->published_at ?? null
            ]);
        } else {
            echo json_encode([
                'success' => false,
                'message' => 'No update available'
            ]);
        }
    }

    public function download($fileName = '')
    {
        $safeName = basename((string) $fileName);
        if ($safeName === '' || $safeName === '.' || $safeName === '..') {
            http_response_code(404);
            echo 'File not found';
            return;
        }

        $path = __DIR__ . '/../../public/downloads/' . $safeName;
        if (!is_file($path)) {
            http_response_code(404);
            echo 'File not found';
            return;
        }

        header('Content-Type: application/zip');
        header('Content-Length: ' . filesize($path));
        header('Content-Disposition: attachment; filename="' . $safeName . '"');
        header('Cache-Control: no-store, no-cache, must-revalidate, max-age=0');
        header('Pragma: no-cache');
        header('Expires: 0');
        readfile($path);
        exit;
    }

    public function auth($action = '')
    {
        $this->setCorsHeaders();
        http_response_code(410);
        echo json_encode([
            'success' => false,
            'message' => 'Deprecated endpoint removed.'
        ]);
    }

    public function license($action = '')
    {
        $this->setCorsHeaders();
        http_response_code(410);
        echo json_encode([
            'success' => false,
            'message' => 'Deprecated endpoint removed.'
        ]);
    }

    public function demo_update()
    {
        $this->setCorsHeaders();

        $db = new Database();
        
        // try to catch missing table/column gracefully, but assuming 'settings' exists
        try {
            $db->query("SELECT setting_value FROM settings WHERE setting_key = 'demo_update_info'");
            $row = $db->single();

            if ($row && !empty($row->setting_value)) {
                echo $row->setting_value;
            } else {
                echo json_encode([
                    'success' => false,
                    'message' => 'No demo update available'
                ]);
            }
        } catch (\Throwable $e) {
            echo json_encode([
                'success' => false,
                'message' => 'Database error: ' . $e->getMessage()
            ]);
        }
    }
}
