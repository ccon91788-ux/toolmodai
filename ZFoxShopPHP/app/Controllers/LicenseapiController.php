<?php
require_once __DIR__ . '/../Core/JwtHelper.php';

class LicenseapiController extends Controller
{
    private const DEFAULT_JWT_SECRET = 'ZFox_Super_Secret_Dragon_Key_2026_@!';
    private const TOKEN_LIFETIME     = 2592000; // 30 Ngày
    private const RATE_LIMIT_MAX     = 5;
    private const RATE_LIMIT_WINDOW  = 60;
    private const RATE_LIMIT_BLOCK   = 900;

    private static function getJwtSecret()
    {
        $envSecret = $_ENV['ZFOX_JWT_SECRET'] ?? getenv('ZFOX_JWT_SECRET');
        return (is_string($envSecret) && trim($envSecret) !== '') ? trim($envSecret) : self::DEFAULT_JWT_SECRET;
    }

    private static function getExpectedPanelHash()
    {
        $envHash = $_ENV['ZFOX_PANEL_HASH'] ?? getenv('ZFOX_PANEL_HASH');
        return (is_string($envHash) && trim($envHash) !== '') ? strtolower(trim($envHash)) : '';
    }

    private function checkRateLimit()
    {
        // Yêu cầu user: Bỏ block hoặc hạn chế ở web đi. 
        // -> Luôn trả về true, tắt rate limiting / IP blocking.
        return true;
    }

    private function setCorsHeaders()
    {
        header('Content-Type: application/json; charset=utf-8');
        header('Access-Control-Allow-Origin: *');
        header('Access-Control-Allow-Methods: POST, GET, OPTIONS');
        header('Access-Control-Allow-Headers: Content-Type, Authorization');
    }

    // ── 1. Challenge ──────────────────────────────────────────────────────
    public function challenge()
    {
        $this->setCorsHeaders();
        if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') exit;
        if (!$this->checkRateLimit()) return;

        $nonce      = bin2hex(random_bytes(16));
        $session_id = bin2hex(random_bytes(32));

        $db = new Database();
        $db->query("INSERT INTO sessions_memory (session_id, license_id, hwid, nonce, last_beat) VALUES (:sid, 0, '', :nonce, NOW())");
        $db->bind(':sid',  $session_id);
        $db->bind(':nonce', $nonce);

        try {
            $db->execute();
            echo json_encode(['success' => true, 'session_id' => $session_id, 'nonce' => $nonce]);
        } catch (Exception $e) {
            echo json_encode(['success' => false, 'message' => 'Challenge creation failed']);
        }
    }

