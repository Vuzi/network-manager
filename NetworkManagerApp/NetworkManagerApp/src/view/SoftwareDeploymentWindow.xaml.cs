using Microsoft.Win32;
using NetworkManager.DomainContent;
using NetworkManager.WMIExecution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using static NetworkManager.Utils;

namespace NetworkManager.View {
    /// <summary>
    /// Logique d'interaction pour SoftwareDeployment.xaml
    /// </summary>
    public partial class SoftwareDeploymentWindow : Window {
        
        private List<Computer> computers;
        private ErrorHandlerWindow errorHandler;

        public SoftwareDeploymentWindow(ErrorHandlerWindow errorHandler, List<Computer> computers) {
            InitializeComponent();

            this.errorHandler = errorHandler;
            this.computers = computers;
            this.Label_ComputerName.Content = $"Software Deployment on {(computers.Count == 1 ? computers[0].name : computers.Count + " computers" )}";
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

            File.Copy(ofd.FileName, MainWindow.config.get("softwarepath") + "/" + Path.GetFileName(ofd.FileName), true);
            AppsToDeploy_Loaded(this, null);
        }
        
        private Dictionary<string, string> subDirectoryParameters = new Dictionary<string, string>() {
            { "FreewareVS", "/VERYSILENT" },
            { "FreewareS", "/S" },
            { "MSI", "" }
        };

        private void AppsToDeploy_Loaded(object sender, RoutedEventArgs e) {
            string path = MainWindow.config.get("softwarepath");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            List<string> dirs = new List<string>();
            dirs.AddRange(Directory.GetFiles(path));

            foreach (var sub in subDirectoryParameters) {
                string subPath = $@"{path}\{sub.Key}";

                if (!Directory.Exists(subPath))
                    Directory.CreateDirectory(subPath);

                dirs.AddRange(Directory.GetFiles(subPath));
            }

            dirs.Sort();

            var data = new List<SoftwareModel>();

            foreach (var filePath in dirs) {
                var sysicon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                var bmpSrc = Imaging.CreateBitmapSourceFromHIcon(sysicon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                sysicon.Dispose();

                FileInfo fileInfo = new FileInfo(filePath);
                var itm = new SoftwareModel();
                itm.icon = bmpSrc;
                itm.fileInfo = fileInfo;
                itm.size = Utils.getBytesReadable(fileInfo.Length);
                itm.file = fileInfo.Name;
                itm.path = filePath;
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

            DataGrid_SoftwareList.ItemsSource = data;
        }

        private async void ButtonLaunch_Click(object sender, RoutedEventArgs eventArg) {
            SoftwareModel soft = (SoftwareModel)DataGrid_SoftwareList.SelectedItem;

            if (soft == null)
                return;

            showLoading();

            try {
                int timeout = int.Parse(TextBox_Timeout.Text);
                string path = soft.path;
                string args = TextBox_LaunchArgs.Text;

                var tasks = computers.Select(async (computer) => {
                    var r = new ValueOrError<WMIExecutionResult, Exception>();
                    try {
                        r.value = await Task.Run(async () => {
                            // Install on the current computer
                            return await computer.installSoftware(path, new string[] { args }, timeout);
                        });
                    } catch (Exception e) {
                        r.error = e;
                    }

                    return r;
                });

                var results = await Task.WhenAll(tasks);
                var reports = new List<WMIExecutionResult>();

                foreach(var result in results) {
                    if (result.hasValue())
                        reports.Add(result.value);
                    else {
                        errorHandler.addError(result.error);
                        WarningImage.Visibility = Visibility.Visible;
                    }
                }

                // Show reports
                var reporter = new ExecutionReportsWindow(reports);
                reporter.Left = this.Left + 50;
                reporter.Top = this.Top + 50;
                reporter.Show();
            } catch (Exception e) {
                WarningImage.Visibility = Visibility.Visible;
                errorHandler.addError(e);
            } finally {
                hideLoading();
            }
        }

        private void WarningImage_Click(object sender, RoutedEventArgs e) {
            errorHandler.Left = this.Left + 50;
            errorHandler.Top = this.Top + 50;
            errorHandler.Show();
        }
        
        private void dataGrid_SoftwareList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e) {
            SoftwareModel soft = (SoftwareModel)DataGrid_SoftwareList.SelectedItem;

            if (soft == null)
                return;

            string arg = subDirectoryParameters.GetValueOrDefault(Directory.GetParent(soft.path).Name);

            TextBox_LaunchArgs.Text = arg == null ? "" : arg;
            TextBox_LaunchArgs.IsEnabled = !(soft.path.ToLower().EndsWith(".msi"));
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
