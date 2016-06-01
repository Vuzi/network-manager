
using System;

using NetworkManager.Domain;
using NetworkManager.WMIExecution;

namespace NetworkManager {
    public class Program {
        static void Main(string[] args) {
            // Tests

            if (!Utils.isRunAsAdministrator())
                throw new Exception() {
                    Data = { { "why", "The application should only be used as an administrator" } },
                    Source = "NetworkManagerClient"
                };

            /*
            foreach (Computer computer in NetworkManager.Domain.Domain.getComputersInDomain()) {

                Console.WriteLine(computer.name);

                Console.WriteLine("Softwares : ");
                foreach (var soft in WMIExecutor.getInstalledSoftwares(computer, 64)) {
                    Console.WriteLine("\t" + soft.displayName + " " + soft.installDate);
                }
                Console.WriteLine("Logged : ");
                foreach (User user in WMIExecutor.getLoggedUsers(computer)) {
                    Console.WriteLine("\t" + user.name);
                }

                Console.WriteLine("========================================");
            
                try {
                    var r = computer.exec("DIR");
                    r.Wait();

                    Console.WriteLine("> out : ");
                    Console.Write(r.Result.output);
                    Console.WriteLine("> err : ");
                    Console.Write(r.Result.err);
                } catch (AggregateException e) {
                    WMIException wmie = (e as AggregateException).InnerException as WMIException;

                    Console.WriteLine("> failed : ");
                    Console.Write(wmie.error.Message);
                }
            }*/

            Console.ReadLine();
        }
    }
}
