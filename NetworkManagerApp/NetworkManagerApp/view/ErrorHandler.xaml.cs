
using NetworkManager.WMIExecution;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NetworkManagerApp.view {
    /// <summary>
    /// Logique d'interaction pour ErrorHandler.xaml
    /// </summary>
    public partial class ErrorHandler : Window {

        public Button warningIndicator { get; set; }

        public ErrorHandler() {
            InitializeComponent();
        }

        private void button_Clear_Click(object sender, RoutedEventArgs e) {
            dataGrid_Errors.Items.Clear();
            warningIndicator.Visibility = Visibility.Hidden;
        }

        private void button_Close_Click(object sender, RoutedEventArgs e) {
            this.Hide();
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);
            e.Cancel = true;
            this.Hide();
        }

        public void addError(Exception e) {
            if(e is WMIException) {
                WMIException we = (WMIException)e;
                e = we.error != null ? we.error : e;
            }

            dataGrid_Errors.Items.Add(new Error() {
                type = e.GetType().ToString(),
                message = e.Message,
                date = DateTime.Now,
                source = e.Source
            });

            warningIndicator.Visibility = Visibility.Visible;
        }
    }

    class Error {
        public string type { get; set; }
        public string message { get; set; }
        public string source { get; set; }
        public DateTime date { get; set; }
    }
}
