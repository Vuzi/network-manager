
using System;

namespace NetworkManager.Scheduling {

    public enum TaskStatus {
        SCHEDULED = 1, IN_PROGRESS = 2, OK = 3, KO = 4
    }

    public enum TaskType {
        REBOOT = 1, BOOT = 2, SHUTDOWN = 3, INSTALL_SOFTWARE = 4, BASH = 5
    }

    /// <summary>
    /// Execution result of a task
    /// </summary>
    public class TaskResult {
        public string UUID { get; set; }

        /// <summary>
        /// Task performed
        /// </summary>
        public Task task { get; set; }

        /// <summary>
        /// Status of the task
        /// </summary>
        public TaskStatus status { get; set; }

        /// <summary>
        /// Computer which performed the task
        /// </summary>
        public string computerName { get; set; }
    
        /// <summary>
        /// Computer's domain
        /// </summary>
        public string domainName { get; set; }

        /// <summary>
        /// Launching of the task
        /// </summary>
        public DateTime start { get; set; }

        /// <summary>
        /// End of the task
        /// </summary>
        public DateTime end { get; set; }

        /// <summary>
        /// Return value of the task
        /// </summary>
        public int returnValue { get; set; }

        /// <summary>
        /// Output of the task
        /// </summary>
        public string output { get; set; }

        /// <summary>
        /// Error output of the task
        /// </summary>
        public string err { get; set; }
    }

    /// <summary>
    /// Planned task
    /// </summary>
    public class Task {
        public string UUID { get; set; }

        /// <summary>
        /// The type of the task to performs
        /// </summary>
        public TaskType type { get; set; }

        /// <summary>
        /// When should be performed the task
        /// </summary>
        public DateTime date { get; set; }

        /// <summary>
        /// Main value of the task
        ///  - In INSTALL_SOFTWARE mode, name of the application exe
        ///  - In BASH mode, content of the batch file
        /// </summary>
        public string value { get; set; }

        /// <summary>
        /// Extra value of the task
        ///  - In INSTALL_SOFTWARE mode, parameters provided to the exe
        ///  - In BASH mode, parameters provided to the batch
        /// </summary>
        public string extra { get; set; }
    }
}
