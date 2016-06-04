using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net;
using System.Security.Principal;

namespace NetworkManager {

    public static class Utils {

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) {
            TValue ret;
            dictionary.TryGetValue(key, out ret);
            return ret;
        }

        /// <summary>
        /// Return true if the application is run as an administrator, false otherwise
        /// </summary>
        /// <returns>True if the application is run as an administrator, false otherwise</returns>
        public static bool isRunAsAdministrator() {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                    .IsInRole(WindowsBuiltInRole.Administrator);
        }
        
        /// <summary>
        /// Open the file explorer on the C disk of the remote computer
        /// </summary>
        public static void showExplorer(string path) {
            Process rdcProcess = new Process();
            rdcProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\explorer.exe");
            rdcProcess.StartInfo.Arguments = path;
            rdcProcess.Start();
        }

        /// <summary>
        /// Send a wake on lan magic packet to wake up the specified MAC address
        /// </summary>
        /// <param name="macAddress"></param>
        public static void wakeOnLan(string macAddress) {
            PhysicalAddress.Parse(macAddress.Replace(':', '-')).SendWol();
        }
    }
}
