using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using TransportType = Microsoft.Azure.Devices.Client.TransportType;

namespace IoTLibrary.Messages
{
    public class FileUploads
    {
        private static DeviceClient _deviceClient;

        static FileUploads()
        {
            _deviceClient = null;
        }

        public void InitFileUpload()
        {
            Console.WriteLine("Simulated device to upload File to IoT Hub\n");
            CreateFile();
            try
            {
                _deviceClient = DeviceClient.CreateFromConnectionString(AzureIoTHub.GetDeviceConnectionString(),
                    TransportType.Mqtt);
                SendDeviceDataToBlob();
            }
            catch (Exception)
            {
                //
            }

            Console.WriteLine("Add IoT hub File upload Notification");
            Console.ReadLine();
        }

        private static void CreateFile()
        {
            // Method intentionally left empty.
        }

        private static async void SendDeviceDataToBlob()
        {
            var telemetryDataFile = @"FileData.txt";
            Console.WriteLine("Uploading file: {0}", telemetryDataFile);
            var watch = Stopwatch.StartNew();

            using (var telemetryData = new FileStream(telemetryDataFile, FileMode.Open))
            {
                await _deviceClient.UploadToBlobAsync(telemetryDataFile, telemetryData);
            }
            watch.Stop();
            Console.WriteLine("Time to upload file: {0}ms\n", watch.ElapsedMilliseconds);
        }

        public async Task ReceiveFileUploadNotificationAsync(ServiceClient serviceClient)
        {
            Console.WriteLine("\nReceiving file upload notification from service");

            var notificationReceiver = serviceClient.GetFileNotificationReceiver();
            while (true)
            {
                var fileUploadNotification = await notificationReceiver.ReceiveAsync();
                if (fileUploadNotification == null) continue;

                var downoadFile = fileUploadNotification.BlobName;
                if (downoadFile == null) throw new ArgumentNullException(nameof(downoadFile));
                // Download the File uploaded on Blob storage
                // process the File data
                // ......
                // ......

                await notificationReceiver.CompleteAsync(fileUploadNotification);
            }
        }
    }
}