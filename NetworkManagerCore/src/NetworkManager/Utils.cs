using System.Security.Principal;

namespace NetworkManager {
    class Utils {
        
        public static bool isRunAsAdministrator() {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                    .IsInRole(WindowsBuiltInRole.Administrator);
        }

    }
}
