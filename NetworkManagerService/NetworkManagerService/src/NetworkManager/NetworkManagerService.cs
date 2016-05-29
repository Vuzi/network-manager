using NetworkManager.Domain;
using NetworkManager.Scheduling;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Timers;

namespace NetworkManager {
    public class NetworkManagerService {
        
        private Timer timer;
        private SQLiteConnection conn;
        private TaskStore taskStore;

        private Dictionary<TaskType, Action<Computer, TaskResult>> handlers = 
            new Dictionary<TaskType, Action<Computer, TaskResult>> {
            {
                TaskType.REBOOT,
                (computer, taskResult) => {
                    computer.reboot();
                    taskResult.returnValue = 0;
                }
            },
            {
                TaskType.BOOT,
                (computer, taskResult) => {
                    // TODO
                    // wakeOnLan
                    // store mac in extra field
                    taskResult.returnValue = 0;
                }
            },
            {
                TaskType.SHUTDOWN,
                (computer, taskResult) => {
                    computer.shutdown();
                    taskResult.returnValue = 0;
                }
            },
            {
                TaskType.INSTALL_SOFTWARE,
                (computer, taskResult) => {
                    // TODO
                    // copy local file to remote
                    // start the file at the remote
                    // wait for termination
                    // update values
                }
            },
            {
                TaskType.INSTALL_SOFTWARE,
                (computer, taskResult) => {
                    // TODO
                    // copy local batch file to remote
                    // start the batch file at the remote
                    // wait for termination
                    // update values
                }
            }
        };

        /// <summary>
        /// Prepare the SQLite connection
        /// </summary>
        private void prepareDatabaseConnection() {
            SQLiteConnection.CreateFile("NetworkManager.sqlite");
            conn = new SQLiteConnection("Data Source=NetworkManager.sqlite;Version=3;");
            conn.Open();
            taskStore = new TaskStore(conn);
        }

        /// <summary>
        /// Create and initialize the service core. It will create a time which will
        /// check which tasks should be performed, and performs them, and then log
        /// the result
        /// </summary>
        public NetworkManagerService() {

            // Connect to the local database
            prepareDatabaseConnection();

            // Create the timer
            timer = new Timer(5000); //5s
            timer.Elapsed += (sender, e) => {

                try {
                    // Get the tasks to performs
                    foreach(var task in taskStore.getTaskToPerforms()) {
                        foreach(var taskResult in taskStore.getTasksResultByTask(task)) {

                            var computer = new Computer() {
                                name = taskResult.computerName,
                                domain = taskResult.domainName
                            };

                            try {
                                taskResult.start = DateTime.Now;
                                handlers[task.type](computer, taskResult);
                                taskResult.status = TaskStatus.OK;
                            } catch (WMIExecution.WMIException ex) {
                                taskResult.status = TaskStatus.KO;
                                taskResult.err = ex.error.Message;
                            } catch (Exception ex) {
                                taskResult.status = TaskStatus.KO;
                                taskResult.err = ex.Message;
                            } finally {
                                taskResult.end = DateTime.Now;
                            }

                            // Update the taskResult
                            taskStore.updateTaskResult(taskResult);
                        }
                    }

                } catch (Exception ex) {
                    // Log the error
                    Console.WriteLine(ex.Message);
                } finally {
                    // Restart the timer
                    timer.Start();
                }

            };
            timer.AutoReset = false;
        }

        /// <summary>
        /// Start the service timer
        /// </summary>
        public void start() {
            timer.Start();
        }

        /// <summary>
        /// Stop the service timer
        /// </summary>
        public void stop() {
            timer.Stop();
        }
    }
}
