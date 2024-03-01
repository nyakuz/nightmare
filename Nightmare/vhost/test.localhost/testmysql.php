<?php
try{
	$connection = new PDO(
		'mysql:host=mysql;port=3306;dbname=database0;charset=utf8',
		'database',
		'password!@#$',[
		PDO::ATTR_PERSISTENT => true,
		PDO::ATTR_TIMEOUT    => 1,
		PDO::ATTR_ERRMODE    => PDO::ERRMODE_EXCEPTION,
		PDO::MYSQL_ATTR_INIT_COMMAND =>
'SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET time_zone="Asia/Seoul";'
	]);
}catch(PDOException $e){
	return 0;
}

$var1 = 'abcd';
$query = "SELECT NOW(), ?";
$stmt = $connection->prepare($query);
$stmt->bindColumn(1,$out1,PDO::PARAM_STR);
$stmt->bindColumn(2,$out2,PDO::PARAM_STR);
$stmt->bindParam(1,$var1,PDO::PARAM_STR);
$stmt->execute();

while($stmt->fetch(PDO::FETCH_BOUND)){
	echo " $out1 $out2 ";
}