using Microsoft.Azure.Devices.Common.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;

namespace IoTLibrary.MQTT
{
    public class mqttClient1
    {

        static string TopicDevice2Service;
        static MqttClient client;
        string IoThubURI = "IoTHubCookBook.azure-devices.net";
        string hostURL = "";
        int MqttPort = 8883;

        /// <summary>
        /// Connect and Send data to IoT hub over MQTT protocol
        /// </summary>
        /// <param name="debviceId"></param>
        /// <param name="deviceKey"></param>
        public void SendEvent(string debviceId, string deviceKey)
        {
            string Username = string.Format("{0}/{1}", IoThubURI, debviceId);
            string Password = CreateSharedAccessSignature(debviceId, deviceKey, IoThubURI);

            client = new MqttClient(IoThubURI, MqttPort, true, MqttSslProtocols.TLSv1_0, (sender, certificate, chain, errors) => true, null);
            client.Connect(debviceId, Username, Password);

            double avgWindSpeed = 10; // m/s
            Random rand = new Random();
            double currentWindSpeed = 0;


            int i = 0;
            while (i < 10)
            {
                currentWindSpeed = avgWindSpeed + rand.NextDouble() * 4 - 2;
                var telemetryDataPoint = new
                {
                    deviceId = debviceId,
                    windSpeed = currentWindSpeed,
                    highTemp = 72.3,
                    lowtemp = 11.2,
                    latitude = "17.5122560",
                    longitude = "70.7760470"
                };

                var json = JsonConvert.SerializeObject("Yatish Completed");

                client.Publish(string.Format("devices/{0}/messages/events/telemetry", debviceId), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(json)));



                i++;
            }


        }





        static string CreateSharedAccessSignature(string deviceId, string deviceKey, string iotHubName)
        {
            return new SharedAccessSignatureBuilder
            {
                Key = deviceKey,
                Target = string.Format("{0}/devices/{1}", iotHubName, deviceId),
                KeyName = null,
                TimeToLive = TimeSpan.FromMinutes(20)
            }.ToSignature();
        }


        public void RecievedCommand(string debviceId, string deviceKey)
        {
            string Username = string.Format("{0}/{1}", IoThubURI, debviceId);
            string Password = CreateSharedAccessSignature(debviceId, deviceKey, IoThubURI);

            client = new MqttClient(IoThubURI, MqttPort, true, MqttSslProtocols.TLSv1_0, (sender, certificate, chain, errors) => true, null);
            client.Connect(debviceId, Username, Password);


 
        }




    }
}
