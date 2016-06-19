
using SQLite;
using System;
using System.Collections.Generic;

namespace NetworkManager.Job {

    public enum JobStatus {
        CREATED, SCHEDULED, IN_PROGRESS, TERMINATED
    }

    /// <summary>
    /// Job scheduled. A job is composed of a list of target computers and
    /// of a list of tasks to performs on each computer
    /// </summary>
    public class Job {
        [PrimaryKey]
        public string id { get; set; }
        public string name { get; set; }
        public DateTime scheduledDateTime { get; set; }
        public JobStatus status { get; set; }

        [Ignore]
        public List<string> computersNames { get; set; }
        [Ignore]
        public List<JobTask> tasks { get; set; }
        [Ignore]
        public JobReport report { get; set; }
    }
}
