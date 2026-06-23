<?php
class License {
    private $db;

    public function __construct() {
        $this->db = new Database();
    }

    public function createLicense($user_id, $package_id, $duration_days) {
        // ZFOX-XXXXXXXXXXXX
        $key = 'ZFOX-' . strtoupper(substr(md5(uniqid('', true)), 0, 12));
        $expires = date('Y-m-d H:i:s', strtotime("+$duration_days days"));

        $this->db->query('INSERT INTO licenses (user_id, license_key, package_id, expires_at) VALUES (:user_id, :key, :package_id, :expires)');
        $this->db->bind(':user_id', $user_id);
        $this->db->bind(':key', $key);
        $this->db->bind(':package_id', $package_id);
        $this->db->bind(':expires', $expires);

        if ($this->db->execute()) {
            return $key;
        }
        return false;
    }

    public function getUserLicenses($user_id) {
        $this->db->query('SELECT l.*, p.name as package_name FROM licenses l JOIN packages p ON l.package_id = p.id WHERE l.user_id = :user_id ORDER BY l.id DESC');
        $this->db->bind(':user_id', $user_id);
        return $this->db->resultSet();
    }
}
