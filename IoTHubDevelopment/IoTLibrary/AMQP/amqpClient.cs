using System;
using System.Text;
using Amqp;
using Amqp.Framing;
using Newtonsoft.Json;

namespace IoTLibrary.AMQP
{
    public class AmqpClient
    {
        string IoThubURI = "IoTHubCookBook.azure-devices.net";
        int port = 5671;
        readonly Connection _connection;
        Session _session;

        private Address Address { get; set; }

        public AmqpClient()
        {
            Address = new Address(IoThubURI, port);
            _connection = new Connection(Address);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        public void SendEvent(string deviceId)
        {
            string to = Fx.Format("/devices/{0}/messages/events", deviceId);

            string audience = Fx.Format("{0}/devices/{1}", IoThubURI, deviceId);
            Fx.Format($"{0}/devices/{1}", IoThubURI, deviceId);

            string sasToken = "SharedAccessSignature sr=IoTHubCookBook.azure-devices.net&sig=hU2tQbo1aYnFfGC8ctSfifeIV677KKlWpnCS%2F05SMxY%3D&se=1531101825&skn=iothubowner";
            bool cbs = PutCbsToken(_connection, sasToken, audience);

            if (cbs)
            {
                _session = new Session(_connection);
            }

            SenderLink senderevent = new SenderLink(_session, "senderevent", to);
            
            double avgWindSpeed = 10; // m/s
            Random rand = new Random();


            int i = 0;
            while (i < 10)
            {
                var currentWindSpeed = avgWindSpeed + rand.NextDouble() * 4 - 2;
                var telemetryDataPoint = new
                {
                    deviceId,
                    windSpeed = currentWindSpeed,
                    highTemp = rand.Next(45,95),
                    lowtemp = rand.Next(-5, 25),
                    latitude = "17.5122560",
                    longitude = "70.7760470"
                };

                var json = JsonConvert.SerializeObject(telemetryDataPoint);

                var messageValue = Encoding.UTF8.GetBytes(json);

                var telemetryMessage = new Message()
                {
                    BodySection = new Data() { Binary = messageValue }
                };

                telemetryMessage.Properties = new Properties();
                telemetryMessage.Properties.To = to;
                telemetryMessage.Properties.MessageId = Guid.NewGuid().ToString();
                telemetryMessage.ApplicationProperties = new ApplicationProperties();

                senderevent.Send(telemetryMessage);
                System.Threading.Thread.Sleep(5000);

                i++;
            }
            senderevent.Close();
        }

        public void ReceiveCommand()
        {
            string audience = Fx.Format("{0}/messages/servicebound/feedback", IoThubURI);
            Fx.Format("{0}/messages/servicebound/feedback", IoThubURI);

            string entity = Fx.Format("/devices/{0}/messages/deviceBound", AzureIoTHub.deviceId);

            string sasToken = "SharedAccessSignature sr=IoTHubCookBook.azure-devices.net&sig=hU2tQbo1aYnFfGC8ctSfifeIV677KKlWpnCS%2F05SMxY%3D&se=1531101825&skn=iothubowner";
            bool cbs = PutCbsToken(_connection, sasToken, audience);

            if (cbs)
            {
                _session = new Session(_connection);
            }


            ReceiverLink receiveCommand = new ReceiverLink(_session, "receiveCommands", entity);
            int i = 0;
            while (i < 50)
            {
                var received = receiveCommand.Receive();
                if (received != null)
                {
                    receiveCommand.Accept(received);
                    var returnString = Encoding.UTF8.GetString(received.GetBody<byte[]>());
                    Console.WriteLine(returnString);
                    // process the Command at Device
                    // Write your code here
                }
                i++;
            }

            receiveCommand.Close();
        }

        private static bool PutCbsToken(Connection connection, string shareAccessSignature, string audience)
        {
            bool result = true;
            Session session = new Session(connection);

            string cbsReplyToAddress = "cbs-reply-to";
            var cbsSender = new SenderLink(session, "cbs-sender", "$cbs");
            var cbsReceiver = new ReceiverLink(session, cbsReplyToAddress, "$cbs");

            // construct the put-token message
            var request = new Message(shareAccessSignature);
            request.Properties = new Properties();
            request.Properties.MessageId = Guid.NewGuid().ToString();
            request.Properties.ReplyTo = cbsReplyToAddress;
            request.ApplicationProperties = new ApplicationProperties();
            request.ApplicationProperties["operation"] = "put-token";
            request.ApplicationProperties["type"] = "azure-devices.net:sastoken";
            request.ApplicationProperties["name"] = audience;
            cbsSender.Send(request);

            // receive the response
            var response = cbsReceiver.Receive();
            if (response == null || response.Properties == null || response.ApplicationProperties == null)
            {
                result = false;
            }
            else
            {
                int statusCode = (int)response.ApplicationProperties["status-code"];
                if (statusCode != 202 && statusCode != 200) // !Accepted && !OK
                {
                    result = false;
                }
            }

            // the sender/receiver may be kept open for refreshing tokens
            cbsSender.Close();
            cbsReceiver.Close();
            session.Close();

            return result;
        }

        



    }
}
