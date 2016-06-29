
using NetworkManager.DomainContent;
using NetworkManager.View.Component;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace NetworkManager.View.Component {
    /// <summary>
    /// Logique d'interaction pour JobReportDetails.xaml
    /// </summary>
    public partial class JobReportDetails : UserControl {
        public JobReportDetails() {
            InitializeComponent();
        }

        private void detailsGrid_Loaded(object sender, RoutedEventArgs e) {
            detailsGrid.Columns.Clear();


            var job = new Scheduling.Job() {
                computers = new List<ComputerInfo>() {
                    new ComputerInfo { name = "PC 1" },
                    new ComputerInfo { name = "PC 2" },
                    new ComputerInfo { name = "PC 3" },
                    new ComputerInfo { name = "PC 4" },
                },
                status = Scheduling.JobStatus.TERMINATED,
                tasks = new List<Scheduling.JobTask> {
                    new Scheduling.JobTask() { type = Scheduling.JobTaskType.WAKE_ON_LAN },
                    new Scheduling.JobTask() { type = Scheduling.JobTaskType.INSTALL_SOFTWARE },
                    new Scheduling.JobTask() { type = Scheduling.JobTaskType.SHUTDOWN }
                },
                reports = new List<Scheduling.JobReport>() {
                    new Scheduling.JobReport() {
                        computerName = "PC 1",
                        error = false,
                        tasksReports = new List<Scheduling.JobTaskReport>() {
                            new Scheduling.JobTaskReport() {
                                error = false,
                                task = new Scheduling.JobTask() { type = Scheduling.JobTaskType.WAKE_ON_LAN }
                            },
                            new Scheduling.JobTaskReport() {
                                error = false,
                                task = new Scheduling.JobTask() { type = Scheduling.JobTaskType.INSTALL_SOFTWARE }
                            },
                            new Scheduling.JobTaskReport() {
                                error = false,
                                task = new Scheduling.JobTask() { type = Scheduling.JobTaskType.SHUTDOWN }
                            },
                        }
                    },
                    new Scheduling.JobReport() {
                        computerName = "PC 2",
                        error = false,
                        tasksReports = new List<Scheduling.JobTaskReport>() {
                            new Scheduling.JobTaskReport() {
                                error = false,
                                task = new Scheduling.JobTask() { type = Scheduling.JobTaskType.WAKE_ON_LAN }
                            },
                            new Scheduling.JobTaskReport() {
                                error = false,
                                task = new Scheduling.JobTask() { type = Scheduling.JobTaskType.INSTALL_SOFTWARE }
                            },
                            new Scheduling.JobTaskReport() {
                                error = false,
                                task = new Scheduling.JobTask() { type = Scheduling.JobTaskType.SHUTDOWN }
                            },
                        }
                    },
                    new Scheduling.JobReport() {
                        computerName = "PC 3",
                        error = true,
                        tasksReports = new List<Scheduling.JobTaskReport>() {
                            new Scheduling.JobTaskReport() {
                                error = false,
                                task = new Scheduling.JobTask() { type = Scheduling.JobTaskType.WAKE_ON_LAN }
                            },
                            new Scheduling.JobTaskReport() {
                                error = true,
                                task = new Scheduling.JobTask() { type = Scheduling.JobTaskType.INSTALL_SOFTWARE }
                            }
                        }
                    },
                }
            };

            // Column creation
            detailsGrid.Columns.Add(new DataGridTextColumn {
                Header = "Computer Name",
                Binding = new Binding("computerName")
            });

            int i = 0;
            foreach (var task in job.tasks) {
                detailsGrid.Columns.Add(new DataGridTextColumn {
                    Header = task.type.ToString(),
                    Binding = new Binding($"task{i}")
                });

                i++;
            }

            // Row creations
            foreach (var report in job.reports) {
                var dynamicObject = new ExpandoObject() as IDictionary<string, Object>;

                dynamicObject.Add("computerName", report.computerName);

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
                    var cell = detailsGrid.GetCell(i, j + 1);

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
