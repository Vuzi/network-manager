using NetworkManager.DomainContent;
using NetworkManager.View;

using SQLite;

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using NetworkManager.Job;

namespace NetworkManager {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        /// <summary>
        /// Error handler
        /// </summary>
        public ErrorHandlerWindow errorHandler { get; private set; }

        public static ComputerInfoStore computerInfoStore { get; private set; }
        public static JobStore jobStore { get; private set; }

        public Configuration configurationHandler { get; private set; }

        public MainWindow() {
            try {
                InitializeComponent();
            } catch(Exception e) {
                Console.WriteLine(e);
            }

            // Preapre the database
            prepareDatabaseConnection();

            // Detail panels
            computerDetails.mainWindow = this;
            computersDetails.mainWindow = this;

            // Error panel
            errorHandler = new ErrorHandlerWindow();
            errorHandler.warningIndicator = WarningImage;

            // Error panel
            configurationHandler = new Configuration();

            // App level exception handler
            Application.Current.DispatcherUnhandledException += (sender, e) => {
                errorHandler.addError(e.Exception);
                e.Handled = true;
            };

            // Auto updater
            var timer = new DispatcherTimer();
            timer.Tick += async (source, e) => {
                timer.Stop();

                try {
                    await updateListComputers();
                } catch (Exception ex) {
                    errorHandler.addError(ex);
                } finally {
                    timer.Start();
                }
            };
            timer.Interval = new TimeSpan(0, 0, 30);
            timer.Start();
        }

        /// <summary>
        /// Prepare the SQLite connection
        /// </summary>
        private void prepareDatabaseConnection() {
            var conn = new SQLiteConnection("NetworkManager.sqlite");

            computerInfoStore = new ComputerInfoStore(conn);
            jobStore = new JobStore(conn);
        }

