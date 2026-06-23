<?php
// public/index.php
session_start();
require_once '../config/config.php';
require_once '../app/Core/App.php';
require_once '../app/Core/Controller.php';
require_once '../app/Core/Database.php';

$app = new App();
