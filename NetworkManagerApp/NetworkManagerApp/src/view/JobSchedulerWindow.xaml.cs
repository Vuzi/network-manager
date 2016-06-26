
using NetworkManager.DomainContent;
using System.Windows;
using System.Collections.Generic;
using NetworkManager.Model;
using System;
using NetworkManager.Scheduling;

namespace NetworkManager.View {

    public partial class JobSchedulerWindow : Window {

        private List<Computer> preSelectedComputers { get; set; }

        public JobSchedulerWindow() {
            InitializeComponent();

            jobDetails.parent = this;
            updateScheduledJobs();
        }

        public void updateScheduledJobs() {

            scheduledJobs.Items.Clear();

            var element = new ScheduledJobModelGroup() { name = "Created tasks" };
            foreach (Job j in MainWindow.jobStore.getJobByStatus(JobStatus.CREATED))
                element.Items.Add(new ScheduledJobModel() { job = j });
            scheduledJobs.Items.Add(element);

            element = new ScheduledJobModelGroup() { name = "Scheduled tasks" };
            foreach (Job j in MainWindow.jobStore.getJobByStatus(JobStatus.SCHEDULED))
                element.Items.Add(new ScheduledJobModel() { job = j });
            scheduledJobs.Items.Add(element);

            element = new ScheduledJobModelGroup() { name = "In progress tasks" };
            foreach (Job j in MainWindow.jobStore.getJobByStatus(JobStatus.IN_PROGRESS))
                element.Items.Add(new ScheduledJobModel() { job = j });
            scheduledJobs.Items.Add(element);

            element = new ScheduledJobModelGroup() { name = "Terminated tasks" };
            foreach (Job j in MainWindow.jobStore.getJobByStatus(JobStatus.TERMINATED))
                element.Items.Add(new ScheduledJobModel() { job = j });
            scheduledJobs.Items.Add(element);
        }

        public void selectComputer(Computer computer) {
            jobDetails.selectComputer(computer);
        }

        public void selectComputers(List<Computer> computers) {
            jobDetails.selectComputers(computers);
        }
    }
}
