
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
    public partial class ComputerDetails : UserControl {

        public Computer computer { get; private set; }
        public MainWindow mainWindow { get; set; }

        public ComputerDetails() {
            InitializeComponent();

            aliveButtons = new Button[] {
                button_ShutDown,
                button_Reboot,
                button_OpenDiskC,
                button_OpenDiskD,
                button_OpenDiskE,
                button_JobSchedule,
                button_Installsoftware
            };
            notAliveButtons = new Button[] {
                button_WakeOnLan
            };
        }

        public async void showDetails(Computer c) {
            this.computer = c;

            await updateComputer();
        }

        CancellationTokenSource loggedUserToken;

        /// <summary>
        /// Update the logged users list
        /// </summary>
        private async Task updateLoggedUsers(bool force = false) {
            mainWindow.showLoading();

            if (loggedUserToken != null)
                loggedUserToken.Cancel();

            dataGrid_ConnectedUsers.Items.Clear();

            // If the computer is not alive, quit
            if (!computer.isAlive) {
                mainWindow.hideLoading();
                return;
            }

            loggedUserToken = new CancellationTokenSource();
            var localTs = loggedUserToken;

            IEnumerable<User> loggedUsers;
            try {
                if (checkBox_ShowAllUsers.IsChecked.Value)
                    loggedUsers = await computer.getAllLoggedUsers(force);
                else
                    loggedUsers = await computer.getLoggedUsers(force);

                if (localTs != null && localTs.IsCancellationRequested) {
                    mainWindow.hideLoading();
                    return;
                }

                foreach (var user in loggedUsers) {
                    dataGrid_ConnectedUsers.Items.Add(user);
                }
            } catch(Exception e) {
                mainWindow.errorHandler.addError(e);
            }

            mainWindow.hideLoading();
        }

        int[] loggedUserCollapsableColumns = { 2, 4, 5, 6 ,7 };

        /// <summary>
        /// Update the logged users visibility
        /// </summary>
        private void updateLoggedUsersVisibility() {
            bool show = checkBox_ShowAllColumnsUsers.IsChecked.Value;

            foreach(int i in loggedUserCollapsableColumns)
                dataGrid_ConnectedUsers.Columns[i].Visibility = 
                    show ? Visibility.Visible : Visibility.Hidden;
        }

        CancellationTokenSource installedSofwaresToken;

        /// <summary>
        /// Update the installed software list
        /// </summary>
        private async Task updateInstalledSoftwares(bool force = false) {
            mainWindow.showLoading();

            if (installedSofwaresToken != null)
                installedSofwaresToken.Cancel();

            dataGrid_ShowAllInstalledSoftware.ItemsSource = new List<Software>();
            
            // If the computer is not alive, quit
            if (!computer.isAlive) {
                mainWindow.hideLoading();
                return;
            }

            installedSofwaresToken = new CancellationTokenSource();
            var localTs = installedSofwaresToken;
            
            try {
                IEnumerable<Software> installedSoftwares = await computer.getInstalledSofwares(force);

                if (localTs != null && localTs.IsCancellationRequested) {
                    mainWindow.hideLoading();
                    return;
                }

                if(installedSoftwares != null)
                    dataGrid_ShowAllInstalledSoftware.ItemsSource = installedSoftwares.ToList();
            } catch (Exception e) {
                mainWindow.errorHandler.addError(e);
            }

            mainWindow.hideLoading();
        }

        int[] installedSoftwaresCollapsableColumns = { 3, 4 };

        /// <summary>
        /// Update the software view visibility
        /// </summary>
        private void updateInstalledSoftwaresVisibility() {
            bool show = checkBox_ShowAllInstalledSofwareColumns.IsChecked.Value;

            foreach (int i in installedSoftwaresCollapsableColumns)
                dataGrid_ShowAllInstalledSoftware.Columns[i].Visibility =
                    show ? Visibility.Visible : Visibility.Hidden;
        }

        private Button[] aliveButtons;
        private Button[] notAliveButtons;

        /// <summary>
        /// Update the computer information panel
        /// </summary>
        private async Task updateComputerInformations() {
            mainWindow.showLoading();

            label_ClientName.Content = computer.name + (computer.isAlive ? "" : " (offline)");
            textBox_OperatingSystem.Text = computer.os;
            textBox_OperatingSystemVersion.Text = computer.version;
            textBox_AdressMac.Text = "";
            textBox_IPAdress.Text = "";

            if(computer.isAlive) {
                aliveButtons.ToList().ForEach(button => button.Visibility = Visibility.Visible);
                notAliveButtons.ToList().ForEach(button => button.Visibility = Visibility.Collapsed);

                try {
                    // Connected : get the IP from DNS, and the MAC from WMI
                    textBox_IPAdress.Text = computer.getIpAddress().ToString();
                    textBox_AdressMac.Text = await computer.getMacAddress();
                } catch (Exception e) {
                    mainWindow.errorHandler.addError(e);
                }
            } else {
                aliveButtons.ToList().ForEach(button => button.Visibility = Visibility.Collapsed);
                notAliveButtons.ToList().ForEach(button => button.Visibility = Visibility.Visible);

                // Not connected : get the IP and MAC from local database
                var computerInfo = mainWindow.computerInfoStore.getComputerInfoByName(computer.nameLong);

                if (computerInfo != null) {
                    textBox_IPAdress.Text = computerInfo.ipAddress;
                    textBox_AdressMac.Text = computerInfo.macAddress;
                }
            }

            mainWindow.hideLoading();
        }

        /// <summary>
        /// Update all the information of the selected computer
        /// </summary>
        private async Task updateComputer() {
            await Task.WhenAll(updateComputerInformations(), updateLoggedUsers(), updateInstalledSoftwares());
        }
        
        private void button_Installsoftware_Click(object sender, RoutedEventArgs e) {
            if (computer != null) {
                SoftwareDeployment softwareDeployment = new SoftwareDeployment(mainWindow.errorHandler, computer);
                softwareDeployment.Left = mainWindow.Left + 50;
                softwareDeployment.Top = mainWindow.Top + 50;
                softwareDeployment.Show();
            }
        }

        private void button_JobSchedule_Click(object sender, RoutedEventArgs e) {
            if (computer != null) {
                // TODO
            }
        }

        private async void button_ShutDown_Click(object sender, RoutedEventArgs e) {
            if (computer == null)
                return;

            if (computer.isLocalComputer()) {
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to shutdown the local computer ({computer.name}) ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return;
            }

            try {
                await computer.shutdown();
            } catch (Exception ex) {
                mainWindow.errorHandler.addError(ex);
            }
        }

        private async void button_Reboot_Click(object sender, RoutedEventArgs e) {
            if (computer == null || !computer.isAlive)
                return;

            if (computer.isLocalComputer()) {
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to reboot the local computer ({computer.name}) ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return;
            }

            try {
                await computer.reboot();
            } catch (Exception ex) {
                mainWindow.errorHandler.addError(ex);
            }
        }

        private void button_StartRemoteDesktop_Click(object sender, RoutedEventArgs e) {
            if (computer != null) {
                computer.startRemoteDesktop();
            }
        }

        private async void checkBox_ShowAllUsers_Click(object sender, RoutedEventArgs e) {
            if (computer == null || !computer.isAlive)
                return;

            await updateLoggedUsers();
        }

        private void checkBox_ShowAllColumnsUser_Click(object sender, RoutedEventArgs e) {
            updateLoggedUsersVisibility();
        }

        private void checkBox_ShowAllColumnsUsers_Click(object sender, RoutedEventArgs e) {
            updateLoggedUsersVisibility();
        }

        private void checkBox_ShowAllInstalledSofwareColumns_Click(object sender, RoutedEventArgs e) {
            updateInstalledSoftwaresVisibility();
        }

        private void CommandBinding_CopyLine(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) {
            sender.ToString();
        }

        private async void button_ConnectedUsersReload_Click(object sender, RoutedEventArgs e) {
            if (computer == null || !computer.isAlive)
                return;
            
            await updateLoggedUsers(true);
        }

        private async void button_InstalledSoftwaresReload_Click(object sender, RoutedEventArgs e) {
            if (computer == null || !computer.isAlive)
                return;
            
            await updateInstalledSoftwares(true);
        }
        
        private void openDisk(string path) {
            string remotePath = computer.getPath(path);

            if (Directory.Exists(remotePath)) {
                Utils.showExplorer(remotePath);
            } else {
                MessageBox.Show($"The directory {path} is not accessible for the host {computer.name}", "Disk unavailable");
            }
        }

        private void button_OpenDiskC_Click(object sender, RoutedEventArgs e) {
            if (computer == null || !computer.isAlive)
                return;

            openDisk(@"C:\");
        }

        private void button_OpenDiskD_Click(object sender, RoutedEventArgs e) {
            if (computer == null || !computer.isAlive)
                return;

            openDisk(@"D:\");
        }

        private void button_OpenDiskE_Click(object sender, RoutedEventArgs e) {
            if (computer == null || !computer.isAlive)
                return;

            openDisk(@"E:\");
        }

        private void button_WakeOnLan_Click(object sender, RoutedEventArgs e) {
            if(computer != null) {
                var computerInfo = mainWindow.computerInfoStore.getComputerInfoByName(computer.nameLong);

                if (computerInfo != null) {
                    Utils.wakeOnLan(computerInfo.macAddress);
                }
            }
        }
    }
}
