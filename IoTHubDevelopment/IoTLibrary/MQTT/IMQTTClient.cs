namespace IoTLibrary.MQTT
{
    public interface IMqttClient
    {
        void RecievedCommand(string deviceId, string deviceKey);
        void SendEvent(string deviceId, string deviceKey);
    }
}