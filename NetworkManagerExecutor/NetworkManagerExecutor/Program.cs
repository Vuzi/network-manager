
using System;
using NetworkManager.DomainContent;
using SQLite;
using System.Threading.Tasks;
using NetworkManager.Scheduling;

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
                Job job = jobStore.getJobById(id);

                if (job != null) {
                    #if DEBUG
                    Console.WriteLine($"Job => {j.name}");
                    Console.WriteLine("\tComputers : ");
                    foreach(var c in job.computers)
                        Console.WriteLine("\t\t" + c.name);
                    #endif
                    
                    job.status = JobStatus.IN_PROGRESS;
                    job.startDateTime = DateTime.Now;
                    jobStore.updateJob(job);

                    Parallel.ForEach(job.computers, c => {
                        Computer computer = new Computer(c);

                        JobReport report = new JobReport() {
                            computerName = computer.nameLong,
                            error = false
                        };

                        Task.Run(async () => {
                            report.tasksReports = await computer.performsTasks(job.tasks);
                        }).Wait();

                        // Update report
                        foreach (JobTaskReport taskReport in report.tasksReports)
                            if (taskReport.error)
                                report.error = true;

                        // Save the report
                        jobStore.insertJobReport(job, report);
                    });

                    // Execution done
                    job.status = JobStatus.TERMINATED;
                    job.endDateTime = DateTime.Now;
                    jobStore.updateJob(job);

                    job.unSchedule();
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
