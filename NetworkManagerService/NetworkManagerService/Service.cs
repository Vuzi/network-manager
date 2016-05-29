using NetworkManager.Scheduling;
using System;
using System.Data.SQLite;
using System.Linq;
using System.ServiceProcess;

namespace NetworkManager {
    public partial class Service : ServiceBase {

        private NetworkManagerService service;

        /// <summary>
        /// Initialize the service
        /// </summary>
        public Service() {
            InitializeComponent();

            service = new NetworkManagerService();
        }

        /// <summary>
        /// When the service should start
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args) {
            service.start();
            
            /*
            Task task = new Task() {
                UUID = Guid.NewGuid().ToString(),
                date = DateTime.Now,
                type = TaskType.INSTALL_SOFTWARE,
                value = "Firefox",
                extra = "-ms" 
            };
            
            SQLiteConnection.CreateFile("NetworkManager.sqlite");
            var conn = new SQLiteConnection("Data Source=NetworkManager.sqlite;Version=3;");
            conn.Open();
            var taskStore = new TaskStore(conn);

            taskStore.insertTask(task);

            TaskResult taskResult = new TaskResult() {
                UUID = Guid.NewGuid().ToString(),
                task = task,
                computerName = "computerName",
                domainName = "domainName",
                status = TaskStatus.SCHEDULED
            };

            taskStore.insertTaskResult(taskResult);

            taskResult = taskStore.getTasksResultByTask(task).FirstOrDefault();*/
        }

        /// <summary>
        /// When the service should stop
        /// </summary>
        protected override void OnStop() {
            service.stop();
        }

        /// <summary>
        /// Manually start the service
        /// </summary>
        /// <param name="args"></param>
        public void Start(string[] args) {
            OnStart(args);
        }
    }
}
