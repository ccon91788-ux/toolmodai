<?php
class Package {
    private $db;

    public function __construct() {
        $this->db = new Database();
    }

    public function getAllPackages() {
        $this->db->query('SELECT * FROM packages ORDER BY price ASC');
        return $this->db->resultSet();
    }

    public function getPackageById($id) {
        $this->db->query('SELECT * FROM packages WHERE id = :id');
        $this->db->bind(':id', $id);
        return $this->db->single();
    }
}
