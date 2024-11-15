<?php
// 請先安裝 composer。下載網址: https://getcomposer.org/download/
// 新建一個資料夾後於該目錄下開啟cmd
// 輸入安裝指令: composer require php-mqtt/client
// 此目錄下會產生一個 vendor 資料夾
// 於此目錄下新增一個 php 檔案，並複製以下程式碼

require_once 'vendor/autoload.php';  // 請確保已經安裝了相關的套件，並將 autoload 檔案的路徑更新為正確的位置

use PhpMqtt\Client\MqttClient;
use PhpMqtt\Client\ConnectionSettings;
use PhpMqtt\Client\Exceptions\MqttClientException;
use PhpMqtt\Client\Exceptions\DataTransferException;
use PhpMqtt\Client\Exceptions\ConnectingToBrokerFailedException;

$host = 'your_host';
$port = 8083;
$clientID = 'your_clientID';
$username = 'your_userName';
$password = 'your_passWord';
$qos = 1;  // QoS (服務品質) 的等級，可以是 0、1 或 2
$connectionSettings = (new ConnectionSettings)
->setUsername($username)
->setPassword($password);
$topics = [
    'v2/Bus/RealTimeNearStop/City/Taipei/108',
    'v2/Bus/RealTimeNearStop/City/Taipei/206',
];
$mqtt = new MqttClient($host, $port, $clientID);

// 連線至 MQTT 伺服器
connectToMqtt($mqtt, $topics, $qos, $connectionSettings);

// =======================================================
// =======================================================
// 連線至 MQTT 伺服器
function connectToMqtt($mqtt, $topics, $qos, $connectionSettings) {
    try {
        $mqtt->connect($connectionSettings, true);
        echo "Connected to MQTT broker.\n";
        subscribeTopics($mqtt, $topics, $qos);
        $mqtt->loop(true);
        $mqtt->disconnect();
    }
    catch (ConnectingToBrokerFailedException $e){
        echo "Failed to connect to MQTT broker: {$e->getMessage()}\n";
        Reconnect($mqtt, $topics, $qos, $connectionSettings);
    } catch (DataTransferException $e) {
        echo "Connection has been interrupted: {$e->getMessage()}\n";
        Reconnect($mqtt, $topics, $qos, $connectionSettings);
    } catch (MqttClientException $e) {
        echo "MQTT client error: {$e->getMessage()}\n";
    }
}

// 訂閱Topic
function subscribeTopics($mqtt, $topics, $qos) {
    foreach ($topics as $topic) {
        echo "Subscribing to topic: $topic\n";
        $mqtt->subscribe($topic, function ($topic, $message) {
            echo sprintf("Received message on topic: [%s]\n", $topic);
            echo "Message:". $message . "\n";
            echo "=====================================================================\n";
        }, $qos);
    }
}

// 重新連線
function Reconnect($mqtt, $topics, $qos, $connectionSettings){
    echo "Reconnecting in 10 seconds...\n";
    sleep(10); // 等待10秒
    connectToMqtt($mqtt, $topics, $qos, $connectionSettings);
}

?>