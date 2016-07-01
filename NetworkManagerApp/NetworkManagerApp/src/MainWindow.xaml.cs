using NetworkManager.DomainContent;
using NetworkManager.View;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using NetworkManager.Scheduling;
using NetworkManager.View.Model;
using System.Reflection;

namespace NetworkManager {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        /// <summary>
        /// Error handler
        /// </summary>
        public ErrorHandlerWindow errorHandler { get; private set; }

        public ConfigurationWindow configurationHandler { get; private set; }

        public MainWindow() {
            App.logger.Info($"{Assembly.GetExecutingAssembly().GetName().Name} - v{Assembly.GetExecutingAssembly().GetName().Version}");

            InitializeComponent();

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
            timer.Interval = new TimeSpan(0, 0, App.config.getInt("scantimeout", 30));
            timer.Start();
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
            // TODO change
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
    }



}