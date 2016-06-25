using NetworkManager.DomainContent;
using System;
using System.ComponentModel;

namespace NetworkManager.View.Model {

    public class ComputerModel : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        private Computer _computer;
        private bool _isHide;

        public Computer computer {
            get {
                return _computer;
            }

            set {
                if (_computer != null)
                    value.copyCache(_computer); // Keep in memory the cached values of the computer
                _computer = value;
                notifyPropertyChanged("computer");
            }
        }

        internal void notifyPropertyChanged(String info) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        public bool isHide
        {
            get {
                return _isHide;
            }
            set {
                _isHide = value;
                notifyPropertyChanged("isHide");
            }
        }
    }

}
