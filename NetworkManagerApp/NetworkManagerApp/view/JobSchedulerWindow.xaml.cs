
using NetworkManager.Domain;
using System.Windows;
using System.Collections.Generic;

namespace NetworkManager.View {
    /// <summary>
    /// Logique d'interaction pour Planning.xaml
    /// </summary>
    public partial class JobSchedulerWindow : Window {

        private List<Computer> preSelectedComputers { get; set; }

        public JobSchedulerWindow() {
            InitializeComponent();

            // Fill the hours and minutes
            taskHours.Items.Clear();
            for (int i = 0; i <= 24; i++)
                taskHours.Items.Add($"{i}h");

            taskMinutes.Items.Clear();
            for (int i = 0; i <= 59; i++)
                taskMinutes.Items.Add($"{i}mn");
        }

        private void buttonAddTask_Click(object sender, RoutedEventArgs e) {
            TaskSelectionWindow selectTask = new TaskSelectionWindow();
            selectTask.mainWindow = this;
            selectTask.Left = this.Left + 50;
            selectTask.Top = this.Top + 50;
            selectTask.Show();
        }

        private void taskNow_Click(object sender, RoutedEventArgs e) {
            taskDate.IsEnabled = taskNow.IsChecked == false;
            taskHours.IsEnabled = taskNow.IsChecked == false;
            taskMinutes.IsEnabled = taskNow.IsChecked == false;

        }

        private void fillComputers(Domain.Domain d) {
            foreach(Computer c in d.computers) {
                selectedComputers.Items.Add(c);

                if(preSelectedComputers.Find(cSelected => cSelected.nameLong == c.nameLong) != null) {
                    selectedComputers.SelectedItems.Add(c);
                }
            }

            foreach (Domain.Domain subDomain in d.domains) {
                fillComputers(subDomain);
            }
        }

        private async void selectedComputers_Loaded(object sender, RoutedEventArgs e) {
            // Fill the computers
            var domain = new Domain.Domain();
            await domain.fill(false);

            selectedComputers.Items.Clear();
            fillComputers(domain);
        }
        
        private void selectedComputers_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e) {
            selectedComputersLabel.Content = $"{selectedComputers.SelectedItems.Count} computer{(selectedComputers.SelectedItems.Count > 1 ? "s" : "")} selected";
        }

        private void buttonSelectAll_Click(object sender, RoutedEventArgs e) {
            selectedComputers.SelectedItems.Clear();
            foreach (Computer c in selectedComputers.Items)
                selectedComputers.SelectedItems.Add(c);
        }

        private void buttonDeselectAll_Click(object sender, RoutedEventArgs e) {
            selectedComputers.SelectedItems.Clear();
        }

        internal void downTask(UIElement element) {
            int i = tasksPanel.Children.IndexOf(element);

            if (i + 1 < tasksPanel.Children.Count) {
                tasksPanel.Children.RemoveAt(i);
                tasksPanel.Children.Insert(i + 1, element);
            }
        }

        internal void upTask(UIElement element) {
            int i = tasksPanel.Children.IndexOf(element);

            if(i > 0) {
                tasksPanel.Children.RemoveAt(i);
                tasksPanel.Children.Insert(i - 1, element);
            }
        }

        internal void deleteTask(UIElement element) {
            int i = tasksPanel.Children.IndexOf(element);

            if (i >= 0) {
                tasksPanel.Children.RemoveAt(i);
            }
        }

        internal void selectComputer(Computer computer) {
            List<Computer> computers = new List<Computer>();
            computers.Add(computer);
            this.preSelectedComputers = computers;
        }

        internal void selectComputers(List<Computer> computers) {
            this.preSelectedComputers = computers;
        }
    }
}
