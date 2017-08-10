using System;
using System.Text;
using System.Threading;
using IoTLibrary;
using IoTLibrary.AMQP;
using IoTLibrary.Devices;
using IoTLibrary.Messages;
using IoTLibrary.MQTT;
using Microsoft.Azure.Devices.Client;

namespace SimulatedDevice
{
    internal static class SimulatedDevice
    {
        private static void Main()
        {
            Console.WriteLine("Simulated device\n");
            DeviceClient deviceClient = null;


            try
            {
                deviceClient =
                    DeviceClient.CreateFromConnectionString(AzureIoTHub.GetConnectionString(), TransportType.Mqtt);

                new DeviceSimulator(deviceClient).SendDeviceToCloudMessagesAsync(AzureIoTHub.deviceId);
                /*    */


                // UN-comment the code you want to execute
                new FileUploads().InitFileUpload();

                AmqpMessages(AzureIoTHub.deviceId);

                MqttMessages(AzureIoTHub.deviceId, AzureIoTHub.deviceKey);

                UpdateFirmware(AzureIoTHub.deviceId);

                Console.ReadLine();

                ExcuteDirectMethod(deviceClient);

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

            RemoveMethodHandler(deviceClient);


            Console.ReadLine();
        }


        private static void UpdateFirmware(string deviceId)
        {
            var udpate = new FirmwareUpdate();

            udpate.StartFirmwareUpdate(deviceId);
            udpate.QueryTwinFwUpdateReported(deviceId);
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

        private static void MqttMessages(string deviceId, string deviceKey)
        {
            //for MQTT
            var clsmqttClient = new MqttClass();
            clsmqttClient.SendEvent(deviceId, deviceKey);
            clsmqttClient.RecievedCommand(deviceId, deviceKey);
        }

        private static void AmqpMessages(string deviceId)
        {
            // for AMQP
            var clsamqpClient = new AmqpClient();
            clsamqpClient.SendEvent(deviceId);
            var receiverThread = new Thread(clsamqpClient.ReceiveCommand);
            receiverThread.Start();
        }

        private class DeviceData
        {
        }
    }
}