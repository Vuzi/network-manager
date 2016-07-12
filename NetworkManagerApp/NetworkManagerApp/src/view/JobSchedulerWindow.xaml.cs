
using NetworkManager.DomainContent;
using System.Windows;
using System.Collections.Generic;
using NetworkManager.Model;
using NetworkManager.Scheduling;
using System;

namespace NetworkManager.View {

    public partial class JobSchedulerWindow : Window {

        private List<Computer> preSelectedComputers { get; set; } = new List<Computer>();
        private Job job;

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
            updateScheduledJobs(null);
        }

        public void updateScheduledJobs(Job job) {

            scheduledJobs.Items.Clear();

            foreach(var group in groups) {
                var element = new ScheduledJobModelGroup() { name = group.Value };
                foreach (Job j in App.jobStore.getJobByStatus(group.Key))
                    element.Items.Add(new ScheduledJobModel() { job = j, selected = (job != null ? (job.id == j.id) : false) });
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

            if(selectedElement != null && selectedElement is ScheduledJobModel) {
                job = (selectedElement as ScheduledJobModel).job;

                jobDetails.setJob(job);
            }
        }

        private void newJobButton_Click(object sender, RoutedEventArgs e) {
            jobDetails.Visibility = Visibility.Visible;
            jobReportDetails.Visibility = Visibility.Collapsed;

            jobDetails.setJob(null);
        }

        private void button_JobsReload_Click(object sender, RoutedEventArgs e) {
            if (job != null) {
                job = App.jobStore.getJobById(job.id);
                updateScheduledJobs(job);

                // Force update
                jobDetails.setJob(job);
                jobReportDetails.setJob(job);
            } else
                updateScheduledJobs();
        }
    }
}
