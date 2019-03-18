using HT2000Viewer.Common;
using HT2000Viewer.Controls;
using HT2000Viewer.Models;
using HT2000Viewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace HT2000Viewer
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class GraphsPage : Page
    {
        public MainViewModel ViewModel => App.ViewModel;

        public GraphsPage()
        {
            this.InitializeComponent();
      //      this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

        }
        DispatcherTimer _timer = new DispatcherTimer();

        private  void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public QTimeAxis TimeAxis { get; set; }
        public ObservableCollection<Measurement> SensorData { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                string navTag = (string)e.Parameter;

 
                switch (navTag)
                {
                    case "fast":
                        TimeAxis = new QTimeAxis(ViewModel.warehouse.mc[0].TimeSpan);
                        SensorData = ViewModel.warehouse.mc[0].MeasurementData;
                        break;
                    case "normal":
                        TimeAxis = new QTimeAxis(ViewModel.warehouse.mc[1].TimeSpan);
                        SensorData = ViewModel.warehouse.mc[1].MeasurementData;
                        break;
                    case "slow":
                        TimeAxis = new QTimeAxis(ViewModel.warehouse.mc[2].TimeSpan);
                        SensorData = ViewModel.warehouse.mc[2].MeasurementData;
                        break;
                    case "quarter":
                        TimeAxis = new QTimeAxis(ViewModel.warehouse.mc[3].TimeSpan);
                        SensorData = ViewModel.warehouse.mc[3].MeasurementData;
                        break;
                    case "day":
                        TimeAxis = new QTimeAxis(ViewModel.warehouse.mc[4].TimeSpan);
                        SensorData = ViewModel.warehouse.mc[4].MeasurementData;
                        break;
                    case "week":
                        TimeAxis = new QTimeAxis(ViewModel.warehouse.mc[5].TimeSpan);
                        SensorData = ViewModel.warehouse.mc[5].MeasurementData;
                        break;
                }
            }
        }
        

    }
}
