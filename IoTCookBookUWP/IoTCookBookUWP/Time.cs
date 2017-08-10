using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Sensors.Dht;

namespace IoTCookBookUWP
{
    public class Time
    {
        public Time()
        {
            DispatcherTimer _timer;

            _timer = new DispatcherTimer();
            _timer.Tick += (sender, o) =>
            {
                Debug.WriteLine("Timer On");
            };

            _timer.Interval = TimeSpan.FromSeconds(1);

            _timer.Start();

        }
    }
}
