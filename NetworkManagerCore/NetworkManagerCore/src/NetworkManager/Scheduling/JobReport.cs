
using SQLite;
using System;
using System.Collections.Generic;

namespace NetworkManager.Scheduling {

    public class JobReport {
        [PrimaryKey, NotNull]
        public string id { get; set; }
        [NotNull]
        public string jobId { get; set; }
        [NotNull]
        public string computerName { get; set; }
        [NotNull]
        public bool error { get; set; }
        public DateTime startDateTime { get; set; }
        public DateTime endDateTime { get; set; }
        [Ignore]
        public List<JobTaskReport> tasksReports { get; set; }
    }

}
