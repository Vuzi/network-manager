using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NetworkManagerApp.view
{
    /// <summary>
    /// Logique d'interaction pour Configuration.xaml
    /// </summary>
    public partial class Configuration : Window
    {
        public ObservableCollection<Parameter> ListConfig  { get; set; }
        private Properties config;

        public class Parameter
        {
            public string Name { get; set; }
            public string Value { get; set; }

            public Parameter(string _Name, string _Value)
            {
                this.Name = _Name;
                this.Value = _Value;
            }
        }

    public Configuration()
        {
            InitializeComponent();
            this.DataContext = this;
            ListConfig = new ObservableCollection<Parameter>();
            /*load*/
            config = new Properties(@"C:\Users\Administrateur\Documents\Visual Studio 2015\Projects\NetworkManager\NetworkManagerApp\NetworkManagerApp\config\parameters.properties");

            /*set value
            config.set("testParam1", "testValeur1");
            config.set("testParam2", "testValeur2");
            config.set("testParam3", "testValeur3");
            //save
            config.Save();*/
            foreach(var item in config.list)
            {
                ListConfig.Add(new Parameter(item.Key, item.Value));
            }
            
        }
        private void button_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        private void button_Save_Click(object sender, RoutedEventArgs e)
        {
            System.IO.File.WriteAllText(@"C:\Users\Administrateur\Documents\Visual Studio 2015\Projects\NetworkManager\NetworkManagerApp\NetworkManagerApp\config\parameters.properties", string.Empty);
            config.list.Clear();
            foreach (var itemInSoft in ListConfig)
            {
               config.set(itemInSoft.Name, itemInSoft.Value);
            }
            config.Save();
            this.Hide();
        }
        private void button_Add_Row_Click(object sender, RoutedEventArgs e)
        {
            ListConfig.Add(new Parameter("", ""));
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            this.Hide();
        }
    }
}
