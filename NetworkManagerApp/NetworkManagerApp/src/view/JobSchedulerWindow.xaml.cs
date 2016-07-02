
using NetworkManager.DomainContent;
using System.Windows;
using System.Collections.Generic;
using NetworkManager.Model;
using NetworkManager.Scheduling;

namespace NetworkManager.View {

    public partial class JobSchedulerWindow : Window {

        private List<Computer> preSelectedComputers { get; set; } = new List<Computer>();

        public JobSchedulerWindow() {
            InitializeComponent();

            jobDetails.parent = this;
            jobReportDetails.parent = this;

            updateScheduledJobs();
        }

        Dictionary<JobStatus, string> groups = new Dictionary<JobStatus, string>() {
            //{ JobStatus.CREATED, "Created tasks" },
            { JobStatus.SCHEDULED, "Scheduled tasks" },
            { JobStatus.IN_PROGRESS, "In progress tasks" },
            { JobStatus.TERMINATED, "Terminated tasks" },
            { JobStatus.CANCELLED, "Cancelled tasks" }
        };

        public void updateScheduledJobs() {

            scheduledJobs.Items.Clear();

            foreach(var group in groups) {
                var element = new ScheduledJobModelGroup() { name = group.Value };
                foreach (Job j in App.jobStore.getJobByStatus(group.Key))
                    element.Items.Add(new ScheduledJobModel() { job = j });
                scheduledJobs.Items.Add(element);
            }
        }

        public void selectComputer(Computer computer) {
            jobDetails.selectComputer(computer);
        }

        public void selectComputers(List<Computer> computers) {
            jobDetails.selectComputers(computers);
        }

        private void scheduledJobs_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            var selectedElement = scheduledJobs.SelectedItem;

            if(selectedElement is ScheduledJobModel) {
                var job = (selectedElement as ScheduledJobModel).job;

                jobDetails.setJob(job);
            }
        }

        private void newJobButton_Click(object sender, RoutedEventArgs e) {
            jobDetails.Visibility = Visibility.Visible;
            jobReportDetails.Visibility = Visibility.Collapsed;

            jobDetails.setJob(null);
        }

        private void button_JobsReload_Click(object sender, RoutedEventArgs e) {
            updateScheduledJobs();
        }
    }
}
