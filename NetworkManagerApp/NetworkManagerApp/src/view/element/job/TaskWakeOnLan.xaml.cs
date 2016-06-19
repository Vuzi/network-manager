using System.Windows.Controls;

namespace NetworkManager.View.Component.Job {
    /// <summary>
    /// Logique d'interaction pour TaskReboot.xaml
    /// </summary>
    public partial class TaskWakeOnLan : UserControl {

        public static string name { get; } = "Wake on lan";

        public TaskWakeOnLan() {
            InitializeComponent();
        }


    }
}
