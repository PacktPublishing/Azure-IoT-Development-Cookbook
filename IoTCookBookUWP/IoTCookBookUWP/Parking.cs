using System;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace IoTCookBookUWP
{

    public class Parking
    {
        const int IntEchoPin = 24;
        const int IntTriggerPin = 23;
        private GpioPin _pinEcho;
        GpioPin _pinTrigger;
        static DeviceClient _deviceClient;
        readonly string iotHubUri = "IoTHubCookBook.azure-devices.net";
        readonly string deviceName = "myFirstDevice";
        readonly string deviceKey = "LKCXsBKMKISTjr3ii08UXgIpELxy8/38EiMuxNAiqek=";
        readonly DispatcherTimer _timer = new DispatcherTimer();

        public Parking()
        {
            InitGpio();            
            _timer.Tick += (sender, o) =>
            {
                var distance = GetDistance();
                var s = $"Distance : {distance} cm";
                Debug.WriteLine(s);

                // Send Data to IoT Hub based on the Distance Calculated 
                SendDeviceToCloudMessagesAsync(deviceName, DateTimeOffset.Now, distance >= 14);
            };

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Start();

            try
            {
                _deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceName, deviceKey), TransportType.Http1);
            }
            catch (Exception)
            {
                //nothing
            }
        }

        private double GetDistance()
        {
            // Set the trigger 
            _pinTrigger.Write(GpioPinValue.High);
            Task.Delay(TimeSpan.FromMilliseconds(0.01));
            _pinTrigger.Write(GpioPinValue.Low);

            // Find out the time for echo
            var timeInSecond = SendTrigger(_pinEcho, GpioPinValue.High);
            var distance = timeInSecond * 17000;
            return distance;
        }

        private double SendTrigger(GpioPin pin, GpioPinValue value)
        {
            var sw = new Stopwatch();
            // trigger 
            while (pin.Read() != value)
            {
                // No implementation
            }
            sw.Start();

            // Wait for echo 
            while (pin.Read() == value)
            {
                // No implementation
            }
            sw.Stop();

            return sw.Elapsed.TotalSeconds;
        }

        private async void InitGpio()
        {
            var gpio = GpioController.GetDefault();
            if (gpio == null)
            {
                _pinEcho = null;
                _pinTrigger = null;
                return;
            }
            _pinEcho = gpio.OpenPin(IntEchoPin);
            _pinTrigger = gpio.OpenPin(IntTriggerPin);
            _pinTrigger.SetDriveMode(GpioPinDriveMode.Output);
            _pinEcho.SetDriveMode(GpioPinDriveMode.Input);
            _pinTrigger.Write(GpioPinValue.Low);

             await Task.Delay(100);
        }

        /// <summary>
        /// send telemetry data to cloud with parking availability status
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="lastUpdated"></param>
        /// <param name="pStatus"></param>
        private static async void SendDeviceToCloudMessagesAsync(string deviceName, DateTimeOffset lastUpdated, bool pStatus)
        {
            var telemetryDataPoint = new
            {
                deviceId = deviceName,
                time = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                parkingStatus = pStatus,
                LastReadTime = lastUpdated
            };
            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await _deviceClient.SendEventAsync(message);
        }



    }
}
