using System;
using System.Threading;
using IoTLibrary;
using IoTLibrary.AMQP;
using IoTLibrary.Devices;
using IoTLibrary.MQTT;
using Microsoft.Azure.Devices.Client;

namespace SimulatedDevice
{
    internal static class Chapter04
    {
        private static void Main()
        {
            Console.WriteLine("Simulated device\n");

            try
            {
                HttpDeviceMessages(AzureIoTHub.deviceId);

                AmqpDeviceMessages(AzureIoTHub.deviceId);

                MqttDeviceMessages(AzureIoTHub.deviceId);

                AmqpMessages(AzureIoTHub.deviceId);

                MqttMessages(AzureIoTHub.deviceId, AzureIoTHub.deviceKey);

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

        private static void MqttDeviceMessages(string deviceId)
        {
            var deviceClient = DeviceClient.CreateFromConnectionString(AzureIoTHub.GetConnectionString(), TransportType.Mqtt);

            new DeviceSimulator(deviceClient).SendDeviceToCloudMessagesAsync(deviceId);             
        }

        private static void AmqpDeviceMessages(string deviceId)
        {
            var deviceClient = DeviceClient.CreateFromConnectionString(AzureIoTHub.GetConnectionString(), TransportType.Amqp);

            new DeviceSimulator(deviceClient).SendDeviceToCloudMessagesAsync(deviceId);
        }

        private static void HttpDeviceMessages(string deviceId)
        {
            var deviceClient = DeviceClient.CreateFromConnectionString(AzureIoTHub.GetConnectionString(), TransportType.Http1);

            new DeviceSimulator(deviceClient).SendDeviceToCloudMessagesAsync(deviceId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="deviceKey"></param>
        private static void MqttMessages(string deviceId, string deviceKey)
        {
            //for MQTT
            var clsmqttClient = new MqttClass();
            clsmqttClient.SendEvent(deviceId, deviceKey);
            clsmqttClient.RecievedCommand(deviceId, deviceKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        private static void AmqpMessages(string deviceId)
        {
            // for AMQP
            var clsamqpClient = new AmqpClient();
            clsamqpClient.SendEvent(deviceId);
            var receiverThread = new Thread(clsamqpClient.ReceiveCommand);
            receiverThread.Start();
        }

    }
}