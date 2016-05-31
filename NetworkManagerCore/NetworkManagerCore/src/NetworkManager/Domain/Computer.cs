
using ActiveDs;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.Collections.Generic;

using NetworkManager.WMIExecution;

namespace NetworkManager.Domain {

    /// <summary>
    /// A computer in the domain
    /// </summary>
    public class Computer {
        public string name { get; set; }
        public string domain { get; set; }
        public string os { get; set; }
        public string version { get; set; }
        public string description { get; set; }
        public DateTime lastLogOn { get; set; }
        public DateTime creation { get; set; }
        public DateTime lastChange { get; set; }
        public bool isAlive { get; set; }

        /// <summary>
        /// Upload a file to the domain computer. The path should either be a full name including partion
        /// (i.e. C:\test\file.txt) or include a shared folder (i.e. \shared\file.txt)
        /// </summary>
        /// <param name="local">The local filename</param>
        /// <param name="remote">The absolution remote filename</param>
        public void uploadFile(string local, string remote) {
            File.Copy(local, $@"\\{name}\{remote.Replace(':', '$')}", true);
        }

        /// <summary>
        /// Download a file to the local computer from the remote computer. The path should either be a 
        /// full name including partion (i.e. C:\test\file.txt) or include a shared folder (i.e. \shared\file.txt)
        /// </summary>
        /// <param name="remote">The absolution remote filename</param>
        /// <param name="local">The local filename</param>
        public void downloadFile(string remote, string local) {
            File.Copy($@"\\{name}\{remote.Replace(':', '$')}", local);
        }

        /// <summary>
        /// Delete a file from the remote computer. The path should either be a full name including partion
        /// (i.e. C:\test\file.txt) or include a shared folder (i.e. \shared\file.txt)
        /// </summary>
        /// <param name="file">The absolution remote filename</param>
        public void deleteFile(string file) {
            File.Delete($@"\\{name}\{file.Replace(':', '$')}");
        }

        /// <summary>
        /// Execute a remote action
        /// </summary>
        /// <param name="command">The command</param>
        /// <param name="args">The command arguments</param>
        /// <param name="maxDuration">The maximum duration of the command</param>
        /// <returns>The async task of the action</returns>
        public async Task<WMIExecutionResult> exec(string command, string[] args = null, long maxDuration = 5000) {
            return await WMIExecutor.exec(this, command, args, maxDuration);
        }

        /// <summary>
        /// Execute a remote action
        /// </summary>
        /// <param name="command">The command</param>
        /// <param name="args">The command arguments</param>
        /// <param name="maxDuration">The maximum duration of the command</param>
        /// <param name="callback">The callback to call after the remote action</param>
        public void exec(string command, string[] args, Action<WMIExecutionResult> callback, Action<WMIException> callbackError = null, long maxDuration = 5000) {
            WMIExecutor.exec(this, command, args, maxDuration).ContinueWith(result => {
                if (result.Exception != null)
                    if(callbackError != null)
                        callbackError(result.Exception.InnerException as WMIException);
                else
                    callback(result.Result);
            });
        }

        /// <summary>
        /// Shutdown the computer
        /// </summary>
        public void shutdown() {
            WMIExecutor.shutdown(this);
        }

        /// <summary>
        /// Reboot the computer
        /// </summary>
        public void reboot() {
            WMIExecutor.reboot(this);
        }

        /// <summary>
        /// Start a remote desktop connection to the computer
        /// </summary>
        public void startRemoteDesktop() {
            Process rdcProcess = new Process();
            rdcProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\mstsc.exe");
            rdcProcess.StartInfo.Arguments = $"/v {name} /prompt /admin";
            rdcProcess.Start();
        }

        /// <summary>
        /// Open the file explorer on the C disk of the remote computer
        /// </summary>
        public void showExplorer() {
            Process rdcProcess = new Process();
            rdcProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\explorer.exe");
            rdcProcess.StartInfo.Arguments = $@"\\{name}\c$";
            rdcProcess.Start();
        }

        /// <summary>
        /// Return the logged users on the computed. Note that only real user accounts will
        /// be returned
        /// </summary>
        /// <returns>The logged user</returns>
        public async Task<IEnumerable<User>> getLoggedUsers() {
            return (await WMIExecutor.getLoggedUsers(this)).Where(u => u.SIDType == 1);
        }

        /// <summary>
        /// Return all the logged users on the computer. Note that local system account will
        /// also be returned
        /// </summary>
        /// <returns>ALl the logged users</returns>
        public async Task<IEnumerable<User>> getAllLoggedUsers() {
            return await WMIExecutor.getLoggedUsers(this);
        }

        /// <summary>
        /// Return all the sofwares installed on the computer.
        /// </summary>
        /// <returns>All the installed sofwares</returns>
        public async Task<IEnumerable<Software>> getInstalledSofwares() {
            var results = await Task.WhenAll(
                Task.Run(() => WMIExecutor.getInstalledSoftwares(this, 32)), 
                Task.Run(() => WMIExecutor.getInstalledSoftwares(this, 64)));
            return results.SelectMany(result => result);
        }
    }

}
