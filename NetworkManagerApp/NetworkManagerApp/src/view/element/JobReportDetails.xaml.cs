
using NetworkManager.DomainContent;
using NetworkManager.View.Component;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using NetworkManager.Scheduling;

namespace NetworkManager.View.Component {
    /// <summary>
    /// Logique d'interaction pour JobReportDetails.xaml
    /// </summary>
    public partial class JobReportDetails : UserControl {

        public JobSchedulerWindow parent { get; set; }

        private Scheduling.Job job;

        public JobReportDetails() {
            InitializeComponent();
        }

        Dictionary<JobTaskType, string> headers = new Dictionary<JobTaskType, string> {
            { JobTaskType.INSTALL_SOFTWARE, "Software installation" },
            { JobTaskType.REBOOT, "Reboot" },
            { JobTaskType.SHUTDOWN, "Shutdown" },
            { JobTaskType.WAKE_ON_LAN, "Wake On Lan" },
        };

        List<Tuple<float, string>> messages = new List<Tuple<float, string>> {
            Tuple.Create(99f, "successful"),
            Tuple.Create(74f, "mostly successful"),
            Tuple.Create(49f, "mostly failed"),
            Tuple.Create(24f, "failed"),
            Tuple.Create(0f , "total failure")
        };

        public void setJob(Scheduling.Job job) {
            this.job = job;

            textBox_CreationDate.Text = job.creationDateTime.ToString("f",
                CultureInfo.CreateSpecificCulture("fr-FR"));

            textBox_LastExecutionDate.Text = job.lastExecutionDateTime != DateTime.MinValue ? job.lastExecutionDateTime.ToString("f",
                CultureInfo.CreateSpecificCulture("fr-FR")) : "Never";
            
            textBox_StartTrigger.Text = job.triggerDescription;

            float success = job.reports.Aggregate(0, (sum, jr) => jr.error ? sum : sum + 1) / (float)job.reports.Count * 100f;
            string message = null;

            foreach(var msg in messages) {
                if(success >= msg.Item1) {
                    message = msg.Item2;
                    break;
                }
            }

            if (success < 0 || float.IsNaN(success))
                textBox_SuccessRate.Content = "-";
            else
                textBox_SuccessRate.Content = $"{message} ({(int)success}%)";

            // Grid clear
            detailsGrid.Items.Clear();
            detailsGrid.Columns.Clear();

            // Column creation
            detailsGrid.Columns.Add(new DataGridTextColumn {
                Header = "Computer Name",
                Binding = new Binding("computerName")
            });
            detailsGrid.Columns.Add(new DataGridTextColumn {
                Header = "Start",
                Binding = new Binding("startDateTime")
            });
            detailsGrid.Columns.Add(new DataGridTextColumn {
                Header = "End",
                Binding = new Binding("endDateTime")
            });
            detailsGrid.Columns.Add(new DataGridTextColumn {
                Header = "Duration",
                Binding = new Binding("duration")
            });

            int i = 0;
            foreach (var task in job.tasks) {
                detailsGrid.Columns.Add(new DataGridTextColumn {
                    Header = headers.GetValueOrDefault(task.type),
                    Binding = new Binding($"task{i}")
                });

                i++;
            }

            // Row creations
            foreach (var report in job.reports) {
                var dynamicObject = new ExpandoObject() as IDictionary<string, object>;

                dynamicObject.Add("computerName", report.computerName);
                dynamicObject.Add("startDateTime", report.startDateTime.ToString("dd/MM/yyyy HH:mm:ss"));
                dynamicObject.Add("endDateTime", report.endDateTime.ToString("dd/MM/yyyy HH:mm:ss"));
                
                var diff = (report.endDateTime - report.startDateTime);
                dynamicObject.Add("duration", (int)diff.TotalMinutes + " minutes " + (int)(diff.TotalSeconds % 60) + " seconds");

                for (i = 0; i < job.tasks.Count; i++) {
                    if (report.tasksReports.Count > i)
                        dynamicObject.Add($"task{i}", report.tasksReports[i].error ? "Failed" : "Success");
                    else
                        dynamicObject.Add($"task{i}", "-");
                }

                detailsGrid.Items.Add(dynamicObject);
            }

            // Row styling
            for (i = 0; i < job.reports.Count; i++) {
                for (int j = 0; j < job.reports[i].tasksReports.Count; j++) {
                    var cell = detailsGrid.GetCell(i, j + 4);

                    switch ((cell.Content as TextBlock).Text) {
                        case "-":
                            cell.Background = Brushes.LightYellow;
                            break;
                        case "Failed":
                            cell.Background = Brushes.Red;
                            break;
                        case "Success":
                            cell.Background = Brushes.LightGreen;
                            break;
                    }
                }
            }
        }

        private void buttonShowJob_Click(object sender, RoutedEventArgs e) {
            parent.jobDetails.Visibility = Visibility.Visible;
            parent.jobReportDetails.Visibility = Visibility.Collapsed;
        }

        private void button_JobReload_Click(object sender, RoutedEventArgs e) {
            if (job != null)
                setJob(App.jobStore.getJobById(job.id));
        }
    }

    public static class DataGridExtension {

        public static DataGridRow GetRow(this DataGrid grid, int index) {
            DataGridRow row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null) {
                // May be virtualized, bring into view and try again.
                grid.UpdateLayout();
                grid.ScrollIntoView(grid.Items[index]);
                row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++) {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null) {
                    child = GetVisualChild<T>(v);
                }
                if (child != null) {
                    break;
                }
            }
            return child;
        }

        public static DataGridCell GetCell(this DataGrid grid, DataGridRow row, int column) {
            if (row != null) {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(row);

                if (presenter == null) {
                    grid.ScrollIntoView(row, grid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(row);
                }

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                return cell;
            }
            return null;
        }

        public static DataGridCell GetCell(this DataGrid grid, int row, int column) {
            DataGridRow rowContainer = GetRow(grid, row);
            return grid.GetCell(rowContainer, column);
        }
    }

    public class ReportDetailModel {
        public string computerName { get; set; }
        public List<string> tasksNames { get; set; }
    }
}
