using System;

namespace NetworkManager.Domain {
    public class Software {
        public string displayName { get; set; }
        public string displayVersion { get; set; }
        public DateTime installDate { get; set; }
        public string publisher { get; set; }
        public string comment { get; set; }
        public string installLocation { get; set; }
        public string estimatedSize { get; set; }

    }
}
