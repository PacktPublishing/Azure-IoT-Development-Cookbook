using System;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;

namespace IoTLibrary.Devices
{
    /// <summary>
    /// 
    /// </summary>
    public class FirmwareUpdate
    {
        public static RegistryManager RegistryManager;
        static ServiceClient _client;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetDevice"></param>
        /// <returns></returns>
        public async void QueryTwinFwUpdateReported(string targetDevice)
        {
            Twin twin = await RegistryManager.GetTwinAsync(targetDevice);
            Console.WriteLine(twin.Properties.Reported.ToJson());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetDevice"></param>
        /// <returns></returns>
        public async void StartFirmwareUpdate(string targetDevice)
        {
            // assign the URL of Blob from where the latest firmware can be downloaded
            String bloburl = "";
            _client = ServiceClient.CreateFromConnectionString(AzureIoTHub.GetConnectionString());
            CloudToDeviceMethod method = new CloudToDeviceMethod("firmwareUpdate");
            method.ResponseTimeout = TimeSpan.FromSeconds(30);
            method.SetPayloadJson(@"{fwPackageUri : '" + bloburl + "'}");

            await _client.InvokeDeviceMethodAsync(targetDevice, method);

            Console.WriteLine("firmware update on device is Successful.");
        }





    }
}
