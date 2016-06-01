using ActiveDs;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NetworkManager.Domain {
    public class Domain {

        public string name;
        public List<Computer> computers { get; set; }

        public Domain() {
            name = System.Environment.UserDomainName;
        }

        public Domain(string name) {
            this.name = name;
        }

        public async Task fill() {
            computers = await getComputersInDomain(name);
        }

        /// <summary>
        /// Return all the computers of the local domain
        /// </summary>
        /// <returns>All the computers of the local domain</returns>
        public async static Task<List<Computer>> getComputersInDomain() {
            return await getComputersInDomain(System.Environment.UserDomainName);
        }

        /// <summary>
        /// Return all the computer of the provided domain
        /// </summary>
        /// <param name="domain">The domain to use</param>
        /// <returns>All the computer of the domain</returns>
        public async static Task<List<Computer>> getComputersInDomain(string domain) {
            return await Task.Run(() => {
                List<Computer> Computers = new List<Computer>();

                DirectoryEntry entry = new DirectoryEntry("LDAP://" + domain);
                DirectorySearcher mySearcher = new DirectorySearcher(entry);
                mySearcher.Filter = ("(objectClass=computer)");
                mySearcher.SizeLimit = int.MaxValue;
                mySearcher.PageSize = int.MaxValue;

                var d = System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain();

                foreach (SearchResult searchResult in mySearcher.FindAll()) {
                    var ds = searchResult.GetDirectoryEntry();

                    string desc = (string)ds.Properties["description"].Value;
                    string os = (string)ds.Properties["operatingsystem"].Value;
                    string version = (string)ds.Properties["operatingsystemversion"].Value;
                    DateTime lastLogOn = GetDateTimeFromLargeInteger(ds.Properties["lastlogon"].Value as IADsLargeInteger);
                    DateTime creation = (DateTime)ds.Properties["whencreated"].Value;
                    DateTime lastChange = (DateTime)ds.Properties["whenchanged"].Value;

                    string name = searchResult.GetDirectoryEntry().Name;
                    if (name.StartsWith("CN="))
                        name = name.Remove(0, "CN=".Length);

                    bool isAlive = false;
                    try {
                        var p = new Ping();
                        isAlive = p.Send(name).Status == IPStatus.Success;
                    } catch(Exception e) { }

                    Computers.Add(new Computer() {
                        name = name,
                        domain = domain,
                        description = desc,
                        os = os,
                        version = version,
                        lastLogOn = lastLogOn,
                        lastChange = lastChange,
                        creation = creation,
                        isAlive = isAlive
                    });
                }

                mySearcher.Dispose();
                entry.Dispose();
                return Computers;
            });
        }

        private static DateTime GetDateTimeFromLargeInteger(IADsLargeInteger largeIntValue) {
            if (largeIntValue == null)
                return DateTime.MinValue;

            long int64Value = (long)((uint)largeIntValue.LowPart + (((long)largeIntValue.HighPart) << 32));
            return DateTime.FromFileTimeUtc(int64Value);
        }
    }
}
