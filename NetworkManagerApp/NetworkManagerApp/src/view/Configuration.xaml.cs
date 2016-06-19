
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace NetworkManager {

    /// <summary>
    /// Logique d'interaction pour Configuration.xaml
    /// </summary>
    public partial class Configuration : Window {
        public ObservableCollection<Parameter> ListConfig  { get; set; }
        private Properties config;

        public class Parameter {
            public string Name { get; set; }
            public string Value { get; set; }

            public Parameter(string _Name, string _Value) {
                this.Name = _Name;
                this.Value = _Value;
            }
        }

        public Configuration() {
            InitializeComponent();
            this.DataContext = this;
            ListConfig = new ObservableCollection<Parameter>();

            config = new Properties(@"config\parameters.properties");

            foreach(var item in config.list) {
                ListConfig.Add(new Parameter(item.Key, item.Value));
            }
        }

        private void button_Close_Click(object sender, RoutedEventArgs e) {
            System.IO.File.WriteAllText(@"config\parameters.properties", string.Empty);
            config.list.Clear();
            foreach (var item in config.list) {
                ListConfig.Add(new Parameter(item.Key, item.Value));
            }
            this.Hide();
        }

        private void button_Save_Click(object sender, RoutedEventArgs e) {
            System.IO.File.WriteAllText(@"config\parameters.properties", string.Empty);
            config.list.Clear();
            foreach (var itemInSoft in ListConfig) {
               config.set(itemInSoft.Name, itemInSoft.Value);
            }
            config.Save();
            this.Hide();
        }

        private void button_Add_Row_Click(object sender, RoutedEventArgs e) {
            ListConfig.Add(new Parameter("", ""));
        }
        
        protected override void OnClosing(CancelEventArgs e) {
            System.IO.File.WriteAllText(@"config\parameters.properties", string.Empty);
            config.list.Clear();
            foreach (var item in config.list) {
                ListConfig.Add(new Parameter(item.Key, item.Value));
            }
            base.OnClosing(e);
            e.Cancel = true;
            this.Hide();
        }
    }
}
