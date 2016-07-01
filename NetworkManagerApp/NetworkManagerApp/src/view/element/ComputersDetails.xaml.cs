
using NetworkManager.DomainContent;
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
        public async Task updateComputers() {
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

                    // Not connected : get the IP and MAC from local database
                    var computerInfo = App.computerInfoStore.getComputerInfoByName(computer.nameLong);

                    if (computerInfo != null) {
                        ipAddress = computer.getIpAddress().ToString();
                        macAddress = computerInfo.macAddress;
                    } else if(computer.isAlive) {
                        try {
                            // Connected : get the IP from DNS, and the MAC from WMI
                            ipAddress = computer.getIpAddress().ToString();
                            macAddress = await computer.getMacAddress();
                        } catch (Exception e) {
                            mainWindow.errorHandler.addError(e);
                        }
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

        int[] computersCollapsableColumns = { 2 };

        /// <summary>
        /// Update the computers visibility
        /// </summary>
        private void updateComputersVisibility() {
            bool show = checkBox_ShowAllColumnsComputers.IsChecked.Value;

            foreach(int i in computersCollapsableColumns)
                dataGrid_Computers.Columns[i].Visibility = 
                    show ? Visibility.Visible : Visibility.Hidden;
        }

        private async void button_ShutDown_Click(object sender, RoutedEventArgs e) {
            List<Computer> toShutdown = computers.FindAll(c => c.isAlive);

            // If no computer is alive
            if (toShutdown.Count == 0) {
                MessageBox.Show($"There is no computer to stop");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                toShutdown.Count > 1 ?
                $"Are you sure you want to shutdown these {toShutdown.Count} computers?" :
                $"Are you sure you want to shutdown this computer?", "Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;

            // If local
            foreach(Computer c in toShutdown) {
                if (c.isLocalComputer()) {
                    MessageBoxResult r = MessageBox.Show($"Are you sure you want to shutdown the local computer ({c.name}) ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (r == MessageBoxResult.No)
                        return;
                }
            }

            await Task.Factory.StartNew(() => {
                Parallel.ForEach(toShutdown, computer => {
                    try {
                        computer.shutdown().Wait();
                    } catch (Exception ex) {
                        mainWindow.errorHandler.addError(ex);
                    }
                });
            });
            mainWindow.requireReload(10);
        }

        private async void button_Reboot_Click(object sender, RoutedEventArgs e) {
            List<Computer> toReboot = computers.FindAll(c => c.isAlive);

            // If no computer is alive
            if(toReboot.Count == 0) {
                MessageBox.Show($"There is no computer to reboot");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                toReboot.Count > 1 ?
                $"Are you sure you want to reboot these {toReboot.Count} computers?" :
                $"Are you sure you want to reboot this computer?", "Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;

            // If local
            foreach (Computer c in toReboot) {
                if (c.isLocalComputer()) {
                    MessageBoxResult r = MessageBox.Show($"Are you sure you want to reboot the local computer ({c.name}) ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (r == MessageBoxResult.No)
                        return;
                }
            }

            await Task.Factory.StartNew(() => {
                Parallel.ForEach(toReboot, computer => {
                    try {
                        computer.reboot().Wait();
                    } catch (Exception ex) {
                        mainWindow.errorHandler.addError(ex);
                    }
                });
            });
            mainWindow.requireReload(10);
        }

        private async void button_WakeOnLan_Click(object sender, RoutedEventArgs e) {
            List<Computer> toWakeOnLan = computers.FindAll(c => !c.isAlive);

            // If no computer is off
            if (toWakeOnLan.Count == 0) {
                MessageBox.Show($"There is no computer to start");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                toWakeOnLan.Count > 1 ?
                $"Are you sure you want to start these {toWakeOnLan.Count} computers?" :
                $"Are you sure you want to start this computer?", "Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;

            await Task.Factory.StartNew(() => {
                Parallel.ForEach(toWakeOnLan, computer => {
                    try {
                        var computerInfo = App.computerInfoStore.getComputerInfoByName(computer.nameLong);

                        if (computerInfo != null) {
                            Utils.wakeOnLan(computerInfo.macAddress);
                        }
                    } catch (Exception ex) {
                        mainWindow.errorHandler.addError(ex);
                    }
                });
                mainWindow.requireReload(15);
            });
        }

        private void button_JobSchedule_Click(object sender, RoutedEventArgs e) {
            JobSchedulerWindow jobScheduler = new JobSchedulerWindow();
            jobScheduler.Left = mainWindow.Left + 50;
            jobScheduler.Top = mainWindow.Top + 50;
            jobScheduler.selectComputers(computers);
            jobScheduler.Show();
        }

        private void button_Installsoftware_Click(object sender, RoutedEventArgs e) {
            List<Computer> toInstallSoftwareOn = computers.FindAll(c => c.isAlive);

            // If a least one computer is alive
            if (toInstallSoftwareOn.Count > 0) {
                SoftwareDeploymentWindow softwareDeployment = new SoftwareDeploymentWindow(mainWindow.errorHandler, toInstallSoftwareOn);
                softwareDeployment.Left = mainWindow.Left + 50;
                softwareDeployment.Top = mainWindow.Top + 50;
                softwareDeployment.Show();
            }
        }

        private void checkBox_ShowAllColumnsComputers_Click(object sender, RoutedEventArgs e) {
            updateComputersVisibility();
        }

        private async void button_ComputersReload_Click(object sender, RoutedEventArgs e) {
            await updateComputers();
        }
    }

    public class ComputerDetailModel {
        public Computer computer { get; set; }
        public string IPAddress { get; set; }
        public string MACAddress { get; set; }
    }
}
