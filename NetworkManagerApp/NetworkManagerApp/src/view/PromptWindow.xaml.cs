using System.Windows;
using System.Windows.Input;

namespace NetworkManager.View {
    /// <summary>
    /// Logique d'interaction pour Prompt.xaml
    /// </summary>
    public partial class PromptWindow : Window {
        private PromptWindowChoice choice = PromptWindowChoice.CANCEL;

        public PromptWindow() {
            InitializeComponent();

            textBox_Login.Text = System.Environment.UserName + "@" + System.Environment.UserDomainName;
        }

        public string getLogin() {
            return textBox_Login.Text;
        }

        public string getPassword() {
            return textBox_Password.Password;
        }

        public PromptWindowChoice getChoice() {
            return choice;
        }

        private void button_Connect_Click(object sender, RoutedEventArgs e) {
            choice = PromptWindowChoice.CONNECT;
            Close();
        }

        private void button_ConnectManually_Click(object sender, RoutedEventArgs e) {
            choice = PromptWindowChoice.CONNECT_MANUALLY;
            Close();
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e) {
            choice = PromptWindowChoice.CANCEL;
            Close();
        }

        void enterEvent(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return) {
                choice = PromptWindowChoice.CONNECT;
                Close();
                e.Handled = true;
            }
        }
    }

    public enum PromptWindowChoice {
        CONNECT, CONNECT_MANUALLY, CANCEL
    }
}
