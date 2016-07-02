
using System;
using NetworkManager.DomainContent;
using SQLite;
using System.Threading.Tasks;
using NetworkManager.Scheduling;
using System.IO;
using System.Reflection;

using log4net;
using log4net.Config;

namespace NetworkManagerExecutor {
    class Program {

        private static readonly ILog logger = LogManager.GetLogger(typeof(Program));

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

        /// <summary>
        /// Prepare the logger
        /// </summary>
        static private void configureLogger() {
            var configFile = Directory.GetCurrentDirectory() + @"\config\log4net-executor.config";

            if (File.Exists(configFile)) {
                XmlConfigurator.Configure(new FileInfo(configFile));
            } else {
                BasicConfigurator.Configure();
                logger.Warn("No log4net configuration file found");
            }
        }

        static void Main(string[] args) {
            // Configure the logger
            configureLogger();

            logger.Info($"{Assembly.GetExecutingAssembly().GetName().Name} - v{Assembly.GetExecutingAssembly().GetName().Version}");
            logger.Debug($"Started in {Directory.GetCurrentDirectory()}");
        
            // Preapre the database
            prepareDatabaseConnection();

            foreach (string id in args) {
                Job job = null;

                try {
                    job = jobStore.getJobById(id);

                    if (job != null) {
                        logger.Info($"Job => {job.name} ({id})");
                        logger.Debug("\tComputers : ");
                        foreach (var c in job.computers)
                            logger.Debug("\t\t" + c.name);

                        job.status = JobStatus.IN_PROGRESS;
                        job.startDateTime = DateTime.Now;
                        jobStore.updateJob(job);

                        logger.Debug("\tTasks processing...");

                        Parallel.ForEach(job.computers, c => {
                            Computer computer = new Computer(c);
                            logger.Debug($"\tTasks processing for {c.name}");

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

                        logger.Info($"\tJob {job.name} terminated, updated and unscheduled");
                    } else {
                        logger.Info($"Job => none ({id})");
                    }
                } catch(Exception e) {
                    logger.Error("An unexpected error occurred", e);

                    if (job != null) {
                        try {
                            job.unSchedule();

                            job.status = JobStatus.TERMINATED;
                            job.endDateTime = DateTime.Now;
                            jobStore.updateJob(job);
                        } catch(Exception e2) {
                            logger.Error("An unexpected error occurred during the handling of the previous exception", e2);
                        }
                    }

                    Environment.Exit(1);
                }

                logger.Info("Execution done, now closing..");
            }
        }
    }
}
