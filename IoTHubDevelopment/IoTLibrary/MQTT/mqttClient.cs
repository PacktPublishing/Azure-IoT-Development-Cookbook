using System;
using System.Text;
using Microsoft.Azure.Devices.Common.Security;
using Newtonsoft.Json;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace IoTLibrary.MQTT
{
    public class MqttClass: IMqttClient
    {
        private readonly string _ioThubUri = "IoTHubCookBook.azure-devices.net";

        private int MqttPort1 { get; } = 8883;

        private MqttClient Client { get; set; }

        /// <summary>
        ///     Connect and Send data to IoT hub over MQTT protocol
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="deviceKey"></param>
        public void SendEvent(string deviceId, string deviceKey)
        {
            string target = $"{_ioThubUri}/devices/{deviceId}";
            string username = $"{_ioThubUri}/{deviceId}";
            var password = CreateSharedAccessSignature(deviceKey, target);

            Client = new MqttClient(_ioThubUri, MqttPort1, true, MqttSslProtocols.TLSv1_0,
                (sender, certificate, chain, errors) => true, null);
            Client.Connect(deviceId, username, password);

            double avgWindSpeed = 10; // m/s
            var rand = new Random();


            var i = 0;
            while (i < 10)
            {
                var currentWindSpeed = avgWindSpeed + rand.NextDouble()*4 - 2;
                var telemetryDataPoint = new
                {
                    deviceId,
                    windSpeed = currentWindSpeed,
                    highTemp = 72.3,
                    lowtemp = 11.2,
                    latitude = "17.5122560",
                    longitude = "70.7760470"
                };

                var json = JsonConvert.SerializeObject(telemetryDataPoint);

                Client.Publish(string.Format("devices/{0}/messages/events/telemetry", deviceId),
                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(json)));


                i++;
            }
        }


        public void RecievedCommand(string deviceId, string deviceKey)
        {
            string target = $"{_ioThubUri}/devices/{deviceId}/messages/devicebound/#";
            string username = $"{_ioThubUri}/{deviceId}";
            var password = CreateSharedAccessSignature(deviceKey, target);

            Client = new MqttClient(_ioThubUri, MqttPort1, true, MqttSslProtocols.TLSv1_0,
                (sender, certificate, chain, errors) => true, null);


            Client.MqttMsgSubscribed += client_MqttMsgSubscribed;
            Client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            Client.Connect(deviceId, username, password);

            Client.Subscribe(new[] {target}, new[] {MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE});
            Client.Subscribe(new[] {"/#"}, new[] {MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE});
        }


        private static string CreateSharedAccessSignature(string deviceKey, string target)
        {
            return new SharedAccessSignatureBuilder
            {
                Key = deviceKey,
                Target = target,
                KeyName = null,
                TimeToLive = TimeSpan.FromMinutes(20)
            }.ToSignature();
        }

        private void client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            // handle message received
            var abc = ""; // Encoding.UTF8.GetString(e.Message);
            Console.WriteLine(abc);
        }

        private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            Console.WriteLine("Received = " + Encoding.UTF8.GetString(e.Message) + " on topic " + e.Topic);
        }
    }
}