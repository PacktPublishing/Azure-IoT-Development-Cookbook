using Microsoft.Azure.Devices;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Codecs.Mqtt.Packets;
using IoTLibrary;
using IoTLibrary.Devices;
using IoTLibrary.Messages;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Newtonsoft.Json;
using TransportType = Microsoft.Azure.Devices.TransportType;

namespace IoTHubDevelopment
{
    public static class IoTHubDevelopment
    {
        private static Task<CloudToDeviceMethodResult> _result;

        public static void Main()
        {
            // Method intentionally left empty.

            var deviceIdentity = new DeviceIdentity();

            ProcessDeviceToCloud();

            ProcessFileUpload();

            // Device identity Operations
            ManageDeviceIdentity(deviceIdentity, AzureIoTHub.deviceId);

            Console.ReadLine();

            // working with Device Twins
            ManageDeviceTwin(deviceIdentity, AzureIoTHub.deviceId);

            Console.ReadLine();

            // IoT Hub Device Jobs
            ManageDeviceJob(deviceIdentity, AzureIoTHub.deviceId);
            

            Console.ReadLine();
        }

        private static void ProcessDeviceToCloud()
        {
            Console.WriteLine("Processing Device Messages\n");

           new ReadDeviceToCloudMessages().ReceiveMessagesFromDevice();          
            Console.ReadLine();
        }

        private static async void ProcessFileUpload()
        {
            var serviceClient = ServiceClient.CreateFromConnectionString(AzureIoTHub.GetConnectionString());
            await new FileUploads().ReceiveFileUploadNotificationAsync(serviceClient);
        }


        /// <summary>
        /// Manage Device Identity 
        /// </summary>
        /// <param name="deviceIdentity"></param>
        /// <param name="deviceId"></param>
        private static void ManageDeviceIdentity(DeviceIdentity deviceIdentity, string deviceId)
        {
            deviceIdentity.AddDevice(deviceId);

            var device = deviceIdentity.GetDevice(deviceId);

            Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);

            device.Status = DeviceStatus.Disabled;
            device.StatusReason = "Device Disabled Code Sample";

            Console.WriteLine("Generated device Id: {0}\n " +
                              "Generated device Status: {1}\n " +
                              "Generated device StatusReason: {2}\n " +
                              "Generated device LastActivityTime: {3}\n " +
                              "Generated device ConnectionState: {4}\n",
                device.Id, device.Status.ToString(), device.StatusReason,
                device.LastActivityTime.ToString(CultureInfo.InvariantCulture), device.ConnectionState.ToString());


            var allDevices = deviceIdentity.GetAllDevice();
            var totalDeviceCount = 0;

            if (allDevices != null)
            {
                totalDeviceCount = allDevices.Count();
            }

            Console.WriteLine("All device: {0}", totalDeviceCount);

            //Export the Device list to blob storage
            deviceIdentity.ExportIoTDevices();
            Console.WriteLine("{0} Device Details exported", totalDeviceCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceIdentity"></param>
        /// <param name="deviceId"></param>
        private static async void ManageDeviceTwin(DeviceIdentity deviceIdentity, string deviceId)
        {
            var deviceTwin = deviceIdentity.GetDeviceTwin(deviceId).Result;

            Console.WriteLine("Updated desired configuration");

            var results = deviceIdentity.SetDesiredConfigurationAndQuery(deviceTwin).Result;

            foreach (var result in results)
            {
                Console.WriteLine("Configure report for: {0}", result.DeviceId);
                Console.WriteLine("Desired deviceConfig: {0}",
                    JsonConvert.SerializeObject(result.Properties.Desired["deviceConfig"], Formatting.Indented));
                Console.WriteLine();
            }

            await deviceIdentity.DeleteDeviceAsync(deviceId);
            Console.ReadLine();
            var serviceClient = ServiceClient.CreateFromConnectionString(deviceIdentity.GetConnectionString());

            deviceIdentity.SendCloudToDeviceMessageAsync(deviceId, serviceClient);

            _result = deviceIdentity.InvokeDirectMethodOnDevice(deviceId, serviceClient);
            var response = _result.Result;

            Console.WriteLine("Response status: {0}, payload:", response.Status);
            Console.WriteLine(response.GetPayloadAsJson());

            Console.ReadLine();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceIdentity"></param>
        /// <param name="deviceId"></param>
        private static async void ManageDeviceJob(DeviceIdentity deviceIdentity, string deviceId)
        {
            var jobClient = JobClient.CreateFromConnectionString(deviceIdentity.GetConnectionString());
            string methodJobId = Guid.NewGuid().ToString();


            await deviceIdentity.StartMethodJob(methodJobId, jobClient, deviceId);

            Console.WriteLine("Started Twin Update Job");

            var result1 = deviceIdentity.MonitorJob(methodJobId, jobClient).Result;

            Console.WriteLine("Job Status : " + result1.Status);

            string twinUpdateJobId = Guid.NewGuid().ToString();

            await deviceIdentity.StartTwinUpdateJob(twinUpdateJobId, jobClient, deviceId);

            result1 = deviceIdentity.MonitorJob(twinUpdateJobId, jobClient).Result;
            Console.WriteLine("Job Status : " + result1.Status);
            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();
        }
    }
}