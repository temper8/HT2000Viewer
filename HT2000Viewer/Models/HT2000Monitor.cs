using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Devices.Usb;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using HT2000Viewer.Common;
using HT2000Viewer.Models;
using Windows.ApplicationModel.Core;

namespace HT2000Viewer.Models
{

    public class HT2000Monitor
    {
        private const long TIMESTAMP_SHIFT = 0x77797DA0; // TODO: magic constant
        private const int TEMPERATURE_SHIFT = 112; // TODO: magic constant

        Measurement CreateRealMeasurement(byte[] buffer)
        {
            Measurement m = new Measurement
            {
                Tik = DateTime.Now,
                CO2 = buffer[25] + 256 * buffer[24],
                Temperature = (TEMPERATURE_SHIFT + /*Byte.*/(uint)(buffer[8])) / 10.0,
                Humidity = (256 * buffer[9] + buffer[10]) / 10.0
            };
            prevCO2 = m.CO2;
            prevTemperature = m.Temperature;
            prevHumidity = m.Humidity;
            return m;
        }

        static double prevCO2 = 600;
        static double prevTemperature = 20;
        static double prevHumidity = 40;
        static Random rnd = new Random();

        Measurement CreateRandomMeasurement()
        {
            Measurement m = new Measurement
            {
                Tik = DateTime.Now,
                CO2 = prevCO2 + 10 * (rnd.NextDouble() - 0.5),
                Temperature = prevTemperature + 1 * (rnd.NextDouble() - 0.5),
                Humidity = prevHumidity + 1 * (rnd.NextDouble() - 0.5)
            };
            prevCO2 = m.CO2;
            prevTemperature = m.Temperature;
            prevHumidity = m.Humidity;
            return m;
        }

        private ExtendedExecutionSession session = null;

        void ClearExtendedExecution()
        {
            if (session != null)
            {
                session.Revoked -= SessionRevoked;
                session.Dispose();
                session = null;
            }

            if (_timer != null)
            {
                StopDeviceWatcher();
                //_timer.Dispose();
                // periodicTimer = null;
            }
        }

        private async void BeginExtendedExecution()
        {
            // The previous Extended Execution must be closed before a new one can be requested.
            // This code is redundant here because the sample doesn't allow a new extended
            // execution to begin until the previous one ends, but we leave it here for illustration.
            ClearExtendedExecution();

            var newSession = new ExtendedExecutionSession();
            newSession.Reason = ExtendedExecutionReason.Unspecified;
            newSession.Description = "Raising periodic toasts";
            newSession.Revoked += SessionRevoked;
            ExtendedExecutionResult result = await newSession.RequestExtensionAsync();

            switch (result)
            {
                case ExtendedExecutionResult.Allowed:
                    // rootPage.NotifyUser("Extended execution allowed.", NotifyType.StatusMessage);
                    Debug.WriteLine("Extended execution allowed.");
                    session = newSession;
                    StartDeviceWatcher();
                  //  periodicTimer = new Timer(OnTimer, DateTime.Now, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10));
                    break;

                default:
                case ExtendedExecutionResult.Denied:
                    Debug.WriteLine("Extended execution denied.");
                    //  rootPage.NotifyUser("Extended execution denied.", NotifyType.ErrorMessage);
                    newSession.Dispose();
                    break;
            }
           // UpdateUI();
        }

        private async void SessionRevoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            Debug.WriteLine("SessionRevoked");

//            await CoreDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //{
                switch (args.Reason)
                {
                    case ExtendedExecutionRevokedReason.Resumed:
                        //rootPage.NotifyUser("Extended execution revoked due to returning to foreground.", NotifyType.StatusMessage);
                        Debug.WriteLine("Extended execution revoked due to returning to foreground.");
                        break;

                    case ExtendedExecutionRevokedReason.SystemPolicy:
                        //rootPage.NotifyUser("Extended execution revoked due to system policy.", NotifyType.StatusMessage);
                        Debug.WriteLine("Extended execution revoked due to system policy.");
                        break;
                }

                EndExtendedExecution();
            //});
            
        }
        private void EndExtendedExecution()
        {
            ClearExtendedExecution();
         //   UpdateUI();
        }
        HidDevice myDevice = null;
        // Find HID devices.
        public bool IsConnect {get { return myDevice != null; } }

        public delegate void DeviceInsertedHandler();
        public DeviceInsertedHandler Inserted;

        public delegate void DeviceRemovedHandler();
        public DeviceRemovedHandler Removed;

        ushort vendorId = 0x10c4;
        ushort productId = 0x82cd;
        ushort usagePage = 0xFF00;
        ushort usageId = 0x0001;

