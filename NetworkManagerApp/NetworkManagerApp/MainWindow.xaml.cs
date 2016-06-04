using NetworkManager.Domain;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using NetworkManagerApp.view;

namespace NetworkManager {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        /// <summary>
        /// Selected computer
        /// </summary>
        private Computer computer;

        /// <summary>
        /// Error handler
        /// </summary>
        private ErrorHandler errorHandler;

        public MainWindow() {
            InitializeComponent();
        }

        /// <summary>
        /// Update the domain computer list
        /// </summary>
        private async Task updateListComputers() {
            showLoading();

            try {
                List_Computer.Items.Clear();

                // Only one domain for now
                Domain.Domain domain = new Domain.Domain();
                await domain.fill();
                DomainModel domainModel = new DomainModel() {
                    name = domain.name,
                    computers = new ObservableCollection<ComputerModel>(
                        domain.computers.Select(c => new ComputerModel() { computer = c }))
                };

                List_Computer.Items.Add(domainModel);

                // Expand
                TreeViewItem item = (TreeViewItem)List_Computer.ItemContainerGenerator.ContainerFromItem(domainModel);
                item.IsExpanded = true;

            } catch(Exception e) {
                errorHandler.addError(e);
            }

            hideLoading();
        }
        
        CancellationTokenSource loggedUserToken;

        /// <summary>
        /// Update the logged users list
        /// </summary>
        private async Task updateLoggedUsers() {
            showLoading();

            if (loggedUserToken != null)
                loggedUserToken.Cancel();
            
            loggedUserToken = new CancellationTokenSource();
            var localTs = loggedUserToken;

            dataGrid_ConnectedUsers.Items.Clear();

            IEnumerable<User> loggedUsers;
            try {
                if (checkBox_ShowAllUsers.IsChecked.Value)
                    loggedUsers = await computer.getAllLoggedUsers();
                else
                    loggedUsers = await computer.getLoggedUsers();

                if (localTs != null && localTs.IsCancellationRequested) {
                    hideLoading();
                    return;
                }

                foreach (var user in loggedUsers) {
                    dataGrid_ConnectedUsers.Items.Add(user);
                }
            } catch(Exception e) {
                errorHandler.addError(e);
            }

            hideLoading();
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
        private async Task updateInstalledSoftwares() {
            showLoading();

            if (installedSofwaresToken != null)
                installedSofwaresToken.Cancel();

            installedSofwaresToken = new CancellationTokenSource();
            var localTs = installedSofwaresToken;

            dataGrid_ShowAllInstalledSoftware.ItemsSource = new List<Software>();
            
            try {
                IEnumerable<Software> installedSoftwares = await computer.getInstalledSofwares();

                if (localTs != null && localTs.IsCancellationRequested) {
                    hideLoading();
                    return;
                }

                if(installedSoftwares != null)
                    dataGrid_ShowAllInstalledSoftware.ItemsSource = installedSoftwares.ToList();
            } catch (Exception e) {
                errorHandler.addError(e);
            }

            hideLoading();
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

        /// <summary>
        /// Update the computer information panel
        /// </summary>
        private async Task updateComputerInformations() {
            showLoading();

            label_ClientName.Content = computer.name;
            textBox_OperatingSystem.Text = computer.os;
            textBox_OperatingSystemVersion.Text = computer.version;
            textBox_AdressMac.Text = "";
            textBox_IPAdress.Text = "";

            if(computer.isAlive) {
                button_ShutDown.Visibility = Visibility.Visible;
                button_WakeOnLan.Visibility = Visibility.Collapsed;
            } else {
                button_ShutDown.Visibility = Visibility.Collapsed;
                button_WakeOnLan.Visibility = Visibility.Visible;
            }

            try {
                textBox_IPAdress.Text = computer.getIpAddress().ToString();
                textBox_AdressMac.Text = await computer.getMacAddress();
            } catch (Exception e) {
                errorHandler.addError(e);
            }

            hideLoading();
        }

        /// <summary>
        /// Update all the information of the selected computer
        /// </summary>
        private async Task updateComputer() {
            await Task.WhenAll(updateComputerInformations(), updateLoggedUsers(), updateInstalledSoftwares());
        }

        int loadingWaiting = 0;

        /// <summary>
        /// Show the loading icon
        /// </summary>
        private void showLoading() {
            loadingWaiting++;
            LoadingSpinner.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hide the loading icon
        /// </summary>
        private void hideLoading() {
            if(--loadingWaiting == 0)
                LoadingSpinner.Visibility = Visibility.Collapsed;
        }

        private async void List_Computer_Selected(object sender, RoutedEventArgs eventArgs) {
            TreeViewItem tvi = eventArgs.OriginalSource as TreeViewItem;

            if (tvi.DataContext is ComputerModel) {
                this.computer = (tvi.DataContext as ComputerModel).computer;

                await updateComputer();
            }
        }

        private void button_Installsoftware_Click(object sender, RoutedEventArgs e) {
            if (computer != null) {
                // TODO
            }
        }

        private void button_JobSchedule_Click(object sender, RoutedEventArgs e) {
            if (computer != null) {
                // TODO
            }
        }

        private void button_ShutDown_Click(object sender, RoutedEventArgs e) {
            if (computer != null) {
                computer.shutdown();
            }
        }

        private void button_Reboot_Click(object sender, RoutedEventArgs e) {
            if (computer != null) {
                computer.reboot();
            }
        }

        private void button_StartRemoteDesktop_Click(object sender, RoutedEventArgs e) {
            if (computer != null) {
                computer.startRemoteDesktop();
            }
        }

        private async void List_Machine_Loaded(object sender, RoutedEventArgs e) {
            // Update error panel
            errorHandler = new ErrorHandler();
            errorHandler.warningIndicator = WarningImage;

            await updateListComputers();
        }

        private async void checkBox_ShowAllUsers_Click(object sender, RoutedEventArgs e) {
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
            if (computer == null)
                return;
            
            await updateLoggedUsers();
        }

        private async void button_InstalledSoftwaresReload_Click(object sender, RoutedEventArgs e) {
            if (computer == null)
                return;
            
            await updateInstalledSoftwares();
        }

        private async void button_ComputersReload_Click(object sender, RoutedEventArgs e) {
            await updateListComputers();
        }

        private void WarningImage_Click(object sender, RoutedEventArgs e) {
            errorHandler.Left = this.Left + 50;
            errorHandler.Top = this.Top + 50;
            errorHandler.Show();
        }

        private void button_OpenDiskC_Click(object sender, RoutedEventArgs e) {
            if (computer != null) {
                computer.showExplorer("c");
            }
        }

        private void button_OpenDiskD_Click(object sender, RoutedEventArgs e) {
            if (computer != null) {
                computer.showExplorer("d");
            }
        }

        private void button_OpenDiskE_Click(object sender, RoutedEventArgs e) {
            if (computer != null) {
                computer.showExplorer("e");
            }
        }

        private void button_WakeOnLan_Click(object sender, RoutedEventArgs e) {
            if(computer != null) {
                computer.isAlive = !computer.isAlive;
            }
        }
    }

    public class DomainModel {
        public string name { get; set; }
        public ObservableCollection<ComputerModel> computers { get; set; }

        public DomainModel() {
            this.computers = new ObservableCollection<ComputerModel>();
        }
    }

    public class ComputerModel {
        public Computer computer { get; set; }
    }

}