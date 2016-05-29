using NetworkManagerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace NetworkManager {
    static class Program {

        static void Main(string[] args) {
            Service service = new Service();

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
