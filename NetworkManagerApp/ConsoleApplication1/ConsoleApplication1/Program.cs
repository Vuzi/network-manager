using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1 {
    class Program {
        static void Main(string[] args) {

            Console.WriteLine("Sending WOL...");
            PhysicalAddress.Parse("44-8A-5B-6E-6E-26").SendWol();
            Console.WriteLine("WOL send!");

            Console.ReadLine();
        }
    }
}
