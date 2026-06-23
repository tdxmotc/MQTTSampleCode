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

//MQTT連線位址
$host = 'mqtt.transportdata.tw';

//MQTT連線Port
$port = 8883;

//MQTT連線ClientId，需從TDX網站會員中心取得
$clientID = 'your_clientID';

//MQTT連線Username，需從TDX網站會員中心取得
$username = 'your_userName';

//MQTT連線Password，需從TDX網站會員中心取得
$password = 'your_passWord';

//MQTT連線時的QoS等級
$qos = 1;

$connectionSettings = (new ConnectionSettings)
->setUsername($username)
->setPassword($password);

//訂閱的MQTT頻道
$topics = [
    'v2/Bus/Alert/City/#',
];

$mqtt = new MqttClient($host, $port, $clientID);

//連線至 MQTT 伺服器
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
