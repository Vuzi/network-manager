using NetworkManager.Scheduling;
using System;
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
        
        public static string name { get; } = "Install software";
        public JobDetails mainWindow { get; set; }

        public TaskInstall() {
            InitializeComponent();
        }

        private Dictionary<string, string> subDirectoryParameters = new Dictionary<string, string>() {
            { "FreewareVS", "/VERYSILENT" },
            { "FreewareS", "/S" },
            { "MSI", "" }
        };

        private void DataGrid_softwareList_Loaded(object sender, RoutedEventArgs e) {

            string path = App.config.get("softwarepath");

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
                itm.path = filePath;
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

            DataGrid_softwareList.ItemsSource = data;
        }

        private void DataGrid_softwareList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e) {
            SoftwareModel soft = (SoftwareModel)DataGrid_softwareList.SelectedItem;

            if (soft == null)
                return;

            string arg = subDirectoryParameters.GetValueOrDefault(Directory.GetParent(soft.path).Name);

            Textbox_launchArgs.Text = arg == null ? "" : arg;
            Textbox_launchArgs.IsEnabled = !(soft.path.ToLower().EndsWith(".msi"));
        }

        private void button_Down_Click(object sender, RoutedEventArgs e) {
            mainWindow.downTask(this);
        }

        private void button_Up_Click(object sender, RoutedEventArgs e) {
            mainWindow.upTask(this);
        }

        private void button_Delete_Click(object sender, RoutedEventArgs e) {
            mainWindow.deleteTask(this);
        }

        public void initFromTask(JobTask task) {
            // Select
            DataGrid_softwareList.SelectedItem = null;
            foreach (SoftwareModel item in DataGrid_softwareList.Items) {
                if(item.path == task.data) {
                    DataGrid_softwareList.SelectedItem = item;
                    break;
                }
            }

            // Arguments
            Textbox_launchArgs.Text = task.data2;

            // Timeout
            Textbox_timeout.Text = task.timeout.ToString();
        }

        /// <summary>
        /// Create the task from the panel
        /// </summary>
        /// <returns></returns>
        public JobTask createTask() {

            // Data
            if (DataGrid_softwareList.SelectedItem == null) {
                MessageBox.Show("Error : No software selected", "Installation task error");
                return null;
            }

            string data = (DataGrid_softwareList.SelectedItem as SoftwareModel).path;
            string data2 = Textbox_launchArgs.Text;

            // Timeout
            int timeout = 60;

            try {
                timeout = int.Parse(Textbox_timeout.Text);
            } catch (Exception) {
                MessageBox.Show("Error : Invalid timeout value", "Installation task error");
                return null;
            }
            
            return new JobTask() {
                type = JobTaskType.INSTALL_SOFTWARE,
                timeout = timeout,
                data = data,
                data2 = data2 // TODO remove data 2 ?
            };
        }
    }
}
