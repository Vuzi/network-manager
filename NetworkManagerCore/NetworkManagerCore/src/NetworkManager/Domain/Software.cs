using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkManager.Domain {
    public class Software {
        public string displayName { get; set; }
        public string displayVersion { get; set; }
        public DateTime installDate { get; set; }
        public string publisher { get; set; }
        public string comment { get; set; }

    }
}
