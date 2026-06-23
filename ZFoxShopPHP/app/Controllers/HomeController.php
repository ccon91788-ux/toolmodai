<?php
class HomeController extends Controller {
    public function index() {
        // Prepare some data
        $data = [
            'title' => 'Trang chủ - ZFoxShop'
        ];
        $this->view('home/index', $data);
    }
}
