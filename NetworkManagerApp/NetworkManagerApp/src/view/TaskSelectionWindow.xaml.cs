using NetworkManager.View.Component;
using NetworkManager.View.Component.Job;
using System;
using System.Windows;

namespace NetworkManager.View {
    /// <summary>
    /// Logique d'interaction pour SelectTask.xaml
    /// </summary>
    public partial class TaskSelectionWindow : Window {
        public TaskSelectionWindow() {
            InitializeComponent();
        }

        public JobDetails mainWindow { get; set; }

        private void cancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void select_Click(object sender, RoutedEventArgs e) {
            ComboBoxItem item = taskList.SelectedItem as ComboBoxItem;

            if(item != null) {
                dynamic element = Activator.CreateInstance(item.val as Type);
                element.mainWindow = mainWindow;
                mainWindow.tasksPanel.Children.Add(element);
            }
        }

        private void taskList_Loaded(object sender, RoutedEventArgs e) {
            taskList.Items.Clear();
            taskList.Items.Add(new ComboBoxItem() {
                name = TaskInstall.name,
                val = typeof(TaskInstall)
            });
            taskList.Items.Add(new ComboBoxItem() {
                name = TaskWakeOnLan.name,
                val = typeof(TaskWakeOnLan)
            });
            taskList.Items.Add(new ComboBoxItem() {
                name = TaskShutdown.name,
                val = typeof(TaskShutdown)
            });
            taskList.Items.Add(new ComboBoxItem() {
                name = TaskReboot.name,
                val = typeof(TaskReboot)
            });
        }
    }

    public class ComboBoxItem {
        public string name { get; set; }
        public object val { get; set; }
    }
}
