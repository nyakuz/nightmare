﻿<?php
ini_set('max_execution_time', '5');
ini_set('display_errors', '1');
ini_set('display_startup_errors', '1');
error_reporting(E_ALL);
$body = file_get_contents('php://input');
printf($body);
