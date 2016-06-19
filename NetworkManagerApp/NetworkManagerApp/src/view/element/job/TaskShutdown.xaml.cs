using System.Windows.Controls;

namespace NetworkManager.View.Component.Job {
    /// <summary>
    /// Logique d'interaction pour TaskReboot.xaml
    /// </summary>
    public partial class TaskShutdown : UserControl {

        public static string name { get; } = "Shutdown";

        public TaskShutdown() {
            InitializeComponent();
        }


    }
}
