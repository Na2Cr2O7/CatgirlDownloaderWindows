using CatgirlDownloader;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Windows.Storage.Pickers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using WinRT.Interop;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using System.Net.Http;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CatGirlDownloaderWindows2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class mainpage : Page
    {
        Option option;
        public CatgirlDownloaderAPI catgirlDownloader;
        private BitmapImage bitmapImage;
        private byte[] imageBytes;
        private DispatcherTimer autoReloadTimer;
        nint hwnd;
        public mainpage(ref CatgirlDownloaderAPI catgirlDownloader,ref Option option,ref nint hwnd)
        {
            InitializeComponent();
            this.catgirlDownloader = catgirlDownloader;
            this.option = option;
            this.hwnd=hwnd;
           
           

            //DownloadButton.Content = url;



            NewImage();
            AutoReloadSwitch.IsOn = option.Items!.Auto_reload_enabled;
            autoReloadTimer = new()
            {
                Interval = System.TimeSpan.FromSeconds(option.Items!.Auto_reload_interval),

            };
            autoReloadTimer.Tick += AutoReloadTimer_Tick;
            if (option.Items!.Auto_reload_enabled)
            {
                autoReloadTimer.Start();
            }
        }
        private void AutoReloadTimer_Tick(object? sender, object e)
        {
            NewImage();
        }

        private async void NewImage()
        {
            Progress.IsActive = true;
            string url = catgirlDownloader.GetImageUrl(option.Items!.Nsfw_mode!);
            //BitmapImage bitmapImage = new()
            //{
            //    UriSource = new System.Uri(Display.BaseUri, url)
            //};
            //Display.Source = bitmapImage;
            using var httpClient = new HttpClient();
            imageBytes = await httpClient.GetByteArrayAsync(url);

            using (var stream = new MemoryStream(imageBytes))
            {
                bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(stream.AsRandomAccessStream());
            }
            Display.Source = bitmapImage;
            Progress.IsActive = false;
            if (AutoReloadSwitch.IsOn)
            {
                autoReloadTimer.Interval= System.TimeSpan.FromSeconds(option.Items!.Auto_reload_interval);
                autoReloadTimer.Start();
            }
            else
            {
                autoReloadTimer.Stop();
            }
        }
        private static string ComputeMd5(byte[] data)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            autoReloadTimer.Stop();

            NewImage();
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            if (imageBytes is null)
            {
                return;
            }
            
            InitializeWithWindow.Initialize(savePicker, hwnd);

            // Configure picker
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            savePicker.SuggestedFileName = ComputeMd5(imageBytes);
            savePicker.FileTypeChoices.Add("JPEG Image", new List<string> { ".jpg", ".jpeg" });
            savePicker.DefaultFileExtension = ".jpg";

            // Show save picker
            var file = await savePicker.PickSaveFileAsync();
            if (file == null)
                return; // User canceled

            try
            {
                using var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                await fileStream.WriteAsync(imageBytes.AsBuffer());
                await fileStream.FlushAsync();
            }
            catch (System.Exception ex)
            {
                // Handle errors (network issue, invalid URL, etc.)
                // For example:
                // Debug.WriteLine($"Failed to download/save image: {ex.Message}");
                // Show error dialog or info bar
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Display_ImageOpened(object sender, RoutedEventArgs e)
        {
            Progress.IsActive = false;
            if (AutoReloadSwitch.IsOn)
            {
                autoReloadTimer.Start();
            }
            else
            {
                autoReloadTimer.Stop();
            }
        }


        private void AutoReloadSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            option.Items!.Auto_reload_enabled = AutoReloadSwitch.IsOn;
            option.Save();
            if (autoReloadTimer is null)
            {
                return;
            }
            if (AutoReloadSwitch.IsOn)
            {
                autoReloadTimer.Start();
            }
            else
            {
                autoReloadTimer.Stop();
            }
        }
    }
}
