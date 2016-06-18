
using NetworkManager.WMIExecution;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NetworkManager.View {
    /// <summary>
    /// Logique d'interaction pour ErrorHandler.xaml
    /// </summary>
    public partial class ErrorHandlerWindow : Window {

        public Button warningIndicator { get; set; }

        public ErrorHandlerWindow() {
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
            string computer = "-";
            int severity = 1;

            if(e is WMIException) {
                WMIException we = (WMIException)e;
                computer = we.computer;
                severity = 0;
                e = we.error != null ? we.error : e;
            }

            dataGrid_Errors.Items.Add(new Error() {
                type = e.GetType().ToString(),
                message = e.Message,
                date = DateTime.Now,
                computer = computer,
                source = e.Source,
                severity = severity
            });

            warningIndicator.Visibility = Visibility.Visible;
        }
    }

    class Error {
        public string type { get; set; }
        public string message { get; set; }
        public string source { get; set; }
        public DateTime date { get; set; }
        public string computer { get; set; }
        public int severity { get; internal set; }
    }
}
