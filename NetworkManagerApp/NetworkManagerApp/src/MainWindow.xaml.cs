using NetworkManager.DomainContent;
using NetworkManager.View;

using SQLite;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using NetworkManager.Job;
using NetworkManager.View.Model;

namespace NetworkManager {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        /// <summary>
        /// Error handler
        /// </summary>
        public ErrorHandlerWindow errorHandler { get; private set; }
        
        /// <summary>
        /// Configuration
        /// </summary>
        public static Properties config { get; private set; } = new Properties(@"config\parameters.properties");

        public static ComputerInfoStore computerInfoStore { get; private set; }
        public static JobStore jobStore { get; private set; }

        public ConfigurationWindow configurationHandler { get; private set; }

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
            configurationHandler = new ConfigurationWindow();

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
            timer.Interval = new TimeSpan(0, 0, config.getInt("scantimeout", 30));
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
        
        /// <summary>
        /// Update the domain computer list
        /// </summary>
        public async Task updateListComputers() {
            showLoading();
            
            try {
                // Only one domain for now
                Domain domain = new Domain();
                await domain.fill();

                DomainModel domainModel = await DomainModel.createDomainModel(domain, errorHandler);

                if (!List_Computer.Items.IsEmpty) {
                    // Update the existing model with the new generated one
                    var needRefresh = (List_Computer.Items[0] as DomainModel).updateDomainModel(domainModel, List_Computer.SelectedItems);

                    if(needRefresh) // If an update is needed
                        updateSelectedComputers();

                    countMachine();

                } else {
                    // Set the new domain model
                    List_Computer.Items.Add(domainModel);

                    // Expand the root node
                    TreeViewExItem item = (TreeViewExItem)List_Computer.ItemContainerGenerator.ContainerFromItem(domainModel);
                    item.IsExpanded = true;
                    countMachine();
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
            // TODO change
            var timer = new DispatcherTimer();
            timer.Tick += async (source, e) => {
                timer.Stop();
                await updateListComputers();
            };
            timer.Interval = new TimeSpan(0, 0, inSeconds);
            timer.Start();
        }
        public void countMachine()
        {
            int cpt = 0;
            foreach (ComputerModel item in (List_Computer.Items[0] as DomainModel).Items)
            {
                if (item.isHide != true)
                    cpt++;
            }
            label_NumberOfMachines.Content = cpt.ToString();
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

        private void button_Configuration_Click(object sender, RoutedEventArgs e) {
            configurationHandler.Left = this.Left + 50;
            configurationHandler.Top = this.Top + 50;
            configurationHandler.Show();
        }

        public async void requireReload(Computer computer, int v) {
            await computer.updateAliveStatus();

            DomainModel domainModel = List_Computer.Items.First() as DomainModel;

            if (domainModel != null) {
                domainModel.updateComputerModel(computer);
                updateSelectedComputers();
            }
        }

        private void checkBox_FilterOn_Checked(object sender, RoutedEventArgs e)
        {
            foreach(var item in List_Computer.Items){
                if(item.GetType() == typeof(DomainModel))
                {
                    DomainModel dm = (DomainModel)item;
                    IEnumerable<ComputerModel> listcomputer = dm.getComputers();
                    foreach(var c in listcomputer)
                    {
                        if (c.computer.isAlive == true)
                            c.isHide = false;                        
                    }
                }
            }
        }

        private void checkBox_FilterOff_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in List_Computer.Items)
            {
                if (item.GetType() == typeof(DomainModel))
                {
                    DomainModel dm = (DomainModel)item;
                    IEnumerable<ComputerModel> listcomputer = dm.getComputers();
                    foreach (var c in listcomputer)
                    {
                        if (c.computer.isAlive == false)
                            c.isHide = false;
                    }
                }
            }
        }

        private void checkBox_FilterOff_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in List_Computer.Items)
            {
                if (item.GetType() == typeof(DomainModel))
                {
                    DomainModel dm = (DomainModel)item;
                    IEnumerable<ComputerModel> listcomputer = dm.getComputers();
                    foreach (var c in listcomputer)
                    {
                        if (c.computer.isAlive == false)
                            c.isHide = true;
                    }
                }
            }
        }

        private void checkBox_FilterOn_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in List_Computer.Items)
            {
                if (item.GetType() == typeof(DomainModel))
                {
                    DomainModel dm = (DomainModel)item;
                    IEnumerable<ComputerModel> listcomputer = dm.getComputers();
                    foreach (var c in listcomputer)
                    {
                        if (c.computer.isAlive == true)
                            c.isHide = true;
                        
                    }
                }
            }
        }
    }



}