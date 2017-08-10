using System;
using IoTLibrary;
using IoTLibrary.Devices;
using IoTLibrary.Messages;
using Microsoft.Azure.Devices.Client;

namespace SimulatedDevice
{
    internal static class Chapter03
    {
        private static void Main()
        {
            Console.WriteLine("Simulated device\n");

            try
            {
                var deviceClient = DeviceClient.CreateFromConnectionString(AzureIoTHub.GetConnectionString(), TransportType.Mqtt);

                new DeviceSimulator(deviceClient).SendDeviceToCloudMessagesAsync(AzureIoTHub.deviceId);
                /*    */

                // UN-comment the code you want to execute
                new FileUploads().InitFileUpload();

                UpdateFirmware(AzureIoTHub.deviceId);

                Console.ReadLine();
            }
            catch (AggregateException ex)
            {
                foreach (var exception in ex.InnerExceptions)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error in sample: {0}", exception);
                }
            }
            catch (Exception)
            {
                //Some code here
            }
        }


        private static void UpdateFirmware(string deviceId)
        {
            var udpate = new FirmwareUpdate();

            udpate.StartFirmwareUpdate(deviceId);
            udpate.QueryTwinFwUpdateReported(deviceId);
        }

    }
}