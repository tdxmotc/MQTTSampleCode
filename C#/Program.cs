// 本範例使用之套件為 MQTTnet。網址: https://github.com/dotnet/MQTTnet
// 版本 : 5.1.0.1559
// 請先透過 NuGet 安裝此套件 !!

using MQTTnet;
using System.Buffers;
using System.Text;

namespace TDX_MQTT
{
    internal class Program
    {
        //MQTT Client
        private static IMqttClient mqttClient;

        //MQTT連線位址
        private static string mqttHost = "mqtt.transportdata.tw";

        //MQTT連線Port
        private static int mqttPort = 8883;

        //MQTT連線ClientId，需從TDX網站會員中心取得
        private static string mqttClientID = "your_clientID"; 
        
        //MQTT連線Username，需從TDX網站會員中心取得
        private static string mqttUserName = "your_userName"; 
        
        //MQTT連線Password，需從TDX網站會員中心取得
        private static string mqttPassword = "your_passWord"; 

        //MQTT連線時的QoS等級
        private static MQTTnet.Protocol.MqttQualityOfServiceLevel mqttQos = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce;

        //MQTT連線時的TLS設定
        private static MqttClientTlsOptions mqttTlsOptions = new MqttClientTlsOptions()
        {
            UseTls = true,
            SslProtocol = System.Security.Authentication.SslProtocols.Tls13,
            AllowUntrustedCertificates = false
        };

        //訂閱的MQTT頻道
        private static List<string> mqttTopics = new List<string>
        { "v2/Bus/Alert/City/#" };

        static async Task Main(string[] args)
        {
            Console.WriteLine("MQTT Initialzing...");
            await Initial();

            //讓應用程式不結束，等待推播事件
            var cts = new CancellationTokenSource();
            await Task.Delay(Timeout.Infinite, cts.Token);
        }

        /// <summary>初始化設定與連線</summary>
        private static async Task Initial()
        {
            //設定連線參數
            var options = new MqttClientOptionsBuilder()
                .WithClientId(mqttClientID)
                .WithTcpServer(mqttHost, mqttPort)
                .WithCredentials(mqttUserName, mqttPassword)
                .WithCleanSession()
                .WithTlsOptions(mqttTlsOptions)
                .WithWillQualityOfServiceLevel(mqttQos)
                .Build();

            //建立MqttClient、加入事件
            mqttClient = new MqttClientFactory().CreateMqttClient();
            mqttClient.ConnectedAsync += ConnectedHandle;        //伺服器連線事件
            mqttClient.DisconnectedAsync += DisconnectedHandle;  //伺服器斷線事件
            mqttClient.ApplicationMessageReceivedAsync += ApplicationMessageReceivedHandle;  //接收到資料事件

            //連線至伺服器
            await mqttClient.ConnectAsync(options);  
        }

        /// <summary>連線事件</summary>
        private static async Task ConnectedHandle(MqttClientConnectedEventArgs arg)
        {
            Console.WriteLine("Connected to MQTT Broker.");

            //成功連線後，訂閱MQTT頻道
            await SubscribeTopic();
        }

        /// <summary>斷線事件</summary>
        private static async Task DisconnectedHandle(MqttClientDisconnectedEventArgs arg)
        {
            const int timeForWait = 10;
            Console.WriteLine($"Connection has been interrupted: {arg.ConnectResult.ResultCode}");
            Console.WriteLine($"Reconnecting in 10 seconds...");
            await Task.Delay(TimeSpan.FromSeconds(timeForWait));

            //當發生斷線事件時重新連線，無須重新訂閱Topic
            await mqttClient.ReconnectAsync();
        }

        /// <summary>訂閱頻道</summary>
        private static async Task SubscribeTopic()
        {
            if (mqttClient.IsConnected)
            {
                //訂閱頻道
                mqttTopics.ForEach(async topic => {
                    var topicFilter = new MqttTopicFilterBuilder().WithTopic(topic).Build();
                    await mqttClient.SubscribeAsync(topicFilter, CancellationToken.None);  
                    Console.WriteLine($"Subscribed to topic: {topic}");
                });
            }
        }

        /// <summary>接收到資料事件</summary>
        private static Task ApplicationMessageReceivedHandle(MqttApplicationMessageReceivedEventArgs arg)
        {
            Console.WriteLine($"Received message on topic: [{arg.ApplicationMessage.Topic}]");
            Console.WriteLine($"Message: {Encoding.UTF8.GetString(arg.ApplicationMessage.Payload.ToArray())}");
            Console.WriteLine("==========================================================================");
            return Task.CompletedTask;
        }
    }
}
