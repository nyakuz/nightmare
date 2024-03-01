<?php
@ini_set('display_errors', '1');
@ini_set('display_startup_errors', '1');
@error_reporting(E_ALL);

if(!defined('CURL_HTTP_VERSION_3')){
	define('CURL_HTTP_VERSION_3', 30);
}

echo __FILE__.' CURL_HTTP_VERSION_3: '.CURL_HTTP_VERSION_3."\n";

$curl = curl_init();
$url = 'https://cloudflare-quic.com/cdn-cgi/trace';
curl_setopt($curl, CURLOPT_URL, $url);
curl_setopt($curl, CURLOPT_HTTP_VERSION, CURL_HTTP_VERSION_3);
$response = curl_exec($curl);
if(curl_errno($curl)){
	$error = curl_error($curl);
	echo 'cURL error: ' . $error;
}
curl_close($curl);
echo $response;