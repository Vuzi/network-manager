
using SQLite;

namespace NetworkManager.Job {
    
    public class JobTaskReport {
        [PrimaryKey, NotNull]
        public string id { get; set; }
        public bool error { get; set; }
        public string extra { get; set; }

        [NotNull]
        public string taskId { get; set; }

        [NotNull]
        public string computerName { get; set; }

        [Ignore]
        public JobTask task { get; set; }
    }

}
