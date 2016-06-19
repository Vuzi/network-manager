
using NetworkManager.DomainContent;
using System.Windows;
using System.Collections.Generic;
using System;
using System.Linq;
using NetworkManager.Job;

namespace NetworkManager.View {
    /// <summary>
    /// Logique d'interaction pour Planning.xaml
    /// </summary>
    public partial class JobSchedulerWindow : Window {

        private List<Computer> preSelectedComputers { get; set; }

        public JobSchedulerWindow() {
            InitializeComponent();

            // Fill the hours and minutes
            jobHoursPicker.Items.Clear();
            for (int i = 0; i <= 24; i++)
                jobHoursPicker.Items.Add($"{i}h");

            jobMinutesPicker.Items.Clear();
            for (int i = 0; i <= 59; i++)
                jobMinutesPicker.Items.Add($"{i}mn");
        }

        private void buttonAddTask_Click(object sender, RoutedEventArgs e) {
            TaskSelectionWindow selectTask = new TaskSelectionWindow();
            selectTask.mainWindow = this;
            selectTask.Left = this.Left + 50;
            selectTask.Top = this.Top + 50;
            selectTask.Show();
        }

        private void jobNow_Click(object sender, RoutedEventArgs e) {
            jobDatePicker.IsEnabled = jobNowCheckbox.IsChecked == false;
            jobHoursPicker.IsEnabled = jobNowCheckbox.IsChecked == false;
            jobMinutesPicker.IsEnabled = jobNowCheckbox.IsChecked == false;

        }

        private void fillComputers(Domain d) {
            foreach(Computer c in d.computers) {
                selectedComputersGrid.Items.Add(c);

                if(preSelectedComputers.Find(cSelected => cSelected.nameLong == c.nameLong) != null) {
                    selectedComputersGrid.SelectedItems.Add(c);
                }
            }

            foreach (Domain subDomain in d.domains) {
                fillComputers(subDomain);
            }
        }

        private async void selectedComputersGrid_Loaded(object sender, RoutedEventArgs e) {
            // Fill the computers
            var domain = new Domain();
            await domain.fill(false);

            selectedComputersGrid.Items.Clear();
            fillComputers(domain);
        }
        
        private void selectedComputersGrid_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e) {
            selectedComputersLabel.Content = $"{selectedComputersGrid.SelectedItems.Count} computer{(selectedComputersGrid.SelectedItems.Count > 1 ? "s" : "")} selected";
        }

        private void buttonSelectAll_Click(object sender, RoutedEventArgs e) {
            selectedComputersGrid.SelectedItems.Clear();
            foreach (Computer c in selectedComputersGrid.Items)
                selectedComputersGrid.SelectedItems.Add(c);
        }

        private void buttonDeselectAll_Click(object sender, RoutedEventArgs e) {
            selectedComputersGrid.SelectedItems.Clear();
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

        private void buttonCreateJob_Click(object sender, RoutedEventArgs e) {

            // Get the picked date
            DateTime? jobDateTime = null;
            if (jobNowCheckbox.IsChecked == true) {
                jobDateTime = DateTime.Now;
            } else {
                jobDateTime = jobDatePicker.SelectedDate;

                if (jobDateTime == null) {
                    MessageBox.Show("Error : An execution date must be provided to the job", "Job creation error");
                    return;
                } else if (jobDateTime < DateTime.Now) {
                    MessageBox.Show("Error : The specified date can't be in the past", "Job creation error");
                    return;
                }
            }

            // Get the selected computers
            if(selectedComputersGrid.SelectedItems.Count <= 0) {
                MessageBox.Show("Error : No computer selected. At least one computer should be selected", "Job creation error");
                return;
            }

            var computersNames = new List<string>();
            foreach(Computer c in selectedComputersGrid.SelectedItems) {
                computersNames.Add(c.nameLong);
            }

            // Get the tasks
            var tasks = new List<JobTask>();

            foreach(dynamic element in tasksPanel.Children) {
                JobTask jobTask = element.createTask();

                if (jobTask == null)
                    return; // Error during jobtask creation

                Console.WriteLine(element.GetType());

                tasks.Add(jobTask);
            }

            if(tasks.Count <= 0) {
                MessageBox.Show("Error : No task defined. At least one taks should be defined", "Job creation error");
                return;
            }

            // OK, create the job
            var job = new Job.Job() {
                scheduledDateTime = jobDateTime.Value,
                computersNames = computersNames,
                status = JobStatus.CREATED,
                tasks = tasks,
                report = null
            };

            // Insert into the job store
            MainWindow.jobStore.insertJob(job);

            // Create a windows task
            // TODO
        }

    }
}
