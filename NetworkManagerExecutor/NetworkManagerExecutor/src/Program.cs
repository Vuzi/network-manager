
using System;
using NetworkManager.DomainContent;
using SQLite;
using System.Threading.Tasks;
using NetworkManager.Scheduling;
using System.IO;
using System.Reflection;

using log4net;
using log4net.Config;
using NetworkManager;

namespace NetworkManagerExecutor {

    /// <summary>
    /// Network Manager Executor main (and only) class
    /// </summary>
    class Program {

        private static readonly ILog logger = LogManager.GetLogger(typeof(Program));

        private static ComputerInfoStore computerInfoStore;
        private static JobStore jobStore;

        private static DateTime now = DateTime.Now;

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

        /// <summary>
        /// Handle a job for a computer
        /// </summary>
        /// <param name="job">The job to handle</param>
        /// <param name="c">The computer to performs the job on</param>
        static void handleJobForComputer(Job job, ComputerInfo c) {
            Computer computer = new Computer(c);
            logger.Debug($"\tTasks processing for {c.name}");

            JobReport report = new JobReport() {
                computerName = computer.nameLong,
                startDateTime = now,
                error = false
            };

            Task.Run(async () => {
                report.tasksReports = await computer.performsTasks(job.tasks);
            }).Wait();

            // Update report
            foreach (JobTaskReport taskReport in report.tasksReports) {
                if (taskReport.error) {
                    report.error = true;
                    logger.Error($"\tTask failed : {taskReport.extra}");
                }
            }

            report.endDateTime = DateTime.Now;

            // Save the report
            jobStore.insertJobReport(job, report);
        }

        /// <summary>
        /// Handle the provided job. This will perform the job on every computer related to this job
        /// </summary>
        /// <param name="job">The job to handle</param>
        static void handleJob(Job job) {
            logger.Info($"Job => {job.name} ({job.id})");
            logger.Debug("\tComputers : ");
            foreach (var c in job.computers)
                logger.Debug("\t\t" + c.name);

            job.status = JobStatus.IN_PROGRESS;
            jobStore.updateJob(job);

            logger.Debug("\tTasks processing...");

            Parallel.ForEach(job.computers, c => {
                handleJobForComputer(job, c);
            });

            // Execution done
            job.lastExecutionDateTime = DateTime.Now;

            // If cyclic, re-schedule the job
            if (job.cyclic)
                job.status = JobStatus.SCHEDULED;
            // If not, update it as terminated and un-schedule it
            else {
                job.status = JobStatus.TERMINATED;
                job.unSchedule();
            }

            jobStore.updateJob(job);

            logger.Info($"\tJob {job.name} terminated, updated and unscheduled");
        }

        /// <summary>
        /// Handle a job by its ID
        /// </summary>
        /// <param name="id">The job ID</param>
        static void handleJob(string id) {
            Job job = null;

            try {
                job = jobStore.getJobById(id);

                if (job != null) {
                    handleJob(job);
                } else {
                    logger.Info($"Job => none ({id})");
                }
            } catch (Exception e) {
                logger.Error("An unexpected error occurred", e);

                if (job != null) {
                    try {
                        if (job.cyclic)
                            job.status = JobStatus.SCHEDULED;
                        else {
                            job.status = JobStatus.TERMINATED;
                            job.unSchedule();
                        }

                        jobStore.updateJob(job);
                    } catch (Exception e2) {
                        logger.Error("An unexpected error occurred during the handling of the previous exception", e2);
                    }
                }

                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Executor entry point. Job's id to execute should be provided as arguments
        /// </summary>
        /// <param name="args">Job's id to execute</param>
        static void Main(string[] args) {
            // Configure the logger
            configureLogger();

            try {
                logger.Info($"{Assembly.GetExecutingAssembly().GetName().Name} - v{Assembly.GetExecutingAssembly().GetName().Version}");
                logger.Debug($"Started in {Directory.GetCurrentDirectory()}");

                logger.Debug("Acquiring the lock...");

                // Lock on the filesystem. This lock is here to prevent multiple executor to access at once the database, and
                // cause concurrence issues (thanks SQLite..)
                Utils.Lock("nm.lck", () => {
                    logger.Debug("Lock acquired");

                    // Preapre the database
                    prepareDatabaseConnection();

                    foreach (string id in args) {
                        handleJob(id);
                    }
                });

                logger.Info("Execution done, now closing..");
            } catch (Exception e) {
                logger.Error("Unhandled exception occured", e);
                Environment.Exit(1);
            }
        }
    }
}
