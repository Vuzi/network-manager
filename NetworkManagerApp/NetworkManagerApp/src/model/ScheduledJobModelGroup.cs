using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NetworkManager.Model {
    class ScheduledJobModelGroup {
        public bool selected { get; set; }

        public string _name;
        public string name {
            get {
                return _name;
            }
            set {
                _name = value;
                notifyPropertyChanged("name");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<ScheduledJobModel> Items { get; set; } = new ObservableCollection<ScheduledJobModel>();

        internal void notifyPropertyChanged(String info) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
