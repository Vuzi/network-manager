
using SQLite;
using System;

namespace NetworkManager.DomainContent {

    /// <summary>
    /// Saved computer info in the database
    /// </summary>
    public class ComputerInfo {
        [PrimaryKey, NotNull]
        public string name { get; set; }
        [NotNull]
        public string ipAddress { get; set; }
        [NotNull]
        public string macAddress { get; set; }
        [NotNull]
        public DateTime lastUpdate { get; set; }
    }
}