        DeviceWatcher watcher;
        public void InitWatcher()
        {
            // Create the selector.
            string selector =
                HidDevice.GetDeviceSelector(usagePage, usageId, vendorId, productId);

            watcher = DeviceInformation.CreateWatcher(selector);
            watcher.Added += new TypedEventHandler<DeviceWatcher, DeviceInformation>(this.OnDeviceAdded);
            watcher.Removed += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(this.OnDeviceRemoved);
            watcher.EnumerationCompleted += new TypedEventHandler<DeviceWatcher, Object>(this.OnDeviceEnumerationComplete);

            //StartDeviceWatcher();
            BeginExtendedExecution();
        }
      

        private void StartDeviceWatcher()
        {
          
            _timer.Start();
            if ((watcher.Status != DeviceWatcherStatus.Started)
                    && (watcher.Status != DeviceWatcherStatus.EnumerationCompleted))
            {

                watcher.Start();
            }
        }


        private void StopDeviceWatcher()
        {
            // Stop all device watchers
            _timer.Stop();
            if ((watcher.Status == DeviceWatcherStatus.Started)
                    || (watcher.Status == DeviceWatcherStatus.EnumerationCompleted))
                {
              
                watcher.Stop();
                }


            // Clear the list of devices so we don't have potentially disconnected devices around
           // watcher = null;

          
        }

        public void OnSuspending()
        {
            Debug.WriteLine("OnSuspending");
            //  StopDeviceWatcher();
        }

        public void OnResuming()
        {
            Debug.WriteLine("OnResuming");
            BeginExtendedExecution();
           // StartDeviceWatcher();
        }


        private void OnDeviceAdded(DeviceWatcher sender, DeviceInformation deviceInformation)
        {
            Debug.WriteLine("OnDeviceAdded");
            if (Inserted != null) Inserted();
        }

        private void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceInformationUpdate)
        {
            Debug.WriteLine("OnDeviceRemoved");
            if (Removed != null) Removed();
        }
        private void OnDeviceEnumerationComplete(DeviceWatcher sender, Object args)
        {
            Debug.WriteLine("OnDeviceEnumerationComplete");
        }

        //DispatcherTimer _timer = new DispatcherTimer();
        System.Timers.Timer _timer;
        public void Initialize()
        {

            //  FillSensorData();
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += UpdateSensorData;
            //  _timer.Interval = TimeSpan.FromSeconds(1);
            //  _timer.Tick += UpdateSensorData;
            //   _timer.Start();

            InitWatcher();
        }

        public delegate void DevicePushData(Measurement state);
        public DevicePushData OnPushData;
          
        private void AddState(Measurement state)
        {
            if (OnPushData != null) OnPushData(state);
        }

        private async void UpdateSensorData(object sender, object e)
        {
            //   Debug.WriteLine("UpdateSensorData");
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
              {
                  // visits = visits < 0 ? 0 : visits;
                  if (IsConnect)
                      AddState(await ReadState());
                  else
                      AddState(CreateRandomMeasurement());
              });
        }




        public async void Connect()
        {

            string selector =
                            HidDevice.GetDeviceSelector(usagePage, usageId, vendorId, productId);


            // Enumerate devices using the selector.
            var devices = await DeviceInformation.FindAllAsync(selector);

            if (devices.Any())
            {
                // At this point the device is available to communicate with
                // So we can send/receive HID reports from it or 
                // query it for control descriptions.
               // Info.Text = "HID devices found: " + devices.Count;

                // Open the target HID device.
                HidDevice device =
                    await HidDevice.FromIdAsync(devices.ElementAt(0).Id, FileAccessMode.ReadWrite);

                myDevice = device;

              //  ReadWriteToHidDevice(device);

                if (device != null)
                {

                }

            }
            else
            {
                // There were no HID devices that met the selector criteria.
               // Info.Text = "HID device not found";
            }
        }

        public async Task<Measurement> ReadState()
        {
            if (myDevice != null)
            {
                // construct a HID output report to send to the device
                //  HidOutputReport outReport = device.CreateOutputReport();

                /// Initialize the data buffer and fill it in
                byte[] buffer = new byte[] { 10, 20, 30, 40 };

                DataWriter dataWriter = new DataWriter();

                //
                try
                {
                    HidInputReport inReport = await myDevice.GetInputReportAsync(05);
                    if (inReport != null)
                    {

                        UInt16 id = inReport.Id;
                        Debug.WriteLine("inReport.Id" + id);
                        var bytes = new byte[32];
                        DataReader dataReader = DataReader.FromBuffer(inReport.Data);
                        dataReader.ReadBytes(bytes);
                        return CreateRealMeasurement(bytes);
                        // Info.Text += Environment.NewLine + "co2 = " + state.co2.ToString();
                    }
                    else
                    {
                        //   this.NotifyUser("Invalid input report received");
                        Debug.WriteLine("Invalid input report received");
                    }
                }
                catch
                {
                    myDevice = null;
                    if (Removed != null) Removed();
                    Debug.WriteLine("краааш");
                }
                finally
                {
                  
                 
                }
                return CreateRandomMeasurement();

            }
            else
            {
                //this.NotifyUser("device is NULL");
                Debug.WriteLine("device is NULL");
            }
            return CreateRandomMeasurement();
        }

    }


}
