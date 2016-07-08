using NetworkManager.DomainContent;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using NetworkManager.Scheduling;
using NetworkManager.View.Component.Job;

namespace NetworkManager.View.Component {

    /// <summary>
    /// Logique d'interaction pour JobDetails.xaml
    /// </summary>
    public partial class JobDetails : UserControl {

        public JobSchedulerWindow parent { get; set; }
        private List<Computer> preSelectedComputers { get; set; } = new List<Computer>();
        private Microsoft.Win32.TaskScheduler.Trigger trigger;
        private List<FrameworkElement> elementToDisable;
        private Scheduling.Job job;

        public JobDetails() {
            InitializeComponent();

            // Control elements to disable
            elementToDisable = new List<FrameworkElement> {
                textBox_TaskName,
                button_changeTrigger,
                textBox_plannedTrigger,
                jobCyclicCheckbox,
                jobNowCheckbox,
                selectedComputersGrid,
                buttonSelectAll,
                buttonDeselectAll,
                buttonAddTask,
                selectedComputersLabel,
                tasksPanel
            };
        }

        /// <summary>
        /// Fill recusively the computer panel with the provided domain
        /// </summary>
        /// <param name="d"></param>
        private void fillComputers(Domain d) {
            foreach (Computer c in d.computers) {
                selectedComputersGrid.Items.Add(c);

                if (preSelectedComputers.Find(cSelected => cSelected.nameLong == c.nameLong) != null) {
                    selectedComputersGrid.SelectedItems.Add(c);
                }
            }

            foreach (Domain subDomain in d.domains) {
                fillComputers(subDomain);
            }
        }

