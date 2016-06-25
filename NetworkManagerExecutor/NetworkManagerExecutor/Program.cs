
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
                    Console.WriteLine($"Job => {j.name}");
                    Console.WriteLine("\tComputers : ");
                    foreach(var c in j.computers)
                        Console.WriteLine("\t\t" + c.name);

                    j.status = JobStatus.SCHEDULED;
                    //jobStore.updateStatus(j);

                    if (j.name == "Test Job #3") {
                        Parallel.ForEach(j.computers, c => {
                            Computer computer = new Computer(c);

                            try {
                                computer.performsTasks(j.tasks);
                            } catch (Exception e) {
                                Console.WriteLine(e.Message);
                            }
                        });

                        j.status = JobStatus.TERMINATED;
                        //jobStore.updateStatus(j);
                    }
                } else {
                    Console.WriteLine($"Job => none");
                }
            }

            Console.ReadLine();

        }
    }
}
