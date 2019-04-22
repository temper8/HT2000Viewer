using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace HT2000Viewer.Models
{
    public class MqttConnection
    {
        //string brokerHostName = "m24.cloudmqtt.com";
        public string brokerHostName {
            get => (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["BrokerHostName"];
            set => Windows.Storage.ApplicationData.Current.LocalSettings.Values["BrokerHostName"] = value;
        }

        public int brokerPort
        {
            get => Convert.ToInt32(Windows.Storage.ApplicationData.Current.LocalSettings.Values["BrokerPort"]);
            set => Windows.Storage.ApplicationData.Current.LocalSettings.Values["BrokerPort"] = value;
        }

        public string username
        {
            get => (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["username"];
            set => Windows.Storage.ApplicationData.Current.LocalSettings.Values["username"] = value;
        }

        public string password
        {
            get => (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["password"];
            set => Windows.Storage.ApplicationData.Current.LocalSettings.Values["password"] = value;
        }

        public string clientId
        {
            get => (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["clientId"];
            set => Windows.Storage.ApplicationData.Current.LocalSettings.Values["clientId"] = value;
        }

        MqttClient client = null;

        public MqttConnection()
        {
            try
            {
                client = new MqttClient(brokerHostName, brokerPort, false, MqttSslProtocols.None);

                clientId = Guid.NewGuid().ToString();
                client.Connect(clientId, username, password);
            }
            catch (Exception e)
            {

            }
        }

        public void Publish(string topic, double value)
        {
            if (client == null) return;
            string strValue = value.ToString("F1");//  Convert.ToString("F1", value);
            client.Publish("/home/temperature", Encoding.UTF8.GetBytes(strValue), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }


        public void PublishState(Measurement m)
        {
            //  Temperature = m.Temperature;
            //  Humidity = m.Humidity;
            //  CO2 = m.CO2;
            Publish("/home/temperature", m.Temperature);
        }
            // publish a message on "/home/temperature" topic with QoS 2 

        }
    }
