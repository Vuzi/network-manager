using NetworkManager.Domain;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NetworkManager {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
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
        
        private void List_Computer_Selected(object sender, RoutedEventArgs e) {
            TreeViewItem tvi = e.OriginalSource as TreeViewItem;
            if(tvi.DataContext is ComputerModel) {
                this.computer = (tvi.DataContext as ComputerModel).computer;

                label_ClientName.Content = computer.name;
                textBox_OperatingSystem.Text = computer.os;
                textBox_OperatingSystemVersion.Text = computer.version;

                listBox_ConnectedUsers.Items.Clear();
                foreach (var user in computer.getAllLoggedUsers()) {
                    listBox_ConnectedUsers.Items.Add(user.name);
                }

                listBox_ShowAllInstalledSoftware.Items.Clear();
                foreach (var software in computer.getInstalledSofwares()) {
                    listBox_ShowAllInstalledSoftware.Items.Add(software.displayName);
                }
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