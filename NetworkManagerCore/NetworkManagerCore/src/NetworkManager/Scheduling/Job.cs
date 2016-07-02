
using Microsoft.Win32.TaskScheduler;
using NetworkManager.DomainContent;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NetworkManager.Scheduling {

    public enum JobStatus {
        CREATED, SCHEDULED, IN_PROGRESS, TERMINATED, CANCELLED
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
                ts.GetFolder("\\Network Manager")?.DeleteTask($"NetworkManager Task #{id}");
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
                td.RegistrationInfo.Author = $"Network Manager v{Assembly.GetExecutingAssembly().GetName().Version}";
                td.Settings.DisallowStartIfOnBatteries = false;
                td.Principal.UserId = "SYSTEM";
                td.Principal.RunLevel = TaskRunLevel.Highest;
                td.Principal.LogonType = TaskLogonType.ServiceAccount;

                // Trigger
                td.Triggers.Add(new TimeTrigger((scheduledDateTime != DateTime.MinValue ? scheduledDateTime : DateTime.Now).AddSeconds(3)));
                
                // Action
                td.Actions.Add(new ExecAction(exe, $"{id}", Directory.GetCurrentDirectory()));

                // Task registration
                var folder = ts.RootFolder.CreateFolder("Network Manager", null, false);
                folder.RegisterTaskDefinition($"NetworkManager Task #{id}", td);
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
