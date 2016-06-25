
using SQLite;
using System.Collections.Generic;

namespace NetworkManager.Job {

    public class JobReport {
        [PrimaryKey, NotNull]
        public string id { get; set; }
        [NotNull]
        public string jobId { get; set; }
        [NotNull]
        public string computerName { get; set; }
        [NotNull]
        public bool error { get; set; }
        [Ignore]
        public List<JobTaskReport> tasksReports { get; set; }
    }

}
