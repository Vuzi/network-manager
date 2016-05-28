
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using NetworkManager.Domain;
using Microsoft.Win32;

namespace NetworkManager.WMIExecution {
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

        const string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";


        public static List<string> GetInstalledPrograms() {
            var result = new List<string>();
            result.AddRange(GetInstalledProgramsFromRegistry(RegistryView.Registry32));
            result.AddRange(GetInstalledProgramsFromRegistry(RegistryView.Registry64));
            return result;
        }

        private static IEnumerable<string> GetInstalledProgramsFromRegistry(RegistryView registryView) {
            var result = new List<string>();

            using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView).OpenSubKey(registry_key)) {
                foreach (string subkey_name in key.GetSubKeyNames()) {
                    using (RegistryKey subkey = key.OpenSubKey(subkey_name)) {
                        if (IsProgramVisible(subkey)) {
                            result.Add((string)subkey.GetValue("DisplayName"));
                        }
                    }
                }
            }

            return result;
        }

        private static bool IsProgramVisible(RegistryKey subkey) {
            var name = (string)subkey.GetValue("DisplayName");
            var releaseType = (string)subkey.GetValue("ReleaseType");
            //var unistallString = (string)subkey.GetValue("UninstallString");
            var systemComponent = subkey.GetValue("SystemComponent");
            var parentName = (string)subkey.GetValue("ParentDisplayName");

            return
                !string.IsNullOrEmpty(name)
                && string.IsNullOrEmpty(releaseType)
                && string.IsNullOrEmpty(parentName)
                && (systemComponent == null);
        }

        /// <summary>
        /// Get a list of all the logged user on the provided computer. Note that all account will
        /// be returned, such as local service or anonymous logon
        /// </summary>
        /// <param name="computer">The computer to use</param>
        /// <returns>A list of all the logged in users</returns>
        public static IEnumerable<User> getLoggedUsers(Computer computer) {
            Dictionary<string, User> users = new Dictionary<string, User>();
            try {
                var scope = new ManagementScope($@"\\{computer.name}\root\cimv2", getConnectionOptions());
                scope.Connect();
                var Query = new SelectQuery("SELECT LogonId FROM Win32_LogonSession");
                var Searcher = new ManagementObjectSearcher(scope, Query);
                var regName = new Regex($"(?i)Name=\"(?<value>.+)\"");

                foreach (ManagementObject WmiObject in Searcher.Get()) {
                    foreach (ManagementObject LWmiObject in WmiObject.GetRelationships("Win32_LoggedOnUser")) {
                        Match m = regName.Match(LWmiObject["Antecedent"].ToString());
                        if (m.Success) {
                            string login = m.Groups["value"].Value;

                            // Look for user information
                            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, new SelectQuery($"SELECT * FROM Win32_Account WHERE Name='{login}'"));
                            try {
                                foreach (ManagementObject mo in searcher.Get()) {
                                    if (users.ContainsKey(mo["SID"].ToString()))
                                        continue;

                                    users[mo["SID"].ToString()] = (new User() {
                                        caption = mo["Caption"].ToString(),
                                        description = mo["Description"].ToString(),
                                        domain = mo["Domain"].ToString(),
                                        localAccount = (bool) mo["LocalAccount"],
                                        name = mo["Name"].ToString(),
                                        SID = mo["SID"].ToString(),
                                        SIDType = (byte) mo["SIDType"],
                                        status = mo["Status"].ToString()
                                    });
                                }
                            } catch(Exception e) {
                                // Ignore not found error
                            }
                        }
                    }
                }
            } catch (Exception e) {
                throw new WMIException() { error = e };
            }

            return users.Values.AsEnumerable();
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
