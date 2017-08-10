using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace IoTLibrary.Devices
{
    public class DeviceSimulator
    {
        private readonly DeviceClient _deviceClient;


        public DeviceSimulator(DeviceClient deviceClient)
        {
            _deviceClient = deviceClient;
        }


        public async void SendDeviceToCloudMessagesAsync(string deviceId)
        {
            double avgWindSpeed = 10; // m/s
            var rand = new Random();

            var i = 0;

            while (true)
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
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await _deviceClient.SendEventAsync(message);
                i += 1;
               // await Task.Delay(1000);
            }
        }


        public async void ReceiveC2DAsync()
        {
            while (true)
            {
                var receivedMessage = await _deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;

                Encoding.ASCII.GetString(receivedMessage.GetBytes());

                // take action based on cmdMessage value
                // ..... Some code here .....


                //Send Acknowledgment to IoT Hub
                await _deviceClient.CompleteAsync(receivedMessage);
            }
        }


        public Task<MethodResponse> WriteToMessage(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine();
            Console.WriteLine("\t{0}", methodRequest.DataAsJson);
            Console.WriteLine();

            return Task.FromResult(new MethodResponse(new byte[0], 200));
        }

        public Task<MethodResponse> GetDeviceName(MethodRequest methodRequest, object userContext)
        {
            MethodResponse retValue = null;
            if (userContext == null)
            {
                retValue = new MethodResponse(new byte[1], 500);
            }
            else
            {
                var d = userContext as DeviceData;
                if (d != null)
                {
                    var result = "{'name':'" + d.Name + "'}";
                    retValue = new MethodResponse(Encoding.UTF8.GetBytes(result), 200);
                }
            }
            return Task.FromResult(retValue);
        }


        private class DeviceData
        {
            public DeviceData(string myName)
            {
                Name = myName;
            }

            public string Name { get; }
        }
    }
}