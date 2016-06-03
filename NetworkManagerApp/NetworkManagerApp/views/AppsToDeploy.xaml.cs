using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace NetworkManagerApp.views
{
    /// <summary>
    /// Logique d'interaction pour AppsToDeploy.xaml
    /// </summary>
    public partial class AppsToDeploy : Window
    {
        public AppsToDeploy()
        {
            InitializeComponent();
            Loaded += AppsToDeploy_Loaded;
        }

        class ListBoxData
        {
            public BitmapSource ItemIcon { get; set; }
            public string ItemText { get; set; }
            public string ItemVersion { get; internal set; }
        }

        void AppsToDeploy_Loaded(object sender, RoutedEventArgs e)
        {
            textField.Text = "";
        }
        public Version GetAssemblyVersionFromObjectType(object o)
        {
           return o.GetType().Assembly.GetName().Version;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                textField.Text = dialog.SelectedPath;

                

                if (textField.Text == "")
                    return;

                string[] dir = Directory.GetFiles(textField.Text);

                var data = new List<ListBoxData>();

                foreach (var filePath in dir)
                {
                    
                    var sysicon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                    var bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                sysicon.Handle,
                                System.Windows.Int32Rect.Empty,
                                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    sysicon.Dispose();
                    FileVersionInfo myFI = FileVersionInfo.GetVersionInfo(filePath);
                    var itm = new ListBoxData() { ItemIcon = bmpSrc, ItemText = filePath, ItemVersion = myFI.FileVersion };

                    data.Add(itm);
                }

                lstBox.ItemsSource = data;
            }
            else
                textField.Text = "";
        }
    }
}
