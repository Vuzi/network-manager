
using NetworkManager.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.IO;

namespace NetworkManager.View.Component {
    /// <summary>
    /// Logique d'interaction pour UserControl1.xaml
    /// </summary>
    public partial class ComputersDetails : UserControl {

        public List<Computer> computers { get; private set; }
        public MainWindow mainWindow { get; set; }

        public ComputersDetails() {
            InitializeComponent();
        }

        public async void showDetails(List<Computer> computers) {
            this.computers = computers;

            label_NumberComputersSelected.Content = $"{computers.Count} Computers Selected";

            await updateComputers();
        }

        CancellationTokenSource computersToken;

        /// <summary>
        /// Update the logged users list
        /// </summary>
        private async Task updateComputers() {
            mainWindow.showLoading();

            if (computersToken != null)
                computersToken.Cancel();

            dataGrid_Computers.Items.Clear();

            computersToken = new CancellationTokenSource();
            var localTs = computersToken;
            
            try {
                List<ComputerDetailModel> models = new List<ComputerDetailModel>();

                foreach (Computer computer in computers) {
                    string ipAddress = null;
                    string macAddress = null;

                    try {
                        ipAddress = computer.getIpAddress().ToString();
                        macAddress = await computer.getMacAddress();
                    } catch(Exception e) {
                        mainWindow.errorHandler.addError(e);
                    }

                    models.Add(new ComputerDetailModel() {
                        computer = computer,
                        IPAddress = ipAddress,
                        MACAddress = macAddress
                    });
                }

                if (localTs != null && localTs.IsCancellationRequested) {
                    mainWindow.hideLoading();
                    return;
                }

                foreach (var model in models)
                    dataGrid_Computers.Items.Add(model);
                
            } catch(Exception e) {
                mainWindow.errorHandler.addError(e);
            }

            mainWindow.hideLoading();
        }

        int[] computersCollapsableColumns = {  };

        /// <summary>
        /// Update the computers visibility
        /// </summary>
        private void updateComputersVisibility() {
            bool show = checkBox_ShowAllColumnsComputers.IsChecked.Value;

            foreach(int i in computersCollapsableColumns)
                dataGrid_Computers.Columns[i].Visibility = 
                    show ? Visibility.Visible : Visibility.Hidden;
        }
        
        private void button_Installsoftware_Click(object sender, RoutedEventArgs e) {

        }

        private void button_JobSchedule_Click(object sender, RoutedEventArgs e) {

        }

        private void button_ShutDown_Click(object sender, RoutedEventArgs e) {

        }

        private void button_Reboot_Click(object sender, RoutedEventArgs e) {

        }

        private void button_WakeOnLan_Click(object sender, RoutedEventArgs e) {

        }

        private void checkBox_ShowAllColumnsComputers_Click(object sender, RoutedEventArgs e) {

        }

        private void button_ComputersReload_Click(object sender, RoutedEventArgs e) {

        }

        private void button_ComputersReload_Click_1(object sender, RoutedEventArgs e) {

        }
    }

    public class ComputerDetailModel {
        public Computer computer { get; set; }
        public string IPAddress { get; set; }
        public string MACAddress { get; set; }
    }
}
