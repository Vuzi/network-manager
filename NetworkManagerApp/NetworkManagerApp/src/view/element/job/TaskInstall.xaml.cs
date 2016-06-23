﻿using NetworkManager.Job;
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

        private static string path = @"C:\apps\";
        public static string name { get; } = "Install software";
        public JobSchedulerWindow mainWindow { get; set; }

        public TaskInstall() {
            InitializeComponent();
        }

        private void DataGrid_softwareList_Loaded(object sender, RoutedEventArgs e) {
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

        private void button_Down_Click(object sender, RoutedEventArgs e) {
            mainWindow.downTask(this);
        }

        private void button_Up_Click(object sender, RoutedEventArgs e) {
            mainWindow.upTask(this);
        }

        private void button_Delete_Click(object sender, RoutedEventArgs e) {
            mainWindow.deleteTask(this);
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

            string data = (DataGrid_softwareList.SelectedItem as SoftwareModel).path + " " + Textbox_launchArgs.Text;

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
                data = data
            };
        }
    }
}