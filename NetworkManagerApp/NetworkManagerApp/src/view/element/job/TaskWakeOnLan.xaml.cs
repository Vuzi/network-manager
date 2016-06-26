using NetworkManager.Scheduling;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NetworkManager.View.Component.Job {
    /// <summary>
    /// Logique d'interaction pour TaskReboot.xaml
    /// </summary>
    public partial class TaskWakeOnLan : UserControl {

        public static string name { get; } = "Wake on lan";
        public JobSchedulerWindow mainWindow { get; set; }

        public TaskWakeOnLan() {
            InitializeComponent();
        }

        private void button_Down_Click(object sender, RoutedEventArgs e) {
            mainWindow.jobDetails.downTask(this);
        }

        private void button_Up_Click(object sender, RoutedEventArgs e) {
            mainWindow.jobDetails.upTask(this);
        }

        private void button_Delete_Click(object sender, RoutedEventArgs e) {
            mainWindow.jobDetails.deleteTask(this);
        }

        /// <summary>
        /// Create the task from the panel
        /// </summary>
        /// <returns></returns>
        public JobTask createTask() {
            // Timeout
            int timeout = 60;

            try {
                timeout = int.Parse(TextBox_timeout.Text);
            } catch (Exception) {
                MessageBox.Show("Error : Invalid timeout value", "Wak on lan task error");
                return null;
            }

            return new JobTask() {
                type = JobTaskType.WAKE_ON_LAN,
                timeout = timeout
            };
        }
    }
}
