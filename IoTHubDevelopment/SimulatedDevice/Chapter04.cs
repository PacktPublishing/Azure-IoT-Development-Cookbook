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
                Console.WriteLine("IoT SDK HTTPS Communication");
                HttpDeviceMessages(AzureIoTHub.deviceId);

                Console.WriteLine("IoT SDK AMQP Communication");
                AmqpDeviceMessages(AzureIoTHub.deviceId);

                Console.WriteLine("IoT SDK MQTT Communication");
                MqttDeviceMessages(AzureIoTHub.deviceId);

                Console.WriteLine("AMQP Communication");
                AmqpMessages(AzureIoTHub.deviceId);

                Console.WriteLine("MQTT Communication");
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
            catch (Exception ex)
            {
                //Some code here
                Console.WriteLine("Error in sample: {0}", ex.Message);
            }
        }

        private static void MqttDeviceMessages(string deviceId)
        {
            SendDeviceMessages(deviceId, TransportType.Mqtt);
        }

        private static void AmqpDeviceMessages(string deviceId)
        {
            SendDeviceMessages(deviceId, TransportType.Amqp);
        }

        private static void HttpDeviceMessages(string deviceId)
        {
            SendDeviceMessages(deviceId, TransportType.Http1);
        }

        /// <summary>
        /// Send Device to Cloud Message
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="transportType"></param>
        private static void SendDeviceMessages(string deviceId, TransportType transportType)
        {
            var deviceClient = DeviceClient.CreateFromConnectionString(AzureIoTHub.GetDeviceConnectionString(), transportType);
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