    // ── 2. Login ──────────────────────────────────────────────────────────
    public function login()
    {
        $this->setCorsHeaders();
        if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') exit;
        if (!$this->checkRateLimit()) return;

        $data       = json_decode(file_get_contents('php://input'), true) ?: $_POST;
        $session_id = trim((string)($data['session_id']  ?? ''));
        $licenseKey = trim((string)($data['license_key'] ?? ''));
        $clientHash = strtolower(trim((string)($data['client_hash'] ?? '')));
        // Bỏ nhận $hwid vì đã thay bằng hdd+mb
        $hddId      = trim((string)($data['hdd_id'] ?? ''));
        $mbId       = trim((string)($data['mb_id']  ?? ''));
        $ipv4       = trim((string)($data['ipv4']   ?? ''));
        $clientIp   = $_SERVER['REMOTE_ADDR'] ?? '0.0.0.0';

        if (empty($session_id) || empty($licenseKey) || empty($clientHash)) {
            http_response_code(400);
            echo json_encode(['success' => false, 'message' => 'Missing required fields']);
            return;
        }

        $db = new Database();

        // Xác thực session
        $db->query("SELECT * FROM sessions_memory WHERE session_id = :sid");
        $db->bind(':sid',  $session_id);
        $session = $db->single();

        if (!$session) {
            http_response_code(401);
            echo json_encode(['success' => false, 'message' => 'Invalid session']);
            return;
        }

        $nonce = $session->nonce;

        // Tìm license
        $db->query("SELECT * FROM licenses WHERE license_key = :key");
        $db->bind(':key', $licenseKey);
        $license = $db->single();

        if (!$license) { http_response_code(401); echo json_encode(['success' => false, 'message' => 'Invalid license key']); return; }
        if ($license->status !== 'active') { http_response_code(403); echo json_encode(['success' => false, 'message' => 'License is ' . $license->status]); return; }
        if (strtotime($license->expires_at) < time()) { http_response_code(403); echo json_encode(['success' => false, 'message' => 'License expired']); return; }

        // Bỏ qua check HWID lock cũ (vì cột hardware_id đã xóa)

        // ── Kiểm tra binding HDD + MB + IPv4 (device_limit cứng = 1) ──
        // Tạm thời bỏ check HDD/MB theo yêu cầu
        $alreadyBound = !empty($license->client_ip);
        if ($alreadyBound) {
            $ipMatch  = ($license->client_ip === $ipv4);

            if (!$ipMatch) {
                http_response_code(403);
                echo json_encode(['success' => false, 'message' => 'Thông số IP không khớp. Liên hệ admin để reset.']);
                return;
            }
        }

        // Xác thực HMAC signature
        $expectedHash = hash_hmac('sha256', $nonce, $license->license_key);
        if (!hash_equals($expectedHash, $clientHash)) {
            http_response_code(401);
            echo json_encode(['success' => false, 'message' => 'Integrity verification failed']);
            return;
        }

        // Xác thực file hash
        $expectedPanelHash = self::getExpectedPanelHash();
        if ($expectedPanelHash !== '') {
            $clientFileHash = strtolower(trim((string)($data['client_file_hash'] ?? '')));
            if ($clientFileHash !== $expectedPanelHash) {
                http_response_code(401);
                echo json_encode(['success' => false, 'message' => 'Integrity verification failed. Modified or unrecognized client.']);
                return;
            }
        }

        // ── Bind lần đầu: lưu hardware fingerprint ──
        if (!$alreadyBound) {
            $db->query("UPDATE licenses SET bound_hdd = :hdd, bound_mb = :mb, client_ip = :ip WHERE id = :id");
            $db->bind(':hdd',  $hddId);
            $db->bind(':mb',   $mbId);
            $db->bind(':ip',   $ipv4);
            $db->bind(':id',   $license->id);
            $db->execute();
        }

        // FIX (Anti-Cheat): Khi 1 máy login thành công, phải xóa toàn bộ các session cũ của máy khác
        // để những máy đó (dù chưa qua 15p) cũng sẽ bị rớt nhịp tim ngay lập tức.
        $db->query("DELETE FROM sessions_memory WHERE license_id = :lid AND session_id != :sid");
        $db->bind(':lid', $license->id);
        $db->bind(':sid', $session_id);
        $db->execute();

        // Lấy tên khách hàng
        $customerName = 'Khách';
        $db->query("SELECT username FROM users WHERE id = :uid");
        $db->bind(':uid', $license->user_id);
        $userRow = $db->single();
        if ($userRow) $customerName = $userRow->username;

        // Phát JWT
        $payload   = ['sub' => $license->id, 'key' => $license->license_key, 'sid' => $session_id, 'iat' => time(), 'exp' => time() + self::TOKEN_LIFETIME];
        $jwtToken  = JwtHelper::encode($payload, self::getJwtSecret());

        // Cập nhật sessions_memory
        $db->query("UPDATE sessions_memory SET license_id = :lid, last_beat = NOW() WHERE session_id = :sid");
        $db->bind(':lid', $license->id);
        $db->bind(':sid', $session_id);
        $db->execute();

        echo json_encode([
            'success'            => true,
            'token'              => $jwtToken,
            'expires_in'         => self::TOKEN_LIFETIME,
            'customer_name'      => $customerName,
            'license_expires_at' => $license->expires_at,
            'payload_secret'     => base64_encode('{"Auto_Offset_X": 1054, "Zone_Bypass": true}')
        ]);
    }

