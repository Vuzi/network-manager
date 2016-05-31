
using NetworkManager.WMIExecution;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetworkManager.Domain {
    public class User {
        public string caption { get; set; }
        public string description { get; set; }
        public string domain { get; set; }
        public bool localAccount { get; set; }
        public string name { get; set; }
        public string SID { get; set; }
        public string status { get; set; }
        public byte SIDType { get; set; }

        /// <summary>
        /// Return the logged users on the computed. Note that only real user accounts will
        /// be returned
        /// </summary>
        /// <returns>The logged user</returns>
        public static async Task<IEnumerable<User>> getLoggedUsers(Computer computer) {
            return (await WMIExecutor.getLoggedUsers(computer)).Where(u => u.SIDType == 1);
        }

        /// <summary>
        /// Return all the logged users on the computer. Note that local system account will
        /// also be returned
        /// </summary>
        /// <returns>ALl the logged users</returns>
        public static async Task<IEnumerable<User>> getAllLoggedUsers(Computer computer) {
            return (await WMIExecutor.getLoggedUsers(computer));
        }
    }
}
