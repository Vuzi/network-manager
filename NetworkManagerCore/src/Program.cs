using NetworkManager.Domain;
using NetworkManager.WMIExecution;
using System;

namespace NetworkManager {
    public class Program {
        static void Main(string[] args) {
            if (!Utils.isRunAsAdministrator())
                throw new Exception() {
                    Data = { { "why", "The application should only be used as an administrator" } },
                    Source = "NetworkManagerClient"
                };

            Console.WriteLine(">>>>>>>>>>>>>>>");
            foreach (string s in WMIExecutor.GetInstalledPrograms())
                Console.WriteLine(s);


            foreach (Computer computer in Computer.getComputersInDomain()) {

                Console.WriteLine(computer.name);
                Console.WriteLine("Logged : ");
                foreach (User user in WMIExecutor.getLoggedUsers(computer)) {
                    Console.WriteLine(user.name);
                }

                /*
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
                }*/
            }

            Console.ReadLine();
        }
    }
}