        private async Task<DomainModel> createDomainModel(DomainContent.Domain domain) {

            DomainModel domainModel = new DomainModel() {
                name = domain.name
            };

            foreach (Computer c in domain.computers) {
                domainModel.addComputer(new ComputerModel() {
                    computer = c
                });

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

            foreach (DomainContent.Domain d in domain.domains) {
                domainModel.addDomain( await createDomainModel(d));
            }

            return domainModel;
        }

        private bool updateDomainModel(DomainModel oldDomainModel, DomainModel domainModel) {
            bool needRefresh = false;

            // Update & adding of new computers
            foreach (var computerModel in domainModel.getComputers()) {
                var oldComputerModel = oldDomainModel.getComputer(computerModel.computer.name);

                if (oldComputerModel != null) {
                    // If an update is needed
                    if (oldComputerModel.computer.isAlive != computerModel.computer.isAlive) {
                        foreach (var item in List_Computer.SelectedItems) {
                            if (item is ComputerModel && (item as ComputerModel).computer.nameLong == computerModel.computer.nameLong) {
                                needRefresh = true;
                            }
                        }
                    }

                    oldComputerModel.computer = computerModel.computer;
                } else
                    oldDomainModel.addComputer(computerModel);
            }

            // Update & adding of new models
            foreach (var subdomainModel in domainModel.getDomains()) {
                var oldSubdomainModel = oldDomainModel.getDomain(subdomainModel.name);

                if (oldSubdomainModel != null)
                    needRefresh = needRefresh || updateDomainModel(oldSubdomainModel, subdomainModel);
                else
                    oldDomainModel.addDomain(subdomainModel);
            }

            // Removing of old computers
            foreach (string computerToRemove in oldDomainModel.getComputers().Select(c => c.computer.name).
                                                Except(domainModel.getComputers().Select(c => c.computer.name)).
                                                ToList()) {
                oldDomainModel.removeComputer(computerToRemove);
            }

            // Removing of old domains
            foreach (string domainToRemove in oldDomainModel.getDomains().Select(d => d.name).
                                                Except(domainModel.getDomains().Select(d => d.name)).
                                                ToList()) {
                oldDomainModel.removeDomain(domainToRemove);
            }

            return needRefresh;
        }

        /// <summary>
        /// Update the domain computer list
        /// </summary>
        public async Task updateListComputers() {
            showLoading();

            try {
                // Only one domain for now
                DomainContent.Domain domain = new DomainContent.Domain();
                await domain.fill();

                DomainModel domainModel = await createDomainModel(domain);

                if (!List_Computer.Items.IsEmpty) {
                    // Update the existing model with the new generated one
                    var needRefresh = updateDomainModel(List_Computer.Items[0] as DomainModel, domainModel);

                    if(needRefresh) // If an update is needed
                        updateSelectedComputers();

                } else {
                    // Set the new domain model
                    List_Computer.Items.Add(domainModel);

                    // Expand the root node
                    TreeViewExItem item = (TreeViewExItem)List_Computer.ItemContainerGenerator.ContainerFromItem(domainModel);
                    item.IsExpanded = true;
                }

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

        /// <summary>
        /// Update the right panel according to the selected computers
        /// </summary>
        private void updateSelectedComputers() {
            List<Computer> selectedComputers = new List<Computer>();
            
            foreach (var selectedItem in List_Computer.SelectedItems) {
                if (selectedItem is ComputerModel) {
                    selectedComputers.Add((selectedItem as ComputerModel).computer);
                }
            }

            if (selectedComputers.Count == 1) {
                computersDetails.Visibility = Visibility.Collapsed;
                computerDetails.Visibility = Visibility.Visible;
                computerDetails.showDetails(selectedComputers[0]);
            } else if (selectedComputers.Count > 1) {
                computerDetails.Visibility = Visibility.Collapsed;
                computersDetails.Visibility = Visibility.Visible;
                computersDetails.showDetails(selectedComputers);
            }
        }

        public void requireReload(int inSeconds) {
            var timer = new DispatcherTimer();
            timer.Tick += async (source, e) => {
                timer.Stop();
                await updateListComputers();
            };
            timer.Interval = new TimeSpan(0, 0, inSeconds);
            timer.Start();
        }

        private void List_Computer_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            switch(e.Key) {
                case System.Windows.Input.Key.Up:
                case System.Windows.Input.Key.Down:
                    updateSelectedComputers();
                    break;
            }
        }

        private void List_Computer_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            updateSelectedComputers();
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
        private void button_Configuration_Click(object sender, RoutedEventArgs e)
        {
            configurationHandler.Left = this.Left + 50;
            configurationHandler.Top = this.Top + 50;
            configurationHandler.Show();
        }
    }

    public class DomainModel : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        public string name { get; set; }
        private Dictionary<string, ComputerModel> computers { get; set; }
        private Dictionary<string, DomainModel> domains { get; set; }

        internal void notifyPropertyChanged(String info) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public DomainModel() {
            computers = new Dictionary<string, ComputerModel>();
            domains = new Dictionary<string, DomainModel>();
        }

        public DomainModel getDomain(string name) {
            return domains.GetValueOrDefault(name);
        }

        public ComputerModel getComputer(string name) {
            return computers.GetValueOrDefault(name);
        }

        public void addDomain(DomainModel domain) {
            domains[domain.name] = domain;
            Items.Insert(computerIndex, domain);
            computerIndex++;

            notifyPropertyChanged("Domains");
        }

        public void addComputer(ComputerModel computer) {
            computers[computer.computer.name] = computer;
            Items.Add(computer);

            notifyPropertyChanged("Computers");
        }

        public void removeComputer(ComputerModel computer) {
            removeComputer(computer.computer.name);
        }

        public void removeComputer(string computer) {
            computers.Remove(computer);
            for (int i = computerIndex; i < Items.Count; i++) {
                var item = Items[i] as ComputerModel;

                if (item.computer.name == computer) {
                    Items.RemoveAt(i);
                    notifyPropertyChanged("Computers");
                    break;
                }
            }
        }

        public void removeDomain(DomainModel domain) {
            removeDomain(domain.name);
        }

        public void removeDomain(string domain) {
            domains.Remove(domain);
            for (int i = 0; i < computerIndex; i++) {
                var item = Items[i] as DomainModel;

                if (item.name == domain) {
                    Items.RemoveAt(i);
                    computerIndex--;
                    notifyPropertyChanged("Domains");
                    break;
                }
            }
        }

        public IEnumerable<ComputerModel> getComputers() {
            return computers.Values;
        }

        public IEnumerable<DomainModel> getDomains() {
            return domains.Values;
        }

        private int computerIndex;
        private ObservableCollection<object> items;
        public ObservableCollection<object> Items {
            get {
                if (items == null) {
                    items = new ObservableCollection<object>();
                    computerIndex = 0;
                }

                return items;
            }
        }
    }

    public class ComputerModel : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        private Computer _computer;
        public Computer computer {
            get {
                return _computer;
            }

            set {
                if(_computer != null)
                    value.copyCache(_computer); // Keep in memory the cached values of the computer
                _computer = value;
                notifyPropertyChanged("computer");
            }
        }

        internal void notifyPropertyChanged(String info) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

    }
}