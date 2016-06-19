
namespace NetworkManager.Job {
    
    public enum JobTaskType {
        INSTALL_SOFTWARE, WAKE_ON_LAN, REBOOT, SHUTDOWN
    }

    /// <summary>
    /// Job task of a scheduled job. A job task is an action to performs.
    /// </summary>
    public class JobTask {
        public JobTaskType type { get; set; }
        public int timeout { get; set; }
        public string data { get; set; }
    }
}
