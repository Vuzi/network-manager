
using System;
using NetworkManager.DomainContent;
using NetworkManager.Job;
using SQLite;
using System.Threading.Tasks;

namespace NetworkManagerExecutor {
    class Program {

        public static ComputerInfoStore computerInfoStore { get; private set; }
        public static JobStore jobStore { get; private set; }
        
        /// <summary>
        /// Prepare the SQLite connection
        /// </summary>
        static private void prepareDatabaseConnection() {
            var conn = new SQLiteConnection("NetworkManager.sqlite");

            computerInfoStore = new ComputerInfoStore(conn);
            jobStore = new JobStore(conn);
        }

        static void Main(string[] args) {

            // Preapre the database
            prepareDatabaseConnection();

            foreach (string id in args) {
                Job j = jobStore.getJobById(id);

                if (j != null) {
                    #if DEBUG
                    Console.WriteLine($"Job => {j.name}");
                    Console.WriteLine("\tComputers : ");
                    foreach(var c in j.computers)
                        Console.WriteLine("\t\t" + c.name);
                    #endif

                    j.status = JobStatus.IN_PROGRESS;
                    jobStore.updateJob(j);
                    
                    Parallel.ForEach(j.computers, async c => {
                        Computer computer = new Computer(c);

                        JobReport report = new JobReport() {
                            computerName = computer.nameLong,
                            error = false
                        };
                        report.tasksReports = await computer.performsTasks(j.tasks);

                        // Update report
                        foreach (JobTaskReport taskReport in report.tasksReports)
                            if (taskReport.error)
                                report.error = true;

                        // Save the report
                        jobStore.insertJobReport(j, report);
                    });

                    j.status = JobStatus.TERMINATED;
                    jobStore.updateJob(j);
                    
                }
                #if DEBUG
                else {
                    Console.WriteLine($"Job => none");
                }
                #endif
            }

            Console.ReadLine();

        }
    }
}
