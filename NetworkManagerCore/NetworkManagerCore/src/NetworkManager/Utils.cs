using System;
using System.Collections.Generic;
using System.Security.Principal;
using WindowsInstaller;

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

        public static string GetBytesReadable(long i)
        {
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.### ") + suffix;
        }

        public static string GetMsiProperty(string msiFile, string property)
        {
            string retVal = string.Empty;

            // Create an Installer instance
            Type classType = Type.GetTypeFromProgID("WindowsInstaller.Installer");
            Object installerObj = Activator.CreateInstance(classType);
            Installer installer = installerObj as Installer;

            // Open the msi file for reading
            // 0 - Read, 1 - Read/Write
            Database database = installer.OpenDatabase(msiFile, 0);

            // Fetch the requested property
            string sql = String.Format("SELECT Value FROM Property WHERE Property ='{0}'", property );
            View view = database.OpenView(sql);
            view.Execute(null);

            // Read in the fetched record
            Record record = view.Fetch();
            if (record != null)
                retVal = record.get_StringData(1);

            return retVal;
        }
    }
}
