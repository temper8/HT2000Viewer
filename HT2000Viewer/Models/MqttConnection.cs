using HT2000Viewer.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using static Windows.Storage.ApplicationData;

namespace HT2000Viewer.Models
{
    public class MqttConnection: Observable
    {
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

        public int QoS
        {
            get => Convert.ToInt32(Windows.Storage.ApplicationData.Current.LocalSettings.Values["Qos"]);
            set => Windows.Storage.ApplicationData.Current.LocalSettings.Values["Qos"] = value;
        }

        public bool Enabled
        {
            get
            {
                if (!Current.LocalSettings.Values.ContainsKey("Enabled"))
                    Current.LocalSettings.Values["Enabled"] = false;
                return (bool)Current.LocalSettings.Values["Enabled"];
            }
            set 
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["Enabled"] = value;
                UpdateConnection();
                OnPropertyChanged("Enabled");
            }
        }

        string _ConnectionStatus;
        public string ConnectionStatus
        {
            get => _ConnectionStatus;
            set 
            {
                if (!String.Equals(_ConnectionStatus, value))
                    Set(ref _ConnectionStatus, value); 
            }

        }

        public int Interval
        {
            get
            {
                if (!Current.LocalSettings.Values.ContainsKey("Interval"))
                    Current.LocalSettings.Values["Interval"] = 30;
                return (int)Current.LocalSettings.Values["Interval"];
            }
            set
            {
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["Interval"] = value;
                OnPropertyChanged("Interval");
            }
        }

        
        public string TemperatureTopic
        {
            get
            {
                if (!Current.LocalSettings.Values.ContainsKey("TemperatureTopic"))
                    Current.LocalSettings.Values["TemperatureTopic"] = $"/ht2000/temperature";
                return (string)Current.LocalSettings.Values["TemperatureTopic"];
            }
            set => Windows.Storage.ApplicationData.Current.LocalSettings.Values["TemperatureTopic"] = value;
        }

        public string HumidityTopic
        {
            get
            {
                if (!Current.LocalSettings.Values.ContainsKey("HumidityTopic"))
                    Current.LocalSettings.Values["HumidityTopic"] = $"/ht2000/humidity";
                return (string)Current.LocalSettings.Values["HumidityTopic"];
            }
            set => Windows.Storage.ApplicationData.Current.LocalSettings.Values["HumidityTopic"] = value;
        }

        public string CO2Topic
        {
            get
            {
                if (!Current.LocalSettings.Values.ContainsKey("CO2Topic"))
                    Current.LocalSettings.Values["CO2Topic"] = $"/ht2000/co2";
                return (string)Current.LocalSettings.Values["CO2Topic"];
            }
            set => Windows.Storage.ApplicationData.Current.LocalSettings.Values["CO2Topic"] = value;
        }


        MqttClient client = null;

        public MqttConnection()
        {
            UpdateConnection();
        }
 
        public void UpdateConnection()
        {
            if (Enabled)
                TryConnect();
            else
                DisConnect();
        }

        public void DisConnect()
        {
            if (client != null)
                if (client.IsConnected)
                    client.Disconnect();
            client = null;
            ConnectionStatus = "MQTT status: disconnected";
        }

        SemaphoreSlim slim = new SemaphoreSlim(1);

        public async void TryConnect()
        {
            await slim.WaitAsync();
            ConnectionStatus = "MQTT status: try connect to ...";

            try
            {
                if (client == null)
                {
                    client = new MqttClient(brokerHostName, brokerPort, false, MqttSslProtocols.None);
                }
                client.Connect(clientId, username, password);
                ConnectionStatus = $"MQTT status: connected to {brokerHostName}:{brokerPort}";
            }
            catch (Exception e)
            {
                ConnectionStatus = e.Message;
            }
            if (client != null)
                if (!client.IsConnected)
                    await Task.Delay(5000);
            slim.Release();
        }


        public void Publish(string topic, double value)
        {
            if (client == null) return;
            if (client.IsConnected)
            {
                ConnectionStatus = $"MQTT status: connected to {brokerHostName}:{brokerPort}";
                string strValue = value.ToString("F1");
                client.Publish(topic, Encoding.UTF8.GetBytes(strValue), (byte)QoS, false);
            }
            else
            {
                ConnectionStatus = "MQTT status: disconnected";
                if (slim.CurrentCount > 0)
                    TryConnect();
            }
        }


        DateTime t0 = DateTime.Now;

        public void PublishState(Measurement m)
        {
            TimeSpan i = DateTime.Now - t0;
            if (i.TotalSeconds < Interval) return;
            t0 = DateTime.Now;

            Publish(TemperatureTopic, m.Temperature);
            Publish(HumidityTopic, m.Humidity);
            Publish(CO2Topic, m.CO2);
        }
        }
    }
