
using NetworkManager.DomainContent;
using SQLite;

namespace NetworkManager.Scheduling {
    
    public class JobTaskReport {
        [PrimaryKey, NotNull]
        public string id { get; set; }
        public bool error { get; set; }
        public string extra { get; set; }

        [NotNull]
        public int order { get; set; }
        [NotNull]
        public string taskId { get; set; }
        [NotNull]
        public string jobReportId { get; set; }
        
        [Ignore]
        public JobTask task { get; set; }
    }

}
