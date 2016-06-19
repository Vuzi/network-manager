
using System.Collections.Generic;

namespace NetworkManager.Job {

    public class JobReport {
        public string computerName { get; set; }
        public bool error { get; set; }
        public List<JobTaskReport> tasksReports { get; set; }
    }

}
