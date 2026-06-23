<?php
class Controller {
    public function model($model) {
        require_once '../app/Models/' . $model . '.php';
        return new $model();
    }

    public function view($view, $data = []) {
        $contentView = '../app/Views/' . $view . '.php';
        if (file_exists('../app/Views/layout.php')) {
            require_once '../app/Views/layout.php';
        } else {
            if (file_exists($contentView)) {
                require_once $contentView;
            } else {
                die("View does not exist: $view");
            }
        }
    }
    
    public function redirect($path) {
        header('Location: ' . rtrim(BASE_URL, '/') . '/' . ltrim($path, '/'));
        exit();
    }
}
