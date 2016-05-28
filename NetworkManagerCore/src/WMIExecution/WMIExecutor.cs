
using NetworkManager.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetworkManagerCore.WMIExecution {
    /// <summary>
    /// WMI Execution result
    /// </summary>
    public class WMIExecutionResult {
        public string output;
        public string err;
        public uint returnValue;
        public bool timeout;
        public long duration;
    }

    /// <summary>
    /// WMI Exception
    /// </summary>
    public class WMIException : Exception {
        public Exception error;
    }

    /// <summary>
    /// Class to performs asynchronous WMI requests
    /// </summary>
    public class WMIExecutor {
        private static ConnectionOptions connectionOptions;

        private static ConnectionOptions getConnectionOptions() {
            if (connectionOptions == null) {
                connectionOptions = new ConnectionOptions();
                connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
                connectionOptions.EnablePrivileges = true;
            }

            return connectionOptions;
        }

        private static void genericWMI(Computer computer, string path, string method) {
            try {
                // Create the scope
                var wmiScope = new ManagementScope($@"\\{computer.name}\root\cimv2", getConnectionOptions());

                // Get the WMI process
                var wmiProcess = new ManagementClass(wmiScope, new ManagementPath(path), null /*new ObjectGetOptions()*/);

                // Action
                uint returnValue = (uint)wmiProcess.GetInstances().OfType<ManagementObject>().FirstOrDefault().InvokeMethod(method, new object[] { });

                if (returnValue != 0)
                    throw new Exception($"Method {method} from {path} failed with code {returnValue}");

            } catch (Exception e) {
                throw new WMIException() { error = e };
            }
        }

        // TODO user class
        // TODO get system user ?
        public static List<string> getLoggedUser(Computer computer) {
            List<string> users = new List<string>();
            try {
                var scope = new ManagementScope($@"\\{computer.name}\root\cimv2", getConnectionOptions());
                scope.Connect();
                var Query = new SelectQuery("SELECT LogonId FROM Win32_LogonSession Where LogonType=10 or LogonType=2");
                var Searcher = new ManagementObjectSearcher(scope, Query);
                var regName = new Regex($"(?i)Domain=\"{computer.domain.ToLower()}\",Name=\"(?<value>.+)\"");

                foreach (ManagementObject WmiObject in Searcher.Get()) {
                    foreach (ManagementObject LWmiObject in WmiObject.GetRelationships("Win32_LoggedOnUser")) {
                        Match m = regName.Match(LWmiObject["Antecedent"].ToString());
                        if (m.Success)
                            users.Add(m.Groups["value"].Value);
                    }
                }
            } catch (Exception ex) {
                users.Add(ex.Message);
            }

            return users;
        }

        /// <summary>
        /// Shutdown the computer
        /// </summary>
        /// <param name="computer">The computer to shutdown</param>
        public static void shutdown(Computer computer) {
            genericWMI(computer, "Win32_OperatingSystem", "Shutdown");
        }

        /// <summary>
        /// Reboot the computer
        /// </summary>
        /// <param name="computer">The computer to reboot</param>
        public static void reboot(Computer computer) {
            genericWMI(computer, "Win32_OperatingSystem", "Reboot");
        }

        /// <summary>
        /// Performs a WMI task asynchronously
        /// </summary>
        /// <param name="computer">The target computer</param>
        /// <param name="command">The command to perform</param>
        /// <param name="args">The command arguments</param>
        /// <param name="maxDuration">The maximum duraction of the task</param>
        /// <returns></returns>
        public async static Task<WMIExecutionResult> exec(Computer computer, string command, string[] args = null, long maxDuration = 5000, bool logOutput = true) {
            args = args ?? new string[0];

            // Perform the operation asynchronously
            return await Task.Run(() => {
                try {
                    string uuid = Guid.NewGuid().ToString();
                    string outputPath = $@"Windows\Temp\{uuid}_out";
                    string errPath = $@"Windows\Temp\{uuid}_err";

                    // Create the scope
                    var wmiScope = new ManagementScope($@"\\{computer.name}\root\cimv2", getConnectionOptions());

                    // Get the WMI process
                    var wmiProcess = new ManagementClass(wmiScope, new ManagementPath("Win32_Process"), new ObjectGetOptions());

                    // Method Options
                    InvokeMethodOptions methodOptions = new InvokeMethodOptions(null, TimeSpan.MaxValue);

                    // Parameters
                    // Note that since there is no way of getting the output, CMD is invoked and will write its output to the specified files
                    // then a timeout will wait, and if the process is terminated the two file will be retreived and returned
                    var inParams = wmiProcess.GetMethodParameters("Create");
                    if (logOutput)
                        inParams["CommandLine"] = $@"CMD /U /S /C {command} {string.Join(" ", args)} > C:\{outputPath} 2> C:\{errPath}";
                    else
                        inParams["CommandLine"] = $@"CMD /U /S /C {command} {string.Join(" ", args)}";

                    // Exec
                    ManagementBaseObject outParams = wmiProcess.InvokeMethod("Create", inParams, methodOptions);

                    // Query for the process id
                    ObjectQuery query = new ObjectQuery($"select * from Win32_Process Where ProcessId='{outParams["processId"]}'");
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiScope, query);

                    // Wait for either the process to terminate or the timeout
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    bool timeout = true;
                    while (stopWatch.ElapsedMilliseconds < maxDuration) {
                        if (searcher.Get().Count <= 0) {
                            timeout = false;
                            break;
                        }
                    }

                    stopWatch.Stop();

                    // Result to return
                    var result = new WMIExecutionResult {
                        returnValue = (uint)outParams["returnValue"],
                        timeout = timeout,
                        duration = stopWatch.ElapsedMilliseconds
                    };

                    // Get the outputs (if logging and no timeout has occurred)
                    if (logOutput && !timeout) {
                        string outputPathLocal = $@"\\{computer.name}\c$\{outputPath}";
                        string errPathLocal = $@"\\{computer.name}\c$\{errPath}";

                        result.output = File.ReadAllText(outputPathLocal, Encoding.Unicode);
                        //File.Delete(outputPathLocal);

                        result.err = File.ReadAllText(errPathLocal, Encoding.Unicode);
                        //File.Delete(errPathLocal);
                    }

                    // Return the values
                    return result;
                } catch (Exception e) {
                    throw new WMIException() { error = e };
                }
            });
        }
    }
}
