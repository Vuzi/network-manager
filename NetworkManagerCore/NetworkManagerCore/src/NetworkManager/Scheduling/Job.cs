
using Microsoft.Win32.TaskScheduler;
using NetworkManager.DomainContent;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NetworkManager.Scheduling {

    public enum JobStatus {
        CREATED, SCHEDULED, IN_PROGRESS, TERMINATED
    }

    /// <summary>
    /// Job scheduled. A job is composed of a list of target computers and
    /// of a list of tasks to performs on each computer
    /// </summary>
    public class Job {
        [PrimaryKey, NotNull]
        public string id { get; set; }
        public string name { get; set; }
        public DateTime scheduledDateTime { get; set; }
        public DateTime startDateTime { get; set; }
        public DateTime endDateTime { get; set; }
        [NotNull]
        public DateTime creationDate { get; set; }
        [NotNull]
        public JobStatus status { get; set; }

        [Ignore]
        public List<ComputerInfo> computers { get; set; }
        [Ignore]
        public List<JobTask> tasks { get; set; }
        [Ignore]
        public List<JobReport> reports { get; set; }

        /// <summary>
        /// Unschedule the job
        /// </summary>
        public void unSchedule() {
            using (TaskService ts = new TaskService()) {
                ts.RootFolder.DeleteTask($"NetworkManager_Task-{id}");
            }
        }

        /// <summary>
        /// Schedule the job
        /// </summary>
        public void schedule() {
            string exe = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\NetworkManagerExecutor.exe";
            
            using (TaskService ts = new TaskService()) {
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = $"Network Manager Task #{id}";
                
                td.Triggers.Add(new TimeTrigger { StartBoundary = scheduledDateTime });
                
                td.Actions.Add(new ExecAction(exe, $"{id}", null));

                ts.RootFolder.RegisterTaskDefinition($"NetworkManager_Task-{id}", td);
            }

        }
    }

    public class ComputerInJob {
        [NotNull]
        public string computerName { get; set; }

        [NotNull]
        public string jobId { get; set; }
    }
}
