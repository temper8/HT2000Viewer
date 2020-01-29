using HT2000Viewer.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
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
    public sealed partial class SettingsPage : Page
    {
        public MainViewModel ViewModel => App.ViewModel;
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        public string AppVersion
        {
            get {
                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;

                return string.Format("HT2000 Viewer {0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }  }

        public string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog ResetDialog = new ContentDialog()
            {
                Title = "Reset data",
                Content = "All recorded data will be deleted",
                PrimaryButtonText = "Reset",
                SecondaryButtonText = "Cancel"
            };

            ContentDialogResult result = await ResetDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                App.ViewModel.ResetData();
            }
        }
    }
}
