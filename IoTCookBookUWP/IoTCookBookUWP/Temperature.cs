using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;
using Sensors.Dht;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;

namespace IoTCookBookUWP
{
    public class Temperature
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        readonly Dht11 Dht;
        static DeviceClient _deviceClient;
        readonly string iotHubUri = "IoTHubCookBook.azure-devices.net";
        readonly string deviceName = "myFirstDevice";
        readonly string deviceKey = "LKCXsBKMKISTjr3ii08UXgIpELxy8/38EiMuxNAiqek=";

        public Temperature()
        {
            var pin = GpioController.GetDefault().OpenPin(4, GpioSharingMode.Exclusive);
            Dht = new Dht11(pin, GpioPinDriveMode.Input);

            _timer.Tick += (sender, o) =>
            {
                //  Get Temperature and Humidity Reading 
                GetReading();
            };
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Start();

            try
            {
                _deviceClient = DeviceClient.Create(iotHubUri,
                    new DeviceAuthenticationWithRegistrySymmetricKey(deviceName, deviceKey), TransportType.Http1);
            }
            catch (Exception)
            {
                //nothing
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetReading()
        {
            try
            {
                DhtReading reading = new DhtReading();
                if (reading.IsValid)
                {
                    var temperature = Convert.ToSingle(reading.Temperature);
                    var humidity = Convert.ToSingle(reading.Humidity);
                    var lastUpdated = DateTimeOffset.Now;
                    try
                    {
                        SendDeviceToCloudMessagesAsync(humidity.ToString(CultureInfo.InvariantCulture),
                            temperature.ToString(CultureInfo.InvariantCulture), lastUpdated, "");
                    }
                    catch (Exception)
                    {
                        // some code to handle exception
                    }
                }
            }
            catch (Exception)
            {
                // some code to handle exception
            }
        }

        /// <summary>
        /// Send the Telemetry data to cloud with temperature & humidity
        /// </summary>
        /// <param name="humidityDisplay"></param>
        /// <param name="temperatureDisplay"></param>
        /// <param name="lastUpdated"></param>
        /// <param name="deviceName"></param>
        private static async void SendDeviceToCloudMessagesAsync(string humidityDisplay, string temperatureDisplay,
            DateTimeOffset lastUpdated, string deviceName)
        {
            var telemetryDataPoint = new
            {
                deviceId = deviceName,
                time = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                humidity = humidityDisplay,
                temperature = temperatureDisplay,
                LastReadTime = lastUpdated
            };
            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));
            await _deviceClient.SendEventAsync(message);
        }
    }
}