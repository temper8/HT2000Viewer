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
        string brokerHostName = "m24.cloudmqtt.com";
        int brokerPort = 14288;
        string username = "";
        string password = "";

        MqttClient client = null;
        string clientId;
        public MqttConnection()
        {
            client = new MqttClient(brokerHostName, brokerPort, false, MqttSslProtocols.None);

            clientId = Guid.NewGuid().ToString();
            client.Connect(clientId, username, password);
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
