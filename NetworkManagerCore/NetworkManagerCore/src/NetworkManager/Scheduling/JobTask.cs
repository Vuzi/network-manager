
using SQLite;

namespace NetworkManager.Scheduling {
    
    public enum JobTaskType {
        INSTALL_SOFTWARE, WAKE_ON_LAN, REBOOT, SHUTDOWN
    }

    /// <summary>
    /// Job task of a scheduled job. A job task is an action to performs.
    /// </summary>
    public class JobTask {
        [PrimaryKey, NotNull]
        public string id { get; set; }
        [NotNull]
        public string jobId { get; set; }
        [NotNull]
        public JobTaskType type { get; set; }
        [NotNull]
        public int timeout { get; set; }
        [NotNull]
        public int order { get; set; }
        public string data { get; set; }
        public string data2 { get; set; }
    }
}
