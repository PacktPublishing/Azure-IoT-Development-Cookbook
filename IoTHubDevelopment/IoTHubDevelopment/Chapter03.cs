using Microsoft.Azure.Devices;
using System;
using IoTLibrary;
using IoTLibrary.Devices;
using IoTLibrary.Messages;

namespace IoTHubDevelopment
{
    public static class Chapter03
    {
        public static void Main()
        {
            var deviceIdentity = new DeviceIdentity();

            // Method intentionally left empty.

            ProcessDeviceToCloud();
            
            ProcessFileUpload();

            // working with Device method
            ManageCloudToDeviceMessage(deviceIdentity, AzureIoTHub.deviceId);
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

        private static async void ManageCloudToDeviceMessage(DeviceIdentity deviceIdentity, string deviceId)
        {
            var serviceClient = ServiceClient.CreateFromConnectionString(deviceIdentity.GetConnectionString());

            deviceIdentity.SendCloudToDeviceMessageAsync(deviceId, serviceClient);

            Console.ReadLine();

        }

    }
}