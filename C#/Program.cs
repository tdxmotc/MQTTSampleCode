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
        private static IMqttClient _MqttClient;
        private static string Host = "your_host";
        private static int Port = 8083;
        private static string ClientID = "your_clientID";
        private static string UserName = "your_userName";
        private static string Password = "your_passWord";
        private static MQTTnet.Protocol.MqttQualityOfServiceLevel Qos = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce;
        private static MqttClientTlsOptions TlsOptions = new MqttClientTlsOptions() {
            UseTls = false,
            SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
            AllowUntrustedCertificates = true
        };
        private static List<string> Topics = new List<string> 
        { "v2/Bus/RealTimeNearStop/City/Taipei/108", "v2/Bus/RealTimeNearStop/City/Taipei/206" };

        static async Task Main(string[] args)
        {
            Console.WriteLine("MQTT Initialzing...");
            await Initial();

            // 讓應用程式不結束，等待推播事件
            var cts = new CancellationTokenSource();
            await Task.Delay(Timeout.Infinite, cts.Token);
        }

        /// <summary>初始化設定與連線</summary>
        private static async Task Initial()
        {
            // 設定 Options
            var Options = new MqttClientOptionsBuilder()
                .WithClientId(ClientID)
                .WithTcpServer(Host, Port)
                .WithCredentials(UserName, Password)
                .WithCleanSession()
                .WithTlsOptions(TlsOptions)
                .WithWillQualityOfServiceLevel(Qos)
                .Build();

            // 建立 MqttClient、加入事件
            _MqttClient = new MqttClientFactory().CreateMqttClient();
            _MqttClient.ConnectedAsync += ConnectedHandle;        // 加入伺服器連線事件
            _MqttClient.DisconnectedAsync += DisconnectedHandle;  // 加入伺服器斷線事件
            _MqttClient.ApplicationMessageReceivedAsync += ApplicationMessageReceivedHandle;  // 加入收到推播事件

            // 連線至伺服器
            await _MqttClient.ConnectAsync(Options);  

            // 訂閱頻道
            await SubscribeTopic();
            Console.WriteLine("==========================================================================");
        }


        /// <summary>訂閱頻道</summary>
        private static async Task SubscribeTopic()
        {
            if (_MqttClient.IsConnected)
            {
                Topics.ForEach(async Topic => {
                    var TopicFilter = new MqttTopicFilterBuilder().WithTopic(Topic).Build();
                    await _MqttClient.SubscribeAsync(TopicFilter, CancellationToken.None);  // 訂閱該頻道
                    Console.WriteLine($"Subscribing to topic: {Topic}");
                });
            }
        }

        /// <summary>連線事件</summary>
        private static Task ConnectedHandle(MqttClientConnectedEventArgs arg)
        {
            Console.WriteLine("Connected to MQTT Broker.");
            return Task.CompletedTask;
        }

        /// <summary>斷線事件</summary>
        private static async Task DisconnectedHandle(MqttClientDisconnectedEventArgs arg)
        {
            const int TimeForWait = 10;
            Console.WriteLine("Connection has been interrupted");
            Console.WriteLine($"Reconnecting in 10 seconds...");
            await Task.Delay(TimeSpan.FromSeconds(TimeForWait));
            await _MqttClient.ReconnectAsync(); // 當發生斷線事件時，自動重新連線，無須重新訂閱Topic
        }

        /// <summary>接收到推播事件</summary>
        private static Task ApplicationMessageReceivedHandle(MqttApplicationMessageReceivedEventArgs arg)
        {
            Console.WriteLine($"Received message on topic: [{arg.ApplicationMessage.Topic}]");
            Console.WriteLine($"Message: {Encoding.UTF8.GetString(arg.ApplicationMessage.Payload.ToArray())}");
            Console.WriteLine("==========================================================================");
            return Task.CompletedTask;
        }
    }
}