        /// <summary>
        /// Show a job in the panel. If the job is null, the panel will be reset
        /// </summary>
        /// <param name="job">The job to show</param>
        public void setJob(Scheduling.Job job) {
            this.job = job;

            if(job == null) {
                reset();
                return;
            }

            if(job.status == JobStatus.CREATED) {
                elementToDisable.ForEach(element => element.IsEnabled = true);
            } else {
                elementToDisable.ForEach(element => element.IsEnabled = false);
            }

            label_jobDetailsTitle.Content = "Selected Job";
            textBox_TaskName.Text = job.name;

            if(job.executeNow) {
                jobNowCheckbox.IsChecked = true;
                jobCyclicCheckbox.IsChecked = false;
                textBox_plannedTrigger.Text = string.Empty;
            } else {
                jobNowCheckbox.IsChecked = false;
                jobCyclicCheckbox.IsChecked = job.cyclic;
                textBox_plannedTrigger.Text = job.triggerDescription;
            }

            // Selected computers
            selectedComputersGrid.SelectedItems.Clear();
            foreach (Computer c in selectedComputersGrid.Items) {
                foreach(ComputerInfo ci in job.computers) {
                    if(ci.name == c.nameLong) {
                        selectedComputersGrid.SelectedItems.Add(c);
                        break;
                    }
                }
            }

            // Tasks
            tasksPanel.Children.Clear();
            foreach (JobTask task in job.tasks) {
                dynamic panel = null;

                switch(task.type) {
                    case JobTaskType.INSTALL_SOFTWARE:
                        panel = new TaskInstall();
                        break;
                    case JobTaskType.REBOOT:
                        panel = new TaskReboot();
                        break;
                    case JobTaskType.SHUTDOWN:
                        panel = new TaskShutdown();
                        break;
                    case JobTaskType.WAKE_ON_LAN:
                        panel = new TaskWakeOnLan();
                        break;
                }

                if(panel != null) {
                    panel.initFromTask(task);
                    tasksPanel.Children.Add(panel);
                }
            }

            // Actions accordingly to the status
            switch(job.status) {
                case JobStatus.CANCELLED:
                case JobStatus.TERMINATED:
                    buttonShowReport.Visibility = Visibility.Visible;
                    buttonCancel.Visibility = Visibility.Collapsed;
                    buttonDelete.Visibility = Visibility.Visible;
                    parent.jobReportDetails.setJob(job);
                    break;
                case JobStatus.SCHEDULED:
                    buttonShowReport.Visibility = Visibility.Visible;
                    buttonDelete.Visibility = Visibility.Collapsed;
                    buttonCancel.Visibility = Visibility.Visible;
                    buttonCancel.IsEnabled = true;
                    parent.jobReportDetails.setJob(job);
                    break;
                case JobStatus.IN_PROGRESS:
                    buttonShowReport.Visibility = Visibility.Visible;
                    buttonDelete.Visibility = Visibility.Collapsed;
                    buttonCancel.Visibility = Visibility.Visible;
                    buttonCancel.IsEnabled = false;
                    break;
                case JobStatus.CREATED:
                default:
                    buttonShowReport.Visibility = Visibility.Collapsed;
                    buttonDelete.Visibility = Visibility.Collapsed;
                    buttonCancel.Visibility = Visibility.Collapsed;
                    break;
            }
            buttonCreateJob.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Reset the panel
        /// </summary>
        public void reset() {
            label_jobDetailsTitle.Content = "New Job";
            elementToDisable.ForEach(element => element.IsEnabled = true);

            textBox_TaskName.Text = string.Empty;

            trigger = null;
            jobNowCheckbox.IsChecked = false;
            textBox_plannedTrigger.Text = string.Empty;
            jobCyclicCheckbox.IsChecked = false;

            selectedComputersGrid.SelectedItems.Clear();

            tasksPanel.Children.Clear();

            buttonCreateJob.Visibility = Visibility.Visible;
            buttonShowReport.Visibility = Visibility.Collapsed;
            buttonCancel.Visibility = Visibility.Collapsed;
            buttonDelete.Visibility = Visibility.Collapsed;
        }

        private void buttonAddTask_Click(object sender, RoutedEventArgs e) {
            TaskSelectionWindow selectTask = new TaskSelectionWindow();
            selectTask.mainWindow = this;
            selectTask.Left = parent.Left + 50;
            selectTask.Top = parent.Top + 50;
            selectTask.Show();
        }

        private void jobNow_Click(object sender, RoutedEventArgs e) {
            textBox_plannedTrigger.IsEnabled = jobNowCheckbox.IsChecked == false;
            button_changeTrigger.IsEnabled = jobNowCheckbox.IsChecked == false;
            jobCyclicCheckbox.IsEnabled = jobNowCheckbox.IsChecked == false;
        }

        private async void selectedComputersGrid_Loaded(object sender, RoutedEventArgs e) {
            // Fill the computers
            var domain = new Domain();
            await domain.fill(false);

            selectedComputersGrid.Items.Clear();
            fillComputers(domain);
        }

        private void selectedComputersGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e) {
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

            if (i > 0) {
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

            // If a trigger is defined
            if (trigger == null && !jobNowCheckbox.IsChecked.Value) {
                MessageBox.Show("Error : An execution trigger must be provided to the job", "Job creation error");
                return;
            }

            // Get the selected computers
            if (selectedComputersGrid.SelectedItems.Count <= 0) {
                MessageBox.Show("Error : No computer selected. At least one computer should be selected", "Job creation error");
                return;
            }

            var selectedComputers = new List<ComputerInfo>();
            foreach (Computer c in selectedComputersGrid.SelectedItems) {
                selectedComputers.Add(App.computerInfoStore.getComputerInfoByName(c.nameLong));
            }

            // Get the tasks
            var tasks = new List<JobTask>();

            foreach (dynamic element in tasksPanel.Children) {
                JobTask jobTask = element.createTask();

                if (jobTask == null)
                    return; // Error during jobtask creation

                tasks.Add(jobTask);
            }

            if (tasks.Count <= 0) {
                MessageBox.Show("Error : No task defined. At least one taks should be defined", "Job creation error");
                return;
            }

            // OK, create the job
            var job = new Scheduling.Job {
                creationDateTime = DateTime.Now,
                cyclic = jobCyclicCheckbox.IsChecked.Value,
                executeNow = trigger == null,
                triggerDescription = trigger != null ? trigger.ToString() : "As soon as possible",
                computers = selectedComputers,
                status = JobStatus.SCHEDULED,
                tasks = tasks,
                name = textBox_TaskName.Text
            };

            // Insert into the job store
            App.jobStore.insertJob(job);

            // Create a windows task
            if (jobNowCheckbox.IsChecked.Value)
                job.schedule();
            else
                job.schedule(trigger);

            parent.updateScheduledJobs();

            reset();
        }

        private void buttonShowReport_Click(object sender, RoutedEventArgs e) {
            parent.jobDetails.Visibility = Visibility.Collapsed;
            parent.jobReportDetails.Visibility = Visibility.Visible;
        }

        private void button_JobReload_Click(object sender, RoutedEventArgs e) {
            if(job != null)
                setJob(App.jobStore.getJobById(job.id));
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e) {
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to cancel this job ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;

            job = App.jobStore.getJobById(job.id);

            if(job.status == JobStatus.CREATED || job.status == JobStatus.SCHEDULED) {
                job.unSchedule();
                job.status = JobStatus.CANCELLED;
                App.jobStore.updateJob(job);
            }

            setJob(job);
            parent.updateScheduledJobs();
        }

        private void button_changeTrigger_Click(object sender, RoutedEventArgs e) {
            var triggerDialog = new Microsoft.Win32.TaskScheduler.TriggerEditDialog();

            triggerDialog.ShowDialog();
            trigger = triggerDialog.Trigger;

            textBox_plannedTrigger.Text = trigger.ToString();
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e) {
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete this job ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;

            job = App.jobStore.getJobById(job.id);

            if (job.status == JobStatus.TERMINATED || job.status == JobStatus.CANCELLED) {
                App.jobStore.deleteJob(job);
            }

            reset();
            parent.updateScheduledJobs();
        }
    }
}
