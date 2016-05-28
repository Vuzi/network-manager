using NetworkManager.Domain;
using NetworkManagerCore.WMIExecution;
using System;

namespace NetworkManager {
    public class Program {
        static void Main(string[] args) {
            foreach (Computer computer in Computer.getComputersInDomain()) {

                Console.WriteLine(computer.name);
                Console.WriteLine("Logged : ");
                foreach (string login in WMIExecutor.getLoggedUser(computer.domain, computer.name)) {
                    Console.WriteLine(login);
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
