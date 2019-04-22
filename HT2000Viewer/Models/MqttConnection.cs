using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using static Windows.Storage.ApplicationData;

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
            get => (string)Current.LocalSettings.Values["username"];
            set => Current.LocalSettings.Values["username"] = value;
        }

        public string password
        {
            get => (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["password"];
            set => Windows.Storage.ApplicationData.Current.LocalSettings.Values["password"] = value;
        }

        public string clientId
        {
            get {
                if (!Current.LocalSettings.Values.ContainsKey("clientId"))
                    Current.LocalSettings.Values["clientId"] = Guid.NewGuid().ToString();
                return (string)Current.LocalSettings.Values["clientId"];
            }
            set => Windows.Storage.ApplicationData.Current.LocalSettings.Values["clientId"] = value;
        }
        

        public string TemperatureTopic
        {
            get
            {
                if (!Current.LocalSettings.Values.ContainsKey("TemperatureTopic"))
                    Current.LocalSettings.Values["TemperatureTopic"] = $"/home/temperature";
                return (string)Current.LocalSettings.Values["TemperatureTopic"];
            }
            set => Windows.Storage.ApplicationData.Current.LocalSettings.Values["TemperatureTopic"] = value;
        }

        public string HumidityTopic
        {
            get
            {
                if (!Current.LocalSettings.Values.ContainsKey("HumidityTopic"))
                    Current.LocalSettings.Values["HumidityTopic"] = $"/home/humidity";
                return (string)Current.LocalSettings.Values["HumidityTopic"];
            }
            set => Windows.Storage.ApplicationData.Current.LocalSettings.Values["HumidityTopic"] = value;
        }

        public string CO2Topic
        {
            get
            {
                if (!Current.LocalSettings.Values.ContainsKey("CO2Topic"))
                    Current.LocalSettings.Values["CO2Topic"] = $"/home/co2";
                return (string)Current.LocalSettings.Values["CO2Topic"];
            }
            set => Windows.Storage.ApplicationData.Current.LocalSettings.Values["CO2Topic"] = value;
        }


        MqttClient client = null;

        public MqttConnection()
        {
            try
            {
                client = new MqttClient(brokerHostName, brokerPort, false, MqttSslProtocols.None);

                //clientId = Guid.NewGuid().ToString();
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
            client.Publish(topic, Encoding.UTF8.GetBytes(strValue), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }


        public void PublishState(Measurement m)
        {
            //  Temperature = m.Temperature;
            //  Humidity = m.Humidity;
            //  CO2 = m.CO2;
            Publish(TemperatureTopic, m.Temperature);
            Publish(HumidityTopic, m.Humidity);
            Publish(CO2Topic, m.CO2);
        }
            // publish a message on "/home/temperature" topic with QoS 2 

        }
    }
