using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace NetworkManagerApp.views
{
    /// <summary>
    /// Logique d'interaction pour AppsWindows.xaml
    /// </summary>
    public partial class AppsWindows : Window
    {
        public AppsWindows()
        {
            InitializeComponent();
        }

        private void BtAddApps_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                Console.WriteLine(File.ReadAllText(openFileDialog.FileName));
        }
    }
}
