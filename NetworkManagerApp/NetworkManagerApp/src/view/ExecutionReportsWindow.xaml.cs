
using NetworkManager.WMIExecution;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace NetworkManager.View {
    /// <summary>
    /// Logique d'interaction pour ExecutionReportsWindow.xaml
    /// </summary>
    public partial class ExecutionReportsWindow : Window {

        public List<WMIExecutionResult> reports { get; set; }

        public ExecutionReportsWindow(List<WMIExecutionResult> reports) {
            InitializeComponent();

            this.reports = reports;

            foreach (var executionResult in reports)
                dataGrid_Reports.Items.Add(executionResult);
        }

        private void dataGrid_Reports_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (dataGrid_Reports.SelectedItem == null)
                return;

            // Show report
            var reporter = new ExecutionReport(dataGrid_Reports.SelectedItem as WMIExecutionResult);
            reporter.Left = this.Left + 50;
            reporter.Top = this.Top + 50;
            reporter.Show();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (dataGrid_Reports.SelectedItem == null)
                return;

            // Show report
            var reporter = new ExecutionReport(dataGrid_Reports.SelectedItem as WMIExecutionResult);
            reporter.Left = this.Left + 50;
            reporter.Top = this.Top + 50;
            reporter.Show();
        }
    }
}
