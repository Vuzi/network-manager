
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace NetworkManager {

    /// <summary>
    /// Logique d'interaction pour Configuration.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window {
        public ObservableCollection<Parameter> ListConfig  { get; set; }

        public class Parameter {
            public string name { get; set; }
            public string key { get; set; }
            public string value { get; set; }
            public string description { get; set; }
        }

        public ConfigurationWindow() {
            InitializeComponent();
            this.DataContext = this;
            ListConfig = new ObservableCollection<Parameter>();

            Dictionary<string, Parameter> configs = new Dictionary<string, Parameter>();
            
            foreach(var item in App.config.list) {
                string[] itemKeys = item.Key.Split('.');
                if (configs.ContainsKey(itemKeys[0])) {
                    if (itemKeys.Length > 1 && itemKeys[1] == "name") {
                        configs[itemKeys[0]].name = item.Value;
                    } else if (itemKeys.Length > 1 && itemKeys[1] == "description") {
                        configs[itemKeys[0]].description = item.Value;
                    }
                } else {
                    configs.Add(item.Key, new Parameter() {
                        key = item.Key,
                        value = item.Value
                    });
                }
            }

            foreach (var value in configs.Values)
                ListConfig.Add(value);
        }

        private void button_Close_Click(object sender, RoutedEventArgs e) {
            this.Hide();
        }

        private void button_Save_Click(object sender, RoutedEventArgs e) {
            foreach (var itemInSoft in ListConfig) {
                App.config.set(itemInSoft.key, itemInSoft.value);
            }
            App.config.save();
            this.Hide();
        }
        
        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);
            e.Cancel = true;
            this.Hide();
        }
    }
}
