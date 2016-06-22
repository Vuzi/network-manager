using NetworkManager.DomainContent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace NetworkManager.View.Model {

    public class DomainModel : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        public string name { get; set; }
        private Dictionary<string, ComputerModel> computers { get; set; }
        private Dictionary<string, DomainModel> domains { get; set; }

        internal void notifyPropertyChanged(String info) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public DomainModel() {
            computers = new Dictionary<string, ComputerModel>();
            domains = new Dictionary<string, DomainModel>();
        }

        public DomainModel getDomain(string name) {
            return domains.GetValueOrDefault(name);
        }

        public ComputerModel getComputer(string name) {
            return computers.GetValueOrDefault(name);
        }

        public void addDomain(DomainModel domain) {
            domains[domain.name] = domain;
            Items.Insert(computerIndex, domain);
            computerIndex++;

            notifyPropertyChanged("Domains");
        }

        public void addComputer(ComputerModel computer) {
            computers[computer.computer.name] = computer;
            Items.Add(computer);

            notifyPropertyChanged("Computers");
        }

        public void removeComputer(ComputerModel computer) {
            removeComputer(computer.computer.name);
        }

        public void removeComputer(string computer) {
            computers.Remove(computer);
            for (int i = computerIndex; i < Items.Count; i++) {
                var item = Items[i] as ComputerModel;

                if (item.computer.name == computer) {
                    Items.RemoveAt(i);
                    notifyPropertyChanged("Computers");
                    break;
                }
            }
        }

        public void removeDomain(DomainModel domain) {
            removeDomain(domain.name);
        }

        public void removeDomain(string domain) {
            domains.Remove(domain);
            for (int i = 0; i < computerIndex; i++) {
                var item = Items[i] as DomainModel;

                if (item.name == domain) {
                    Items.RemoveAt(i);
                    computerIndex--;
                    notifyPropertyChanged("Domains");
                    break;
                }
            }
        }

        public IEnumerable<ComputerModel> getComputers() {
            return computers.Values;
        }

        public IEnumerable<DomainModel> getDomains() {
            return domains.Values;
        }

        private int computerIndex;
        private ObservableCollection<object> items;
        public ObservableCollection<object> Items {
            get {
                if (items == null) {
                    items = new ObservableCollection<object>();
                    computerIndex = 0;
                }

                return items;
            }
        }


        public static async Task<DomainModel> createDomainModel(Domain domain, ErrorHandlerWindow erroHandler) {

            DomainModel domainModel = new DomainModel() {
                name = domain.name
            };

            foreach (Computer c in domain.computers) {
                domainModel.addComputer(new ComputerModel() {
                    computer = c
                });

                // If the computer is alive, save its values in the database
                if (c.isAlive) {
                    try {
                        var info = MainWindow.computerInfoStore.getComputerInfoByName(c.nameLong);

                        if (info == null || (DateTime.Now - info.lastUpdate).TotalDays > 30) {
                            MainWindow.computerInfoStore.updateOrInsertComputerInfo(new ComputerInfo() {
                                name = c.nameLong,
                                ipAddress = c.getIpAddress().ToString(),
                                macAddress = await c.getMacAddress(),
                                lastUpdate = DateTime.Now
                            });
                        }
                    } catch (Exception e) {
                        erroHandler.addError(e);
                    }
                }
            }

            foreach (Domain d in domain.domains) {
                domainModel.addDomain(await createDomainModel(d, erroHandler));
            }

            return domainModel;
        }

        public bool updateDomainModel(DomainModel domainModel, System.Collections.IList selectedComputers) {
            return updateDomainModel(this, domainModel, selectedComputers);
        }

        public static bool updateDomainModel(DomainModel oldDomainModel, DomainModel domainModel, System.Collections.IList selectedComputers) {
            bool needRefresh = false;

            // Update & adding of new computers
            foreach (var computerModel in domainModel.getComputers()) {
                var oldComputerModel = oldDomainModel.getComputer(computerModel.computer.name);

                if (oldComputerModel != null) {
                    // If an update is needed
                    if (oldComputerModel.computer.isAlive != computerModel.computer.isAlive) {
                        foreach (var item in selectedComputers) {
                            if (item is ComputerModel && (item as ComputerModel).computer.nameLong == computerModel.computer.nameLong) {
                                needRefresh = true;
                            }
                        }
                    }

                    oldComputerModel.computer = computerModel.computer;
                } else
                    oldDomainModel.addComputer(computerModel);
            }

            // Update & adding of new models
            foreach (var subdomainModel in domainModel.getDomains()) {
                var oldSubdomainModel = oldDomainModel.getDomain(subdomainModel.name);

                if (oldSubdomainModel != null)
                    needRefresh = needRefresh || updateDomainModel(oldSubdomainModel, subdomainModel, selectedComputers);
                else
                    oldDomainModel.addDomain(subdomainModel);
            }

            // Removing of old computers
            foreach (string computerToRemove in oldDomainModel.getComputers().Select(c => c.computer.name).
                                                Except(domainModel.getComputers().Select(c => c.computer.name)).
                                                ToList()) {
                oldDomainModel.removeComputer(computerToRemove);
            }

            // Removing of old domains
            foreach (string domainToRemove in oldDomainModel.getDomains().Select(d => d.name).
                                                Except(domainModel.getDomains().Select(d => d.name)).
                                                ToList()) {
                oldDomainModel.removeDomain(domainToRemove);
            }

            return needRefresh;
        }

        public bool updateComputerModel(Computer computer) {
            return updateComputerModel(this, computer);
        }

        public static bool updateComputerModel(DomainModel domainModel, Computer computer) {
            bool needRefresh = false;

            // Update & adding of new computers
            foreach (var computerModel in domainModel.getComputers()) {
                if (computerModel.computer.nameLong == computer.nameLong) {
                    if (computer.isAlive != computerModel.computer.isAlive) {
                        needRefresh = true;
                    }

                    computerModel.computer = computer;

                    return needRefresh;
                }
            }

            foreach (var subdomainModel in domainModel.getDomains()) {
                if ((needRefresh = (needRefresh || updateComputerModel(subdomainModel, computer))))
                    return needRefresh;
            }

            return needRefresh;
        }
    }
}
