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
        /// Async task cancellation token
        /// </summary>
        CancellationTokenSource ts;

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

        private void List_Machine_Loaded(object sender, RoutedEventArgs e) {
            var tree = sender as TreeView;

            // Only one domain for now
            Domain.Domain domain = new Domain.Domain();
            DomainModel domainModel = new DomainModel() {
                name = domain.name,
                computers = new ObservableCollection<ComputerModel>(
                    domain.computers.Select(c => new ComputerModel() { computer = c }))
            };

            tree.Items.Add(domainModel);
        }
        
        CancellationTokenSource loggedUserToken;

        private async void UpdateLoggedUsers() {
            if (loggedUserToken != null)
                loggedUserToken.Cancel();
            
            loggedUserToken = new CancellationTokenSource();
            var localTs = loggedUserToken;

            listBox_ConnectedUsers.Items.Clear();

            IEnumerable<User> loggedUsers;
            if (checkBox_ShowAllUsers.IsChecked.Value)
                loggedUsers = await computer.getAllLoggedUsers();
            else
                loggedUsers = await computer.getLoggedUsers();

            if (localTs != null && localTs.IsCancellationRequested)
                return;

            foreach (var user in loggedUsers) {
                listBox_ConnectedUsers.Items.Add(user);
            }
        }

        CancellationTokenSource installedSofwaresToken;

        private async void UpdateInstalledSoftwares() {
            if (installedSofwaresToken != null)
                installedSofwaresToken.Cancel();

            installedSofwaresToken = new CancellationTokenSource();
            var localTs = installedSofwaresToken;

            listBox_ShowAllInstalledSoftware.ItemsSource = new List<Software>();

            IEnumerable<Software> installedSoftwares = await computer.getInstalledSofwares();

            if (localTs != null && localTs.IsCancellationRequested)
                return;

            listBox_ShowAllInstalledSoftware.ItemsSource = installedSoftwares.ToList();
        }

        private void List_Computer_Selected(object sender, RoutedEventArgs e) {
            TreeViewItem tvi = e.OriginalSource as TreeViewItem;
            if (tvi.DataContext is ComputerModel) {
                this.computer = (tvi.DataContext as ComputerModel).computer;

                label_ClientName.Content = computer.name;
                textBox_OperatingSystem.Text = computer.os;
                textBox_OperatingSystemVersion.Text = computer.version;
                
                //showLoading();

                UpdateLoggedUsers();
                UpdateInstalledSoftwares();

                //hideLoading();
            }

        }

        private void checkBox_ShowAllUsers_Click(object sender, RoutedEventArgs e) {
            UpdateLoggedUsers();
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