using System.Collections.Generic;
using System.Security.Principal;

namespace NetworkManager {

    public static class Utils {

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) {
            TValue ret;
            dictionary.TryGetValue(key, out ret);
            return ret;
        }

        public static bool isRunAsAdministrator() {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                    .IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
