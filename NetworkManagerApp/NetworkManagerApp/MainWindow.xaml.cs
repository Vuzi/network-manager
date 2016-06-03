using NetworkManager.Domain;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using NetworkManagerApp.views;

namespace NetworkManager {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        /// <summary>
        /// Selected computer
        /// </summary>
        private Computer computer;

        public MainWindow() {
            InitializeComponent();
        }

        private void button_Installsoftware_Click(object sender, RoutedEventArgs e) {
            if(computer != null) {
                computer.showExplorer();
            }
        }

        private void button_JobSchedule_Click(object sender, RoutedEventArgs e) {
            if (computer != null) {
                computer.startRemoteDesktop();
            }
        }

        private void button_ShutDown_Click(object sender, RoutedEventArgs e) {
            if (computer != null) {
                computer.shutdown();
            }
        }

        private void button_CopyReboot_Click(object sender, RoutedEventArgs e) {
            if (computer != null) {
                computer.reboot();
            }
        }

        private async void List_Machine_Loaded(object sender, RoutedEventArgs e) {
            await updateListComputers();
        }

        private async Task updateListComputers() {
            showLoading();

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

            hideLoading();
        }
        
        CancellationTokenSource loggedUserToken;

        private async Task updateLoggedUsers() {
            showLoading();

            if (loggedUserToken != null)
                loggedUserToken.Cancel();
            
            loggedUserToken = new CancellationTokenSource();
            var localTs = loggedUserToken;

            dataGrid_ConnectedUsers.Items.Clear();

            IEnumerable<User> loggedUsers;
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

            hideLoading();
        }

        int[] loggedUserCollapsableColumns = { 2, 4, 5, 6 ,7 };

        private void updateLoggedUsersVisibility() {
            bool show = checkBox_ShowAllColumnsUsers.IsChecked.Value;

            foreach(int i in loggedUserCollapsableColumns)
                dataGrid_ConnectedUsers.Columns[i].Visibility = 
                    show ? Visibility.Visible : Visibility.Hidden;
        }

        CancellationTokenSource installedSofwaresToken;

        private async Task updateInstalledSoftwares() {
            showLoading();

            if (installedSofwaresToken != null)
                installedSofwaresToken.Cancel();

            installedSofwaresToken = new CancellationTokenSource();
            var localTs = installedSofwaresToken;

            dataGrid_ShowAllInstalledSoftware.ItemsSource = new List<Software>();

            IEnumerable<Software> installedSoftwares = await computer.getInstalledSofwares();

            if (localTs != null && localTs.IsCancellationRequested) {
                hideLoading();
                return;
            }

            dataGrid_ShowAllInstalledSoftware.ItemsSource = installedSoftwares.ToList();

            hideLoading();
        }

        int[] installedSoftwaresCollapsableColumns = { 3, 4 };

        private void updateInstalledSoftwaresVisibility() {
            bool show = checkBox_ShowAllInstalledSofwareColumns.IsChecked.Value;

            foreach (int i in installedSoftwaresCollapsableColumns)
                dataGrid_ShowAllInstalledSoftware.Columns[i].Visibility =
                    show ? Visibility.Visible : Visibility.Hidden;
        }

        private async void List_Computer_Selected(object sender, RoutedEventArgs e) {
            TreeViewItem tvi = e.OriginalSource as TreeViewItem;

            if (tvi.DataContext is ComputerModel) {
                this.computer = (tvi.DataContext as ComputerModel).computer;
                try {
                    Dns.GetHostAddresses(this.computer.name).ToList().ForEach(a => Console.WriteLine(a.ToString()));
                    String[] allIPAdress = new string[2];
                    List<IPAddress> ips = Dns.GetHostAddresses(this.computer.name).ToList();
                    textBox_IPAdress6.Text = ips[0].ToString();
                    textBox_IPAdress4.Text = WebUtility.HtmlDecode(ips[1].ToString());
                }
                catch(Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }

                label_ClientName.Content = computer.name;
                textBox_OperatingSystem.Text = computer.os;
                textBox_OperatingSystemVersion.Text = computer.version;
                
                showLoading();

                await Task.WhenAll(updateLoggedUsers(), updateInstalledSoftwares());

                hideLoading();
            }

        }

        int loadingWaiting = 0;

        private void showLoading() {
            loadingWaiting++;
            LoadingSpinner.Visibility = Visibility.Visible;
        }

        private void hideLoading() {
            if(--loadingWaiting == 0)
                LoadingSpinner.Visibility = Visibility.Collapsed;
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

        private void button_AppsDeploy_Click(object sender, RoutedEventArgs e)
        {
            AppsToDeploy test = new AppsToDeploy();
            test.Show();
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