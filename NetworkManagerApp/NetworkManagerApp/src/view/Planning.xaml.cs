
using System.Windows;

namespace NetworkManager.View {
    /// <summary>
    /// Logique d'interaction pour Planning.xaml
    /// </summary>
    public partial class Planning : Window {
        public Planning() {
            InitializeComponent();
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e) {

        }

        private void buttonSelectAll_Copy_Click(object sender, RoutedEventArgs e) {

        }

        private void buttonAddTask_Click(object sender, RoutedEventArgs e) {
            var selectTask = new SelectTask();
            selectTask.mainWindow = this;
            selectTask.Show();
        }
    }
}
