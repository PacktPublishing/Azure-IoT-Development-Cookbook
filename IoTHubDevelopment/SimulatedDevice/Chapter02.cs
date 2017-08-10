using System;
using IoTLibrary;
using IoTLibrary.Devices;
using Microsoft.Azure.Devices.Client;

namespace SimulatedDevice
{
    internal static class Chapter02
    {
        private static void Main()
        {
            Console.WriteLine("Simulated device\n");

            DeviceClient deviceClient = null;

            try
            {
                deviceClient =
                    DeviceClient.CreateFromConnectionString(AzureIoTHub.GetConnectionString(), TransportType.Mqtt);

                ExcuteDirectMethod(deviceClient);
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

            RemoveMethodHandler(deviceClient);
            Console.ReadLine();
        }


        private static void RemoveMethodHandler(DeviceClient deviceClient)
        {
            // remove the 'WriteToConsole' handler
            deviceClient?.SetMethodHandlerAsync("WriteToConsole", null, null).Wait();

            // remove the 'GetDeviceName' handler
            deviceClient?.SetMethodHandlerAsync("GetDeviceName", null, null).Wait();
            deviceClient?.CloseAsync().Wait();
        }

        private static void ExcuteDirectMethod(DeviceClient deviceClient)
        {
            deviceClient.SetMethodHandlerAsync("WriteToMessage", new DeviceSimulator(deviceClient).WriteToMessage,
                null).Wait();

            deviceClient.SetMethodHandlerAsync("GetDeviceName", new DeviceSimulator(deviceClient).GetDeviceName,
                new DeviceData()).Wait();
        }


        private class DeviceData
        {
        }
    }
}