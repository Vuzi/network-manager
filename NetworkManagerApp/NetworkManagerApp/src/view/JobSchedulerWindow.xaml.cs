
using NetworkManager.DomainContent;
using System.Windows;
using System.Collections.Generic;
using NetworkManager.Model;
using System;

namespace NetworkManager.View {

    public partial class JobSchedulerWindow : Window {

        private List<Computer> preSelectedComputers { get; set; }

        public JobSchedulerWindow() {
            InitializeComponent();

            jobDetails.parent = this;

            var element = new ScheduledJobModelGroup() { name = "Scheduled tasks" };
            element.Items.Add(new ScheduledJobModel() { job = new Scheduling.Job() { name = "Task #1" } });
            element.Items.Add(new ScheduledJobModel() { job = new Scheduling.Job() { name = "Task #2" } });
            element.Items.Add(new ScheduledJobModel() { job = new Scheduling.Job() { name = "Task #3" } });
            scheduledJobs.Items.Add(element);

            element = new ScheduledJobModelGroup() { name = "In progress tasks" };
            element.Items.Add(new ScheduledJobModel() { job = new Scheduling.Job() { name = "Task #4" } });
            element.Items.Add(new ScheduledJobModel() { job = new Scheduling.Job() { name = "Task #5" } });
            scheduledJobs.Items.Add(element);

            element = new ScheduledJobModelGroup() { name = "Terminated tasks" };
            element.Items.Add(new ScheduledJobModel() { job = new Scheduling.Job() { name = "Task #6" } });
            element.Items.Add(new ScheduledJobModel() { job = new Scheduling.Job() { name = "Task #7" } });
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
