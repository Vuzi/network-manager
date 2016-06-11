using NetworkManager.Domain;
using NetworkManager.View;

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NetworkManager {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        /// <summary>
        /// Error handler
        /// </summary>
        public ErrorHandler errorHandler { get; private set; }

        public ComputerInfoStore computerInfoStore { get; private set; }

        public MainWindow() {
            InitializeComponent();

            // Preapre the database
            prepareDatabaseConnection();

            // Detail panel
            computerDetails.mainWindow = this;

            // Error panel
            errorHandler = new ErrorHandler();
            errorHandler.warningIndicator = WarningImage;

            // App level exception handler
            Application.Current.DispatcherUnhandledException += (sender, e) => {
                errorHandler.addError(e.Exception);
                e.Handled = true;
            };
        }

        /// <summary>
        /// Prepare the SQLite connection
        /// </summary>
        private void prepareDatabaseConnection() {
            if(!File.Exists("NetworkManager.sqlite"))
                SQLiteConnection.CreateFile("NetworkManager.sqlite");
            var conn = new SQLiteConnection("Data Source=NetworkManager.sqlite;Version=3;");
            conn.Open();
            computerInfoStore = new ComputerInfoStore(conn);
        }

        private async Task<DomainModel> createDomainModel(Domain.Domain domain) {

            DomainModel domainModel = new DomainModel() {
                name = domain.name
            };

            foreach (Computer c in domain.computers) {
                domainModel.computers.Add(new ComputerModel() { computer = c });

                // If the computer is alive, save its values in the database
                if (c.isAlive) {
                    try {
                        computerInfoStore.updateOrInsertComputerInfo(new ComputerInfo() {
                            name = c.nameLong,
                            ipAddress = c.getIpAddress().ToString(),
                            macAddress = await c.getMacAddress()
                        });
                    } catch (Exception e) {
                        errorHandler.addError(e);
                    }
                }
            }

            foreach (Domain.Domain d in domain.domains) {
                domainModel.domains.Add(await createDomainModel(d));
            }

            return domainModel;
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

                DomainModel domainModel = await createDomainModel(domain);

                List_Computer.Items.Add(domainModel);

                // Expand
                TreeViewItem item = (TreeViewItem)List_Computer.ItemContainerGenerator.ContainerFromItem(domainModel);
                item.IsExpanded = true;

            } catch(Exception e) {
                errorHandler.addError(e);
            }

            hideLoading();
        }

        int loadingWaiting = 0;

        /// <summary>
        /// Show the loading icon
        /// </summary>
        public void showLoading() {
            loadingWaiting++;
            LoadingSpinner.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hide the loading icon
        /// </summary>
        public void hideLoading() {
            if(--loadingWaiting == 0)
                LoadingSpinner.Visibility = Visibility.Collapsed;
        }
        
        private void List_Computer_Selected(object sender, RoutedEventArgs eventArgs) {
            TreeViewItem tvi = eventArgs.OriginalSource as TreeViewItem;

            if (tvi.DataContext is ComputerModel) {
                computerDetails.showDetails((tvi.DataContext as ComputerModel).computer);
            }
        }
        
        private async void List_Machine_Loaded(object sender, RoutedEventArgs e) {
            await updateListComputers();
        }

        private async void button_ComputersReload_Click(object sender, RoutedEventArgs e) {
            await updateListComputers();
        }

        private void WarningImage_Click(object sender, RoutedEventArgs e) {
            errorHandler.Left = this.Left + 50;
            errorHandler.Top = this.Top + 50;
            errorHandler.Show();
        }
        
    }

    public class DomainModel {
        public string name { get; set; }
        public ObservableCollection<ComputerModel> computers { get; set; }
        public ObservableCollection<DomainModel> domains { get; set; }

        public DomainModel() {
            this.computers = new ObservableCollection<ComputerModel>();
            this.domains = new ObservableCollection<DomainModel>();
        }

        public IList<object> Items {
            get {
                IList<object> childNodes = new List<object>();
                foreach (var group in this.domains)
                    childNodes.Add(group);
                foreach (var entry in this.computers)
                    childNodes.Add(entry);

                return childNodes;
            }
        }
    }

    public class ComputerModel {
        public Computer computer { get; set; }
    }
}