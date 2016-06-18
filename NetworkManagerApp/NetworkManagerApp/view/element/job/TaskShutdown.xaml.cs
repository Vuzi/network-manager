using System.Windows;
using System.Windows.Controls;

namespace NetworkManager.View.Component.Job {
    /// <summary>
    /// Logique d'interaction pour TaskReboot.xaml
    /// </summary>
    public partial class TaskShutdown : UserControl {

        public static string name { get; } = "Shutdown";
        public JobSchedulerWindow mainWindow { get; set; }

        public TaskShutdown() {
            InitializeComponent();
        }

        private void buttonDown_Click(object sender, RoutedEventArgs e) {
            mainWindow.downTask(this);
        }

        private void buttonUp_Click(object sender, RoutedEventArgs e) {
            mainWindow.upTask(this);
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e) {
            mainWindow.deleteTask(this);
        }
    }
}