    // ── 3. Startup Check (mỗi lần mở panel) ──────────────────────────────
    public function startup_check()
    {
        $this->setCorsHeaders();
        if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') exit;
        if (!$this->checkRateLimit()) return;

        $data       = json_decode(file_get_contents('php://input'), true) ?: $_POST;
        $licenseKey = trim((string)($data['license_key'] ?? ''));
        $hddId      = trim((string)($data['hdd_id']     ?? ''));
        $mbId       = trim((string)($data['mb_id']      ?? ''));
        $ipv4       = trim((string)($data['ipv4']        ?? ''));
        $clientIp   = $_SERVER['REMOTE_ADDR'] ?? '0.0.0.0';

        if (empty($licenseKey) || empty($hddId) || empty($mbId)) {
            http_response_code(400);
            echo json_encode(['success' => false, 'message' => 'Missing required fields']);
            return;
        }

        $db = new Database();
        $db->query("SELECT * FROM licenses WHERE license_key = :key");
        $db->bind(':key', $licenseKey);
        $license = $db->single();

        if (!$license) { http_response_code(401); echo json_encode(['success' => false, 'message' => 'Key không hợp lệ.']); return; }
        if ($license->status !== 'active') { http_response_code(403); echo json_encode(['success' => false, 'message' => 'Key bị khóa: ' . $license->status]); return; }
        if (strtotime($license->expires_at) < time()) { http_response_code(403); echo json_encode(['success' => false, 'message' => 'Key đã hết hạn.']); return; }

        $alreadyBound = !empty($license->client_ip); // Tạm thời bỏ check HDD/MB theo yêu cầu

        if ($alreadyBound) {
            // Validate IPv4 phải khớp, bỏ qua HDD/MB
            $ipMatch  = ($license->client_ip === $ipv4);

            if (!$ipMatch) {
                http_response_code(403);
                echo json_encode(['success' => false, 'message' => 'Thông số IP không khớp. Liên hệ admin để reset.']);
                return;
            }

            // (Bỏ cập nhật heartbeat vì cột last_heartbeat đã bị xóa)
        } else {
            // Chưa bind → bind lần đầu
            $db->query("UPDATE licenses SET bound_hdd = :hdd, bound_mb = :mb, client_ip = :ip WHERE id = :id");
            $db->bind(':hdd',  $hddId);
            $db->bind(':mb',   $mbId);
            $db->bind(':ip',   $ipv4);
            $db->bind(':id',   $license->id);
            $db->execute();
        }

        // Lấy tên khách hàng
        $customerName = 'Khách';
        $db->query("SELECT username FROM users WHERE id = :uid");
        $db->bind(':uid', $license->user_id);
        $userRow = $db->single();
        if ($userRow) $customerName = $userRow->username;

        echo json_encode([
            'success'            => true,
            'customer_name'      => $customerName,
            'license_expires_at' => $license->expires_at,
        ]);
    }

    // ── 4. Heartbeat ──────────────────────────────────────────────────────
    public function heartbeat()
    {
        $this->setCorsHeaders();
        if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') exit;

        $headers    = function_exists('apache_request_headers') ? apache_request_headers() : [];
        $authHeader = $headers['Authorization'] ?? $headers['authorization'] ?? ($_SERVER['HTTP_AUTHORIZATION'] ?? '');

        if (empty($authHeader) || !preg_match('/Bearer\s(\S+)/', $authHeader, $matches)) {
            http_response_code(401);
            echo json_encode(['success' => false, 'message' => 'No Authorization token found']);
            return;
        }

        $decoded = JwtHelper::decode($matches[1], self::getJwtSecret());
        if (!$decoded) {
            http_response_code(401);
            echo json_encode(['success' => false, 'message' => 'Token expired or invalid. Please relogin.']);
            return;
        }

        // Xác thực file hash
        $expectedPanelHash = self::getExpectedPanelHash();
        if ($expectedPanelHash !== '') {
            $clientFileHash = strtolower(trim($headers['X-Client-File-Hash'] ?? $headers['x-client-file-hash'] ?? ($_SERVER['HTTP_X_CLIENT_FILE_HASH'] ?? '')));
            if ($clientFileHash !== $expectedPanelHash) {
                http_response_code(401);
                echo json_encode(['success' => false, 'message' => 'Integrity verification failed.']);
                return;
            }
        }

        $db = new Database();

        // Kiểm tra Hạn Sử Dụng và Trạng thái của Key
        $db->query("SELECT l.status, l.expires_at FROM sessions_memory sm JOIN licenses l ON sm.license_id = l.id WHERE sm.session_id = :sid");
        $db->bind(':sid', $decoded['sid']);
        $lic = $db->single();

        if (!$lic || $lic->status !== 'active') {
            http_response_code(401);
            echo json_encode(['success' => false, 'message' => 'License banned!']);
            return;
        }

        if (strtotime($lic->expires_at) < time()) {
            http_response_code(401);
            echo json_encode(['success' => false, 'message' => 'License expired!']);
            return;
        }

        $db->query("UPDATE sessions_memory SET last_beat = NOW() WHERE session_id = :sid");
        $db->bind(':sid', $decoded['sid']);
        $db->execute();

        if ($db->rowCount() === 0) {
            http_response_code(401);
            echo json_encode(['success' => false, 'message' => 'Session disconnected or banned']);
            return;
        }

        echo json_encode(['success' => true, 'message' => 'Heartbeat received']);
    }
}
