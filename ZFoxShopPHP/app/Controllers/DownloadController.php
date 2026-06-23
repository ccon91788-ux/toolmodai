<?php
class DownloadController extends Controller
{
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
            }
            catch (\Throwable $e) {
            // Ignore duplicate-column errors.
            }
        }
    }

    public function index()
    {
        $db = new Database();
        $this->ensureFilesSchema($db);
        $db->query("SELECT * FROM files WHERE channel = 'stable' ORDER BY id DESC LIMIT 1");
        $file = $db->single();
        if (!$file) {
            $db->query("SELECT * FROM files ORDER BY id DESC LIMIT 1");
            $file = $db->single();
        }

        $data = [
            'title' => 'Tải Tool 10 in 1',
            'file' => $file
        ];

        $this->view('download/index', $data);
    }
}
