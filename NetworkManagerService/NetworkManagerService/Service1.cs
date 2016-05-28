using NetworkManager.Scheduling;
using System;
using System.Data.SQLite;
using System.Linq;
using System.ServiceProcess;

namespace NetworkManagerService {
    public partial class Service1 : ServiceBase {
        public Service1() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            Console.WriteLine("hello world");

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

            taskResult = taskStore.getTasksResultByTask(task).FirstOrDefault();

            Console.WriteLine("hello world");
        }

        protected override void OnStop() {
        }

        public void Start(string[] args) {
            OnStart(args);
        }
    }
}
