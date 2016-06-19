using ActiveDs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NetworkManager.DomainContent {
    public class Domain {

        public string name { get; set;}
        public List<Computer> computers { get; set; }
        public List<Domain> domains { get; set; }

        public Domain() {
            this.name = getLocalDomainName();
        }

        public Domain(string name) {
            this.name = name;
        }

        public async Task fill(bool getStatus = true) {
            computers = await getComputersInDomain(name, getStatus);
            domains = await getDomainsInDomain(name);

            foreach(Domain d in domains)
                await d.fill(getStatus);
        }

        public static string getLocalDomainName() {
            return IPGlobalProperties.GetIPGlobalProperties().DomainName;
        }

        /// <summary>
        /// Return all the computers of the local domain
        /// </summary>
        /// <returns>All the computers of the local domain</returns>
        public async static Task<List<Computer>> getComputersInDomain() {
            return await getComputersInDomain(getLocalDomainName());
        }

        public async static Task<List<Domain>> getDomainsInDomain(string domain) {
            return await Task.Run(() => {
                List<Domain> domains = new List<Domain>();

                // For each sub-domain
                using (var forest = Forest.GetCurrentForest()) {
                    foreach (System.DirectoryServices.ActiveDirectory.Domain d in forest.Domains) {
                        if(d.Name.ToLower() == domain.ToLower()) {
                            foreach (System.DirectoryServices.ActiveDirectory.Domain c in d.Children) {
                                Domain subDomain = new Domain() { name = c.Name };
                                domains.Add(subDomain);
                                c.Dispose();
                            }
                        }
                        d.Dispose();
                    }
                }

                return domains;
            });
        }

        /// <summary>
        /// Return all the computer of the provided domain
        /// </summary>
        /// <param name="domain">The domain to use</param>
        /// <returns>All the computer of the domain</returns>
        public async static Task<List<Computer>> getComputersInDomain(string domain, bool getStatus = true) {
            return await Task.Run(() => {
                List<Computer> computers = new List<Computer>();

                DirectoryEntry entry = new DirectoryEntry("LDAP://" + domain);
                DirectorySearcher mySearcher = new DirectorySearcher(entry);
                mySearcher.Filter = ("(objectClass=computer)");
                mySearcher.SizeLimit = int.MaxValue;
                mySearcher.PageSize = int.MaxValue;

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

                    computers.Add(new Computer() {
                        name = name,
                        domain = domain,
                        description = desc,
                        os = os,
                        version = version,
                        lastLogOn = lastLogOn,
                        lastChange = lastChange,
                        creation = creation,
                        isAlive = false // Default
                    });
                }

                mySearcher.Dispose();
                entry.Dispose();

                // Update alive computers
                if (getStatus) {
                    var tasks = computers.Select(computer => {
                        return Task.Run(() => {
                            bool isAlive = false;

                            try {
                                var p = new Ping();
                                isAlive = p.Send(computer.nameLong, 250).Status == IPStatus.Success;
                            } catch (Exception) { }
                            computer.isAlive = isAlive;
                        });
                    });

                    Task.WhenAll(tasks).Wait();
                }

                return computers;
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
