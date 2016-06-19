
using System;
using System.Collections.Generic;

namespace NetworkManager.Job {

    public enum JobStatus {
        SCHEDULED, IN_PROGRESS, TERMINATED
    }

    /// <summary>
    /// Job scheduled. A job is composed of a list of target computers and
    /// of a list of tasks to performs on each computer
    /// </summary>
    public class Job {
        public DateTime scheduledDateTime { get; set; }
        public List<string> computersNames { get; set; }
        public List<JobTask> tasks { get; set; }
        public JobReport report { get; set; }
        public JobStatus status { get; set; }
    }
}
