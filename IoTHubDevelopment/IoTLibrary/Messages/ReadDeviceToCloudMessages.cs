using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace IoTLibrary.Messages
{
    public class ReadDeviceToCloudMessages
    {
        private static EventHubClient _eventHubClient;
        private readonly string iotHubD2cEndpoint = "messages/events";

        public ReadDeviceToCloudMessages()
        {
            _eventHubClient = EventHubClient.CreateFromConnectionString(AzureIoTHub.GetConnectionString(),
                iotHubD2cEndpoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ReceiveMessagesFromDevice()
        {
            var d2CPartitions = _eventHubClient.GetRuntimeInformation().PartitionIds;
            if (d2CPartitions == null) throw new ArgumentNullException(nameof(d2CPartitions));
            var data = "";

            var tasks = new List<Task>();
            foreach (string partition in d2CPartitions)
            {
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition));
            }
            Task.WaitAll(tasks.ToArray());
 
            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partition"></param>
        /// <returns></returns>
        private static async Task<string> ReceiveMessagesFromDeviceAsync(string partition)
        {
            var eventHubReceiver = _eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.Now);
            while (true)
            {
                var eventData = await eventHubReceiver.ReceiveAsync();
                if (eventData == null) continue;

                var data = Encoding.UTF8.GetString(eventData.GetBytes());
                Console.WriteLine("Message received. Data: '{0}'", data);
                return data;
            }
        }
    }
}