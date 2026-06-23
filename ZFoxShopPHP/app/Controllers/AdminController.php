<?php
class AdminController extends Controller
{
    private const ALLOWED_CHANNELS = ['stable', 'beta'];

    public function __construct()
    {
        if (!isset($_SESSION['user_role']) || $_SESSION['user_role'] !== 'admin') {
            $this->redirect('');
        }
    }

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
                // Ignore duplicate-column errors.
            }
        }

        try {
            $db->query("UPDATE files SET version = file_name WHERE version IS NULL OR version = ''");
            $db->execute();
        } catch (\Throwable $e) {
            // Ignore.
        }

        try {
            $db->query("UPDATE files SET channel = 'stable' WHERE channel IS NULL OR channel = ''");
            $db->execute();
        } catch (\Throwable $e) {
            // Ignore.
        }
    }

    private function normalizeVersion(string $version): string
    {
        $version = trim($version);
        if ($version === '') {
            return date('Y.m.d.His');
        }

        $version = str_replace(['v', 'V'], '', $version);
        $version = preg_replace('/[^0-9.]/', '', $version);
        $version = trim((string) $version, '.');

        if ($version === '') {
            return date('Y.m.d.His');
        }

        return $version;
    }

    private function resolveChannel(string $value): string
    {
        $channel = strtolower(trim($value));
        if (!in_array($channel, self::ALLOWED_CHANNELS, true)) {
            return 'stable';
        }

        return $channel;
    }

    private function buildReleaseFileName(string $version): string
    {
        $versionSafe = preg_replace('/[^0-9A-Za-z._-]/', '_', $version);
        $stamp = date('Ymd_His');
        $nonce = substr(bin2hex(random_bytes(4)), 0, 8);
        return "ZFoxShop_Tool_v{$versionSafe}_{$stamp}_{$nonce}.zip";
    }

    private function cleanupOldZipFiles(string $uploadDir, string $keepFileName): void
    {
        $pattern = rtrim($uploadDir, '/\\') . '/*.zip';
        $zipFiles = glob($pattern);
        if (!$zipFiles) {
            return;
        }

        foreach ($zipFiles as $filePath) {
            $baseName = basename($filePath);
            if ($baseName === $keepFileName) {
                continue;
            }

            if (is_file($filePath)) {
                @unlink($filePath);
            }
        }
    }

    private function saveRelease(Database $db, string $version, string $url, string $description, string $sha256, int $size, string $channel, bool $mandatory): bool
    {
        // Luon chi giu 1 ban release duy nhat trong database.
        $db->query("TRUNCATE TABLE files");
        $db->execute();

        $displayName = 'Tool Tự Động Hóa v' . $version;
        $db->query(
            "INSERT INTO files (file_name, version, file_url, sha256, description, channel, is_mandatory, file_size, published_at)
             VALUES (:name, :version, :url, :sha, :desc, :channel, :mandatory, :size, NOW())"
        );
        $db->bind(':name', $displayName);
        $db->bind(':version', $version);
        $db->bind(':url', $url);
        $db->bind(':sha', $sha256);
        $db->bind(':desc', $description);
        $db->bind(':channel', $channel);
        $db->bind(':mandatory', $mandatory ? 1 : 0);
        $db->bind(':size', $size);
        return $db->execute();
    }

    public function index()
    {
        $db = new Database();
        $this->ensureFilesSchema($db);

        $db->query("SELECT COUNT(*) as total FROM users");
        $totalUsers = $db->single()->total;

        $db->query("SELECT COUNT(*) as total FROM files");
        $totalFiles = $db->single()->total;

        $db->query("SELECT * FROM files ORDER BY id DESC LIMIT 1");
        $currentFile = $db->single();

        $data = [
            'title' => 'Admin Dashboard - ZFoxShop',
            'totalUsers' => $totalUsers,
            'totalFiles' => $totalFiles,
            'currentFile' => $currentFile,
            'success' => isset($_SESSION['admin_success']) ? $_SESSION['admin_success'] : '',
            'error' => isset($_SESSION['admin_error']) ? $_SESSION['admin_error'] : ''
        ];

        unset($_SESSION['admin_success']);
        unset($_SESSION['admin_error']);

        $this->view('admin/index', $data);
    }

    public function upload()
    {
        if ($_SERVER['REQUEST_METHOD'] !== 'POST' || !isset($_FILES['zipfile'])) {
            $this->redirect('/admin');
            return;
        }

        $file = $_FILES['zipfile'];
        $uploadDir = __DIR__ . '/../../public/downloads/';
        if (!is_dir($uploadDir)) {
            mkdir($uploadDir, 0755, true);
        }

        $fileExt = strtolower(pathinfo($file['name'], PATHINFO_EXTENSION));
        if ($fileExt !== 'zip') {
            $_SESSION['admin_error'] = 'Chỉ chấp nhận file .zip!';
            $this->redirect('/admin');
            return;
        }

        if ($file['error'] !== UPLOAD_ERR_OK) {
            $_SESSION['admin_error'] = 'Lỗi upload: code ' . $file['error'];
            $this->redirect('/admin');
            return;
        }

        $version = $this->normalizeVersion($_POST['upload_version'] ?? '');
        $channel = $this->resolveChannel($_POST['channel'] ?? 'stable');
        $mandatory = isset($_POST['mandatory']) && $_POST['mandatory'] === '1';
        $description = trim((string) ($_POST['description'] ?? ''));
        if ($description === '') {
            $description = 'Bản cập nhật ' . $version;
        }

        $newName = $this->buildReleaseFileName($version);
        $targetPath = $uploadDir . $newName;

        if (!move_uploaded_file($file['tmp_name'], $targetPath)) {
            $_SESSION['admin_error'] = 'Có lỗi xảy ra khi upload file.';
            $this->redirect('/admin');
            return;
        }

        // Hosting 200MB: chỉ giữ lại file mới nhất, xóa toàn bộ zip cũ.
        $this->cleanupOldZipFiles($uploadDir, $newName);

        $sha256 = strtoupper(hash_file('sha256', $targetPath));
        $size = filesize($targetPath);
        $fileUrl = rtrim(BASE_URL, '/') . '/api/download/' . rawurlencode($newName);

        $db = new Database();
        $this->ensureFilesSchema($db);

        if ($this->saveRelease($db, $version, $fileUrl, $description, $sha256, (int) $size, $channel, $mandatory)) {
            $_SESSION['admin_success'] = 'Tải lên thành công! Đã đổi tên file mới và xóa các file zip cũ. Release: v' . $version . ' | SHA256: ' . $sha256;
        } else {
            $_SESSION['admin_error'] = 'Lỗi lưu release vào database.';
        }

        $this->redirect('/admin');
    }

    public function update_link()
    {
        $_SESSION['admin_error'] = 'Đã tắt cập nhật thủ công. Vui lòng dùng mục tải lên file ZIP.';
        $this->redirect('/admin');
    }

    public function delete_link()
    {
        $db = new Database();
        $db->query("TRUNCATE TABLE files");
        if ($db->execute()) {
            $_SESSION['admin_success'] = 'Đã xóa toàn bộ dữ liệu cập nhật!';
        }
        $this->redirect('/admin');
    }

    public function reset_device($id = 0)
    {
        $_SESSION['admin_error'] = 'Chức năng key/license đã bị gỡ bỏ.';
        $this->redirect('/admin');
    }

    public function toggle_ban($id = 0)
    {
        $_SESSION['admin_error'] = 'Chức năng key/license đã bị gỡ bỏ.';
        $this->redirect('/admin');
    }

    public function update_web()
    {
        if ($_SERVER['REQUEST_METHOD'] !== 'POST' || !isset($_FILES['webzip'])) {
            $this->redirect('/admin');
            return;
        }

        $file = $_FILES['webzip'];
        $ext = strtolower(pathinfo($file['name'], PATHINFO_EXTENSION));

        if ($ext !== 'zip') {
            $_SESSION['admin_error'] = 'Chỉ chấp nhận file .zip!';
            $this->redirect('/admin');
            return;
        }

        if ($file['error'] !== UPLOAD_ERR_OK) {
            $_SESSION['admin_error'] = 'Lỗi upload: code ' . $file['error'];
            $this->redirect('/admin');
            return;
        }

        $webRoot = realpath(__DIR__ . '/../../');
        if (!$webRoot) {
            $_SESSION['admin_error'] = 'Không xác định được thư mục gốc web.';
            $this->redirect('/admin');
            return;
        }

        $protectedFiles = [
            'config/config.php',
            '.htaccess',
        ];

        $zip = new \ZipArchive();
        $tmpPath = $file['tmp_name'];

        if ($zip->open($tmpPath) !== true) {
            $_SESSION['admin_error'] = 'Không thể mở file ZIP.';
            $this->redirect('/admin');
            return;
        }

        $extracted = 0;
        $skipped = 0;

        for ($i = 0; $i < $zip->numFiles; $i++) {
            $entryName = $zip->getNameIndex($i);
            $entryName = str_replace('\\', '/', $entryName);

            if (substr($entryName, -1) === '/') {
                continue;
            }

            $isProtected = false;
            foreach ($protectedFiles as $pf) {
                if ($entryName === $pf || str_ends_with($entryName, '/' . $pf)) {
                    $isProtected = true;
                    break;
                }
            }
            if ($isProtected) {
                $skipped++;
                continue;
            }

            $destPath = $webRoot . '/' . $entryName;
            $realDest = realpath(dirname($destPath));
            if ($realDest === false) {
                mkdir(dirname($destPath), 0755, true);
            }

            $realDest = realpath(dirname($destPath));
            if ($realDest === false || strpos($realDest, $webRoot) !== 0) {
                $skipped++;
                continue;
            }

            $content = $zip->getFromIndex($i);
            if ($content !== false) {
                file_put_contents($destPath, $content);
                $extracted++;
            }
        }

        $zip->close();
        @unlink($tmpPath);

        $_SESSION['admin_success'] = "Cập nhật web thành công! Đã ghi đè {$extracted} file, bỏ qua {$skipped} file bảo vệ.";
        $this->redirect('/admin');
    }

    public function upload_demo()
    {
        if ($_SERVER['REQUEST_METHOD'] !== 'POST' || !isset($_FILES['demozip'])) {
            $this->redirect('/admin');
            return;
        }

        $file = $_FILES['demozip'];
        $uploadDir = __DIR__ . '/../../public/downloads/';
        if (!is_dir($uploadDir)) {
            mkdir($uploadDir, 0755, true);
        }

        $fileExt = strtolower(pathinfo($file['name'], PATHINFO_EXTENSION));
        if ($fileExt !== 'zip') {
            $_SESSION['admin_error'] = 'Chỉ chấp nhận file .zip!';
            $this->redirect('/admin');
            return;
        }

        if ($file['error'] !== UPLOAD_ERR_OK) {
            $_SESSION['admin_error'] = 'Lỗi upload: code ' . $file['error'];
            $this->redirect('/admin');
            return;
        }

        // Tên file format: demo_20260404_151600.zip
        $timeString = date('Ymd_His');
        $newName = 'demo_' . $timeString . '.zip';
        $targetPath = $uploadDir . $newName;

        if (!move_uploaded_file($file['tmp_name'], $targetPath)) {
            $_SESSION['admin_error'] = 'Có lỗi xảy ra khi upload file demo.';
            $this->redirect('/admin');
            return;
        }

        // Cleanup old demo zip files
        $pattern = rtrim($uploadDir, '/\\') . '/demo_*.zip';
        $zipFiles = glob($pattern);
        if ($zipFiles) {
            foreach ($zipFiles as $filePath) {
                if (basename($filePath) !== $newName && is_file($filePath)) {
                    @unlink($filePath);
                }
            }
        }

        $sha256 = strtoupper(hash_file('sha256', $targetPath));
        $fileUrl = rtrim(BASE_URL, '/') . '/api/download/' . rawurlencode($newName);

        // Record setting_key = 'demo_update_info'
        $demoInfo = [
            'success' => true,
            'filename' => $newName,
            'url' => $fileUrl,
            'time' => date('Y-m-d H:i:s'),
            'sha256' => $sha256
        ];
        
        $jsonStr = json_encode($demoInfo);
        
        $db = new Database();
        $db->query("SELECT id FROM settings WHERE setting_key = 'demo_update_info'");
        $exists = $db->single();
        if ($exists) {
            $db->query("UPDATE settings SET setting_value = :val WHERE setting_key = 'demo_update_info'");
        } else {
            $db->query("INSERT INTO settings (setting_key, setting_value) VALUES ('demo_update_info', :val)");
        }
        $db->bind(':val', $jsonStr);
        $db->execute();

        $_SESSION['admin_success'] = 'Tải bản Demo thành công! Tên file mới: ' . $newName;
        $this->redirect('/admin');
    }
}
