<?php
class AuthController extends Controller {
    public function login() {
        if ($_SERVER['REQUEST_METHOD'] == 'POST') {
            $username = trim($_POST['username']);
            $password = $_POST['password'];

            $userModel = $this->model('User');
            $loggedInUser = $userModel->login($username, $password);

            if ($loggedInUser) {
                // Tạo session
                $_SESSION['user_id'] = $loggedInUser->id;
                $_SESSION['username'] = $loggedInUser->username;
                $_SESSION['user_role'] = $loggedInUser->role;
                
                $this->redirect('');
            } else {
                $data = [
                    'title' => 'Đăng nhập',
                    'error' => 'Tài khoản hoặc mật khẩu không chính xác'
                ];
                $this->view('auth/login', $data);
            }
        } else {
            $data = [
                'title' => 'Đăng nhập - ZFoxShop'
            ];
            $this->view('auth/login', $data);
        }
    }

    public function register() {
        if ($_SERVER['REQUEST_METHOD'] == 'POST') {
            $dataForm = [
                'username' => trim($_POST['username']),
                'password' => $_POST['password'],
                'email' => trim($_POST['email'])
            ];

            $userModel = $this->model('User');

            if ($userModel->findUserByUsername($dataForm['username'])) {
                $data = [
                    'title' => 'Đăng ký',
                    'error' => 'Tên đăng nhập đã tồn tại'
                ];
                $this->view('auth/register', $data);
            } else {
                if ($userModel->register($dataForm)) {
                    $this->redirect('/auth/login');
                } else {
                    $data = [
                        'title' => 'Đăng ký',
                        'error' => 'Đã có lỗi xảy ra. Thử lại sau.'
                    ];
                    $this->view('auth/register', $data);
                }
            }
        } else {
            $data = [
                'title' => 'Đăng ký - ZFoxShop'
            ];
            $this->view('auth/register', $data);
        }
    }

    public function logout() {
        unset($_SESSION['user_id']);
        unset($_SESSION['username']);
        unset($_SESSION['user_role']);
        session_destroy();
        $this->redirect('/auth/login');
    }
}
