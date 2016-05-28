using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace NetworkManagerService {
    static class Program {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        static void Main(string[] args) {
            Service1 service = new Service1();

            if (Environment.UserInteractive) {
                service.Start(args);
                Console.WriteLine("Press any key to stop program");
                Console.Read();
                service.Stop();
            } else {
                ServiceBase.Run(service);
            }
        }
    }
}
