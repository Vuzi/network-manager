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
using NetworkManager;
using NetworkManager.Domain;

namespace NetworkManagerApp.views
{
    /// <summary>
    /// Logique d'interaction pour AppsToDeploy.xaml
    /// </summary>
    public partial class AppsToDeploy : Window
    {
        private Computer currentComputer;

        public AppsToDeploy(Computer computer)
        {

            InitializeComponent();
            Loaded += AppsToDeploy_Loaded;
            this.currentComputer = computer;
            this.AppViewName.Title = "Install software on "+currentComputer.name;
        }

        class ListBoxData
        {
            public BitmapSource ItemIcon { get; set; }
            public string ItemName { get; set; }
            public string ItemVersion { get; set; }
            public string ItemCompany { get; set; }
            public string ItemSize { get; set; }
        }

        private void ButtonAddApplication_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog() { Filter = "Applications Files (*.exe, *.msi)|*.exe;*.msi" };
            var result = ofd.ShowDialog();
            if (result == false) return;
            File.Copy(ofd.FileName, "c:\\apps\\" + System.IO.Path.GetFileName(ofd.FileName));
            AppsToDeploy_Loaded(this, null);
        }

        private void AppsToDeploy_Loaded(object sender, RoutedEventArgs e)
        {
            string[] dir = Directory.GetFiles("c:\\apps\\");
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
                FileInfo fileInfo = new FileInfo(filePath);
                var itm = new ListBoxData();
                if (fileInfo.Extension == ".msi") {
                    itm.ItemIcon = bmpSrc;
                    itm.ItemName = Utils.GetMsiProperty(filePath,"ProductName");
                    itm.ItemVersion = Utils.GetMsiProperty(filePath,"ProductVersion");
                    itm.ItemCompany = Utils.GetMsiProperty(filePath, "Manufacturer");
                    itm.ItemSize = Utils.GetBytesReadable(fileInfo.Length);
                }
                else
                {
                    itm.ItemIcon = bmpSrc;
                    itm.ItemName = myFI.ProductName;
                    itm.ItemVersion = myFI.FileVersion;
                    itm.ItemCompany = myFI.CompanyName;
                    itm.ItemSize = Utils.GetBytesReadable(fileInfo.Length);
                }
                data.Add(itm);
            }

            dataGrid_List_Applications.ItemsSource = data;
        }

        private void label_ComputerName_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
