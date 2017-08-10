using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Shared;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace IoTLibrary.Devices
{
    public class DeviceIdentity
    {
        readonly RegistryManager _registryManager;
        readonly string _connectionString;

        public DeviceIdentity()
        {
            // initialize to default values
            _connectionString =  AzureIoTHub.GetConnectionString();
            _registryManager = RegistryManager.CreateFromConnectionString(_connectionString);
        }


        public string GetConnectionString()
        {
            return _connectionString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public Device AddDevice(string deviceId)
        {
            AddDeviceAsync(deviceId).Wait();

            var device = GetDevice(deviceId);
            return device;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async Task<Device> DeviceAsync(string deviceId)
        {
            Device device = new Device();
            try
            {
                device = await _registryManager.GetDeviceAsync(deviceId);
            }
            catch (DeviceAlreadyExistsException)
            {
                return device;
            }

            return device;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
		public Device GetDevice(string deviceId)
        {
            try
            {
                var a = DeviceAsync(deviceId);
                return a.Result;

            }
            catch (DeviceAlreadyExistsException)
            {
                //
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Device> GetAllDevice()
        {
            try
            {
                var devicelist = _registryManager.GetDevicesAsync(1000);
                return devicelist.Result;
            }
            catch (Exception)
            {
                //
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public async void ExportIoTDevices()
        {
            // Create a blob client which is used to connect to the blob storage.
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=iothubcookbook;AccountKey=pnVtxmokrbpl6OSMCYcLx9FfNWhxC8xkYx/sU8oPL0Gbrw5ka/yvP1bbak+sZrD2+Qejs8zWH1AiI0CEJ129AQ==;EndpointSuffix=core.windows.net");
            var blobClient = storageAccount.CreateCloudBlobClient();

            string Containername = "iothubdevices";

            //Get a reference to a container to use for the sample code, and create it if it does not exist.
            var container = blobClient.GetContainerReference(Containername);
            container.CreateIfNotExists();

            //Generate a SAS token and assign it to the current job.
            var storageUri = GetContainerSasUri(container);
            var outputStorageUri = GetContainerSasUri(container);
            await _registryManager.ExportDevicesAsync(storageUri, "devices1.txt", false);



            var job = await _registryManager.ImportDevicesAsync(storageUri, outputStorageUri);
             
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        static string GetContainerSasUri(CloudBlobContainer container)
        {

            // Set the expiry time and permissions for the container.
            // In this case no start time is specified, so the
            // shared access signature becomes valid immediately.
            var sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24);
            sasConstraints.Permissions =
              SharedAccessBlobPermissions.Write |
              SharedAccessBlobPermissions.Read |
              SharedAccessBlobPermissions.Delete;

            // Generate the shared access signature on the container,
            // setting the constraints directly on the signature.
            string sasContainerToken = container.GetSharedAccessSignature(sasConstraints);

            // Return the URI string for the container,
            // including the SAS token.
            return container.Uri + sasContainerToken;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        private async Task AddDeviceAsync(string deviceId)
        {
            bool success;
            try
            {
                await _registryManager.AddDeviceAsync(new Device(deviceId));
                success = true;
            }
            catch (DeviceAlreadyExistsException)
            {
                success = false;
            }

            if (!success)
                await _registryManager.GetDeviceAsync(deviceId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteDeviceAsync(string deviceId)
        {
            bool success;
            
            try
            {
                Device device = GetDevice(deviceId);
                await _registryManager.RemoveDeviceAsync(device);
                success = true;
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Disable the Device in scenarios of critical issues with device or any tampering 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async Task<bool> DisableDeviceAsync(string deviceId)
        {
            bool success;
          
            try
            {
                Device device = GetDevice(deviceId);
                device.Status = DeviceStatus.Disabled;
                // Update the device registry
                await _registryManager.UpdateDeviceAsync(device);
                success = true;
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }


        public async Task<Twin> GetDeviceTwin(string deviceId)
        {
            var twin = await _registryManager.GetTwinAsync(deviceId);
            return twin;
        }


        public async Task<IEnumerable<Twin>> SetDesiredConfigurationAndQuery(Twin deviceTwin)
        {
            var patch = new
            {
                properties = new
                {
                    desired = new
                    {
                        deviceConfig = new
                        {
                            configId = Guid.NewGuid().ToString(),
                            DeviceOwner = "yatish",
                            latitude = "17.5122560",
                            longitude = "70.7760470"

                        }
                    },
                    reported = new
                    {
                        deviceConfig = new
                        {
                            configId = Guid.NewGuid().ToString(),
                            DeviceOwner = "Rudra",
                            latitude = "17.5122560",
                            longitude = "70.7760470"

                        }
                    }
                },
                tags = new
                {
                    location = new
                    {
                        region = "US",
                        plant = "Redmond43"
                    }
                }

            };

            await _registryManager.UpdateTwinAsync(deviceTwin.DeviceId, JsonConvert.SerializeObject(patch), deviceTwin.ETag);


            var query = _registryManager.CreateQuery("SELECT * FROM devices WHERE deviceId = '" + deviceTwin.DeviceId + "'");
            var results = await query.GetNextAsTwinAsync();

            return results;
        }


        public async Task<IEnumerable<Twin>> SetReportedConfigurationAndQuery(Twin deviceTwin)
        {

            var patch1 = new
            {
                properties = new
                {
                    reported = new
                    {
                        deviceConfig = new
                        {
                            configId = Guid.NewGuid().ToString(),
                            DeviceOwner = "Rudra",
                            latitude = "17.5122560",
                            longitude = "70.7760470"
                        }
                    }
                },
                tags = new
                {
                    location = new
                    {
                        region = "US",
                        plant = "Redmond43"
                    }
                }

            };


            await _registryManager.UpdateTwinAsync(deviceTwin.DeviceId, JsonConvert.SerializeObject(patch1), deviceTwin.ETag);


            var query = _registryManager.CreateQuery("SELECT * FROM devices WHERE deviceId = '" + deviceTwin.DeviceId + "'");
            var results = await query.GetNextAsTwinAsync();

            return results;
        }

        public void SendCloudToDeviceMessageAsync(string deviceId, ServiceClient serviceClient)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes("Close=100;yatish"));
            serviceClient.SendAsync(deviceId, commandMessage).Wait();
        }


        public async void ReceiveFeedbackAsync(ServiceClient serviceClient)
        {
            var feedbackReceiver = serviceClient.GetFeedbackReceiver();

            while (true)
            {
                var feedbackBatch = await feedbackReceiver.ReceiveAsync();
                if (feedbackBatch == null) continue;
                 
                // take action  & Update database for action taken 
                foreach (var feedback in feedbackBatch.Records)
                {
                    if (feedback.StatusCode != FeedbackStatusCode.Success)
                    {
                        // Handle compensation here
                    }
                }

                await feedbackReceiver.CompleteAsync(feedbackBatch);
            }
        }


        public async Task<CloudToDeviceMethodResult> InvokeDirectMethodOnDevice(string deviceId, ServiceClient serviceClient)
        {
            var methodInvocation = new CloudToDeviceMethod("WriteToMessage") { ResponseTimeout = TimeSpan.FromSeconds(300) };
            methodInvocation.SetPayloadJson("'1234567890'");

            var response = await serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);

            return response;
        }


        public  async Task<JobResponse> MonitorJob(string jobId, JobClient jobClient)
        {
            return  await jobClient.GetJobAsync(jobId);            
        }

        public async Task<JobResponse> StartMethodJob(string jobId, JobClient jobClient, string deviceId)
        {
            CloudToDeviceMethod directMethod = new CloudToDeviceMethod("WriteToMessage", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

            return  await jobClient.ScheduleDeviceMethodAsync(jobId,
                "deviceId='"+ deviceId + "'",
                directMethod,
                DateTime.Now,
                10);
        }


        public  async Task<JobResponse> StartTwinUpdateJob(string jobId, JobClient jobClient, string deviceId)
        {
            var twin = new Twin();
            twin.Properties.Desired["HighTemperature"] = "44";
            twin.Properties.Desired["City"] = "Mumbai";
            twin.ETag = "*";

            return await jobClient.ScheduleTwinUpdateAsync(jobId,
                "deviceId='"+ deviceId + "'",
                twin,
                DateTime.Now,
                10);
        }


    }
}
