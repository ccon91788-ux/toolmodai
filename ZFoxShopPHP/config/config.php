<?php
// config/config.php

// Tự động nhận diện BASE_URL linh hoạt cho mọi môi trường (XAMPP/Laragon/cPanel)
$isHttps = false;
if (isset($_SERVER['HTTPS']) && ($_SERVER['HTTPS'] === 'on' || $_SERVER['HTTPS'] == 1)) {
    $isHttps = true;
}
elseif (isset($_SERVER['HTTP_X_FORWARDED_PROTO']) && $_SERVER['HTTP_X_FORWARDED_PROTO'] === 'https') {
    $isHttps = true;
}
elseif (isset($_SERVER['SERVER_PORT']) && $_SERVER['SERVER_PORT'] == 443) {
    $isHttps = true;
}
$protocol = $isHttps ? "https://" : "http://";
$host = isset($_SERVER['HTTP_HOST']) ? $_SERVER['HTTP_HOST'] : 'localhost';
$script_dir = dirname($_SERVER['SCRIPT_NAME']);
$script_dir = str_replace('\\', '/', $script_dir);
if (substr($script_dir, -7) === '/public') {
    $script_dir = substr($script_dir, 0, -7);
}
if ($script_dir === '/')
    $script_dir = '';

define('BASE_URL', $protocol . $host . $script_dir);

// --- DB credentials ---
define('DB_HOST', '103.200.23.120');
define('DB_USER', 'dragons2_nro');
define('DB_PASS', 'Thang259@');
define('DB_NAME', 'dragons2_nro');
