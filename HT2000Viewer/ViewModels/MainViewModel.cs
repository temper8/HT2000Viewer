using HT2000Viewer.Common;
using HT2000Viewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HT2000Viewer.ViewModels
{
    public class MainViewModel : Observable
    {
        public Warehouse warehouse { get; } = new Warehouse();
        public HT2000Monitor ht2000 { get; } = new HT2000Monitor();

        public MqttConnection Mqtt { get; } = new MqttConnection();
        public MainViewModel() {
            Application.Current.Suspending += new SuspendingEventHandler(App_Suspending);
            Application.Current.Resuming += new EventHandler<Object>(App_Resuming);

            ht2000.OnPushData += warehouse.AddState;
            ht2000.OnPushData += Mqtt.PublishState;
            ht2000.Inserted = OnDeviceInserted;
            ht2000.Removed = OnDeviceRemoved;

            ht2000.Initialize();
        }
        
        public async void ResetData()
        {
            warehouse.DropCollections();

            ContentDialog dialog = new ContentDialog()
            {
                Title = "All data has been deleted.",
                //Content = "All data has been deleted.",
                CloseButtonText = "Ok"
            };

            await dialog.ShowAsync();
        }

        private void App_Resuming(object sender, object e)
        {

            //throw new NotImplementedException();
            ht2000.OnResuming();
        }

        private void App_Suspending(object sender, SuspendingEventArgs e)
        {

            ht2000.OnSuspending();
            // throw new NotImplementedException();
        }
        Visibility _MQTTstatusVisibility;
        public Visibility MQTTstatusVisibility
        {
            get => _MQTTstatusVisibility;
            set => Set(ref _MQTTstatusVisibility, value);
        }


        Visibility _WarningVisibility;
        public Visibility WarningVisibility
        {
            get => _WarningVisibility;
            set => Set(ref _WarningVisibility, value);
        }

        async void OnDeviceInserted()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //   WarningBlock.Visibility = Visibility.Collapsed;
                WarningVisibility = Visibility.Collapsed;
            });
            ht2000.Connect();
        }

        async void OnDeviceRemoved()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                WarningVisibility = Visibility.Visible;
            });
        }

    }

}
