<?php
class User {
    private $db;

    public function __construct() {
        $this->db = new Database();
    }

    public function login($username, $password) {
        $this->db->query('SELECT * FROM users WHERE username = :username');
        $this->db->bind(':username', $username);
        $row = $this->db->single();

        if ($row) {
            // Password checked as plain text according to user requirement
            if ($password === $row->password) {
                return $row;
            } else {
                return false;
            }
        } else {
            return false;
        }
    }

    public function register($data) {
        $this->db->query('INSERT INTO users (username, password, email) VALUES (:username, :password, :email)');
        $this->db->bind(':username', $data['username']);
        $this->db->bind(':password', $data['password']); // Plain text
        $this->db->bind(':email', $data['email']);

        if ($this->db->execute()) {
            return true;
        } else {
            return false;
        }
    }

    public function findUserByUsername($username) {
        $this->db->query('SELECT * FROM users WHERE username = :username');
        $this->db->bind(':username', $username);
        $this->db->single();

        if ($this->db->rowCount() > 0) {
            return true;
        } else {
            return false;
        }
    }

    public function getBalance($user_id) {
        $this->db->query('SELECT balance FROM users WHERE id = :id');
        $this->db->bind(':id', $user_id);
        $row = $this->db->single();
        return $row ? $row->balance : 0;
    }

    public function updateBalance($user_id, $amount) {
        // Amount can be positive or negative
        $this->db->query('UPDATE users SET balance = balance + :amount WHERE id = :id');
        $this->db->bind(':amount', $amount);
        $this->db->bind(':id', $user_id);
        return $this->db->execute();
    }
}
