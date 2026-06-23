<?php
class ServicesController extends Controller
{
    public function index()
    {
        if (!isset($_SESSION['user_id'])) {
            $this->redirect('/auth/login');
        }

        $userModel = $this->model('User');
        $userBalance = $userModel->getBalance($_SESSION['user_id']);

        require_once '../app/Models/Package.php';
        $packageModel = new Package();
        $packages = $packageModel->getAllPackages();

        require_once '../app/Models/License.php';
        $licenseModel = new License();
        $licenses = $licenseModel->getUserLicenses($_SESSION['user_id']);

        $data = [
            'title' => 'Dịch Vụ & Mua Key',
            'userBalance' => $userBalance,
            'packages' => $packages,
            'licenses' => $licenses,
            'success' => isset($_SESSION['service_success']) ? $_SESSION['service_success'] : '',
            'error' => isset($_SESSION['service_error']) ? $_SESSION['service_error'] : ''
        ];

        unset($_SESSION['service_success']);
        unset($_SESSION['service_error']);

        $this->view('services/index', $data);
    }

    public function buy($package_id = null)
    {
        if (!isset($_SESSION['user_id']) || empty($package_id)) {
            $this->redirect('/services');
        }

        require_once '../app/Models/Package.php';
        $packageModel = new Package();
        $package = $packageModel->getPackageById($package_id);

        if (!$package) {
            $_SESSION['service_error'] = 'Gói không tồn tại.';
            $this->redirect('/services');
        }

        $userModel = $this->model('User');
        $userBalance = $userModel->getBalance($_SESSION['user_id']);

        if ($userBalance < $package->price) {
            $_SESSION['service_error'] = 'Số dư không đủ. Vui lòng nạp thêm tiền.';
            $this->redirect('/services');
        }

        // Deduct balance
        if ($userModel->updateBalance($_SESSION['user_id'], -$package->price)) {
            require_once '../app/Models/License.php';
            $licenseModel = new License();
            $key = $licenseModel->createLicense($_SESSION['user_id'], $package->id, $package->duration_days);

            if ($key) {
                // Log transaction
                $db = new Database();
                $db->query("INSERT INTO transactions (user_id, amount, type, description) VALUES (:uid, :amt, 'buy_key', :desc)");
                $db->bind(':uid', $_SESSION['user_id']);
                $db->bind(':amt', -$package->price);
                $db->bind(':desc', "Mua gói {$package->name} - Key: {$key}");
                $db->execute();

                $_SESSION['service_success'] = "Mua thành công! Key của bạn là: $key";
            }
            else {
                // Refund if failed
                $userModel->updateBalance($_SESSION['user_id'], $package->price);
                $_SESSION['service_error'] = 'Lỗi hệ thống khi tạo key. Đã hoàn tiền.';
            }
        }
        else {
            $_SESSION['service_error'] = 'Lỗi giao dịch trừ tiền.';
        }

        $this->redirect('/services');
    }

    public function reset_hwid($license_id = null)
    {
        if (!isset($_SESSION['user_id']) || empty($license_id)) {
            $this->redirect('/services');
        }

        $db = new Database();
        $db->query("SELECT * FROM licenses WHERE id = :id AND user_id = :uid");
        $db->bind(':id', $license_id);
        $db->bind(':uid', $_SESSION['user_id']);
        $license = $db->single();

        if ($license) {
            $db->query("UPDATE licenses SET bound_hdd = '', bound_mb = '', client_ip = '', reset_count = reset_count + 1, last_reset_at = NOW() WHERE id = :id");
            $db->bind(':id', $license_id);
            $db->execute();

            // Xóa session làm rơi rụng nhịp tim của máy cũ nếu nó đang chạy
            $db->query("DELETE FROM sessions_memory WHERE license_id = :id");
            $db->bind(':id', $license_id);
            $db->execute();

            $_SESSION['service_success'] = 'Đã reset key thành công cho key: ' . $license->license_key . '. Vui lòng đăng nhập lại trên máy mới.';
        }
        else {
            $_SESSION['service_error'] = 'Không tìm thấy key hoặc bạn không có quyền reset key này.';
        }
        $this->redirect('/services');
    }

    public function mykeys()
    {
        $this->redirect('/services'); // Included in index now
    }

    public function extend($license_id = null)
    {
        if (!isset($_SESSION['user_id']) || empty($license_id)) {
            $this->redirect('/services');
        }

        $db = new Database();
        $db->query("SELECT l.*, p.price, p.duration_days, p.name as package_name 
                    FROM licenses l JOIN packages p ON l.package_id = p.id 
                    WHERE l.id = :id AND l.user_id = :uid");
        $db->bind(':id', $license_id);
        $db->bind(':uid', $_SESSION['user_id']);
        $license = $db->single();

        if (!$license) {
            $_SESSION['service_error'] = 'Không tìm thấy key để gia hạn.';
            $this->redirect('/services');
        }

        $userModel = $this->model('User');
        $userBalance = $userModel->getBalance($_SESSION['user_id']);

        if ($userBalance < $license->price) {
            $_SESSION['service_error'] = "Số dư không đủ để gia hạn gói {$license->package_name} (" . number_format($license->price) . " VNĐ).";
            $this->redirect('/services');
        }

        if ($userModel->updateBalance($_SESSION['user_id'], -$license->price)) {
            $current_expiry = strtotime($license->expires_at);
            // Cộng dồn thời gian nếu key còn sống, hoặc cộng từ hôm nay nếu đã hết hạn
            $base_time = ($current_expiry > time()) ? $current_expiry : time();
            $new_expiry = $base_time + ($license->duration_days * 86400);
            $new_date = date('Y-m-d H:i:s', $new_expiry);

            $db->query("UPDATE licenses SET expires_at = :exp, status = 'active' WHERE id = :id");
            $db->bind(':exp', $new_date);
            $db->bind(':id', $license_id);
            $db->execute();

            $db->query("INSERT INTO transactions (user_id, amount, type, description) VALUES (:uid, :amt, 'buy_key', :desc)");
            $db->bind(':uid', $_SESSION['user_id']);
            $db->bind(':amt', -$license->price);
            $db->bind(':desc', "Gia hạn key {$license->license_key} thêm {$license->duration_days} ngày");
            $db->execute();

            $_SESSION['service_success'] = "Đã gia hạn thành công thêm {$license->duration_days} ngày!";
        }
        else {
            $_SESSION['service_error'] = 'Lỗi hệ thống khi trừ tiền.';
        }
        $this->redirect('/services');
    }

    // ── Cập nhật ghi chú (nickname) cho key ──────────────────────────────
    public function update_nickname($license_id = null)
    {
        if (!isset($_SESSION['user_id']) || empty($license_id)) {
            $this->redirect('/services');
        }

        $nickname = trim((string)($_POST['nickname'] ?? ''));
        // Giới hạn 100 ký tự, tránh XSS
        $nickname = mb_substr($nickname, 0, 100);

        $db = new Database();
        $db->query("UPDATE licenses SET nickname = :nick WHERE id = :id AND user_id = :uid");
        $db->bind(':nick', $nickname);
        $db->bind(':id', $license_id);
        $db->bind(':uid', $_SESSION['user_id']);
        $db->execute();

        if ($db->rowCount() > 0) {
            $_SESSION['service_success'] = 'Đã cập nhật ghi chú thành công!';
        } else {
            $_SESSION['service_error'] = 'Không tìm thấy key hoặc không có quyền.';
        }

        $this->redirect('/services');
    }
}
