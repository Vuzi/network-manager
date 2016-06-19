using Microsoft.Win32;
using NetworkManager.DomainContent;
using NetworkManager.WMIExecution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace NetworkManager.View {
    /// <summary>
    /// Logique d'interaction pour SoftwareDeployment.xaml
    /// </summary>
    public partial class SoftwareDeploymentWindow : Window {

        private static string path = @"C:\apps\";
        private Computer currentComputer;
        private ErrorHandlerWindow errorHandler;

        public SoftwareDeploymentWindow(ErrorHandlerWindow errorHandler, Computer computer) {
            InitializeComponent();

            this.errorHandler = errorHandler;
            this.currentComputer = computer;
            this.label_ComputerName.Content = $"Software Deployment on {computer.name}";
        }

        private void showLoading() {
            LoadingSpinner.Visibility = Visibility.Visible;
            this.IsEnabled = false;
        }


        private void hideLoading() {
            LoadingSpinner.Visibility = Visibility.Collapsed;
            this.IsEnabled = true;
        }

        private void ButtonAddApplication_Click(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog() { Filter = "Executable files (*.exe, *.msi)|*.exe;*.msi" };
            var result = ofd.ShowDialog();

            if (result == false)
                return;

            File.Copy(ofd.FileName, path + System.IO.Path.GetFileName(ofd.FileName), true);
            AppsToDeploy_Loaded(this, null);
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

        private async void ButtonLaunch_Click(object sender, RoutedEventArgs eventArg) {
            SoftwareModel soft = (SoftwareModel)dataGrid_SoftwareList.SelectedItem;

            if (soft == null)
                return;

            showLoading();

            int timeout = int.Parse(textBox_Timeout.Text);
            string path = $@"C:\Windows\Temp\{soft.fileInfo.Name}";
            string[] options = new string[] { textBox_LaunchArgs.Text };
            WMIExecutionResult result = null;

            try {
                // Copy the file
                currentComputer.uploadFile(soft.fileInfo.FullName, path);

                try {
                    // Start the install remotely
                    result = await currentComputer.exec(path, options, timeout * 1000);
                } catch(Exception e) {
                    throw e;
                } finally {
                    // Delete the file
                    try {
                        currentComputer.deleteFile(path);
                    } catch(Exception e) {
                        WarningImage.Visibility = Visibility.Visible;
                        errorHandler.addError(new WMIException() {
                            error = e,
                            computer = currentComputer.nameLong
                        });
                    }
                }

            } catch(Exception e) {
                WarningImage.Visibility = Visibility.Visible;
                errorHandler.addError(new WMIException() {
                    error = e,
                    computer = currentComputer.nameLong
                });
            } finally {
                hideLoading();
            }

            if (result != null) {
                ExecutionReport reporter = new ExecutionReport(currentComputer, result);
                reporter.Left = this.Left + 50;
                reporter.Top = this.Top + 50;
                reporter.Show();
            }
        }

        private void WarningImage_Click(object sender, RoutedEventArgs e) {
            errorHandler.Left = this.Left + 50;
            errorHandler.Top = this.Top + 50;
            errorHandler.Show();
        }
    }

    class SoftwareModel {
        public BitmapSource icon { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        public string company { get; set; }
        public string size { get; set; }
        public string file { get; set; }
        public FileInfo fileInfo { get; set; }
        public string path { get; set; }
    }
}
