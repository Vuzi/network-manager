using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace NetworkManager.View.Component.Job {

    /// <summary>
    /// Logique d'interaction pour TaskReboot.xaml
    /// </summary>
    public partial class TaskInstall : UserControl {

        private static string path = @"C:\apps\";
        public static string name { get; } = "Install software";
        public JobSchedulerWindow mainWindow { get; set; }

        public TaskInstall() {
            InitializeComponent();
        }

        private void AppsToDeploy_Loaded(object sender, RoutedEventArgs e) {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string[] dir = Directory.GetFiles(path);
            var data = new List<SoftwareModel>();
            foreach (var filePath in dir) {

                var sysicon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                var bmpSrc = Imaging.CreateBitmapSourceFromHIcon(sysicon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                sysicon.Dispose();

                FileInfo fileInfo = new FileInfo(filePath);
                var itm = new SoftwareModel();
                itm.icon = bmpSrc;
                itm.fileInfo = fileInfo;
                itm.size = Utils.getBytesReadable(fileInfo.Length);
                itm.file = fileInfo.Name;
                if (fileInfo.Extension == ".msi") {
                    itm.name = Utils.getMsiProperty(filePath, "ProductName");
                    itm.version = Utils.getMsiProperty(filePath, "ProductVersion");
                    itm.company = Utils.getMsiProperty(filePath, "Manufacturer");
                } else {
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);

                    itm.name = fileVersionInfo.ProductName?.Trim();
                    itm.version = fileVersionInfo.FileVersion;
                    itm.company = fileVersionInfo.CompanyName?.Trim();
                }
                data.Add(itm);
            }

            dataGrid_SoftwareList.ItemsSource = data;
        }

        private void buttonDown_Click(object sender, RoutedEventArgs e) {
            mainWindow.downTask(this);
        }

        private void buttonUp_Click(object sender, RoutedEventArgs e) {
            mainWindow.upTask(this);
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e) {
            mainWindow.deleteTask(this);
        }
    }
}
