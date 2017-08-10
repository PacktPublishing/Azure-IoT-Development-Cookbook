using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace IoTLibrary
{
    public static class AzureIoTHub
    {
        //
        // Note: this connection string is specific to the device "IoTCookbook". To configure other devices,
        // see information on iothub-explorer at http://aka.ms/iothubgetstartedVSCS
        //

        const string MydeviceconnectionString = "HostName=IoTHubCookBook.azure-devices.net;DeviceId=myFirstDevice;SharedAccessKey=LKCXsBKMKISTjr3ii08UXgIpELxy8/38EiMuxNAiqek=";
        const string DeviceConnectionString = "HostName=IoTHubCookBook.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=yJJqUNeiYmWM4OY/olO1qJnNVwmwax2c8KyX/GfdXcI=";
        public const string deviceId = "myFirstDevice";
        public const string deviceKey = "LKCXsBKMKISTjr3ii08UXgIpELxy8/38EiMuxNAiqek=";

        //
        // To monitor messages sent to device "IoTCookbook" use iothub-explorer as follows:
        //    iothub-explorer HostName=IoTHubCookeBook.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=P0gnlZ6D6I0mSAY4zIK+WWPzXsvK+pCMyY7cByH1r4g= monitor-events "IoTCookbook"
        //

        // Refer to http://aka.ms/azure-iot-hub-vs-cs-wiki for more information on Connected Service for Azure IoT Hub
 
        public  static string GetConnectionString()
        {
            return DeviceConnectionString;
        }

        public static string GetDeviceConnectionString()
        {
            return MydeviceconnectionString;
        }

    }

}
