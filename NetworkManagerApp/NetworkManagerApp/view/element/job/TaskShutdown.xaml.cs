using NetworkManager.Job;
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

        private void button_Down_Click(object sender, RoutedEventArgs e) {
            mainWindow.downTask(this);
        }

        private void button_Up_Click(object sender, RoutedEventArgs e) {
            mainWindow.upTask(this);
        }

        private void button_Delete_Click(object sender, RoutedEventArgs e) {
            mainWindow.deleteTask(this);
        }

        /// <summary>
        /// Create the task from the panel
        /// </summary>
        /// <returns></returns>
        public JobTask createTask() {
            return new JobTask() {
                type = JobTaskType.SHUTDOWN
            };
        }
    }
}
