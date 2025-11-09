using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Excel = Microsoft.Office.Interop.Excel;

namespace HMI_Report
{
    /// <summary>
    /// Interaction logic for Report_View.xaml
    /// </summary>
    public partial class Report_View : UserControl
    {
        private string connectionString = @"Data Source=DESKTOP\SQLEXPRESS;Initial Catalog=LVTN;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        public static readonly DependencyProperty PrivilegeProperty =
            DependencyProperty.Register(nameof(Privilege), typeof(int), typeof(Report_View), new PropertyMetadata(0, OnPrivilegeChanged));

        public int Privilege
        {
            get => (int)GetValue(PrivilegeProperty);
            set => SetValue(PrivilegeProperty, value);
        }

        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register(nameof(UserName), typeof(string), typeof(Report_View), new PropertyMetadata(string.Empty, OnUserNameChanged));

        public string UserName
        {
            get => (string)GetValue(UserNameProperty);
            set => SetValue(UserNameProperty, value);
        }
        private static void OnUserNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Report_View control && e.NewValue is string newUser)
            {
                // When AcknowledgedBy changes, also update UserName internally
                control.UserName = newUser;
            }
        }

        public static readonly DependencyProperty UserPrivilegeProperty =
            DependencyProperty.Register(nameof(UserPrivilege), typeof(int), typeof(Report_View), new PropertyMetadata(0, OnPrivilegeChanged));

        public int UserPrivilege
        {
            get => (int)GetValue(UserPrivilegeProperty);
            set => SetValue(UserPrivilegeProperty, value);
        }

        private static void OnPrivilegeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Report_View control)
                control.CheckPrivilege();
        }

        private void CheckPrivilege()
        {
            IsEnabled = UserPrivilege >= Privilege;
        }

        public Report_View()
        {
            InitializeComponent();

            string[] items = { "Alarm_FLOOR1", "LoginDiary", "User" };
            foreach (var item in items)
                comboBoxReports.Items.Add(item);
        }


        private void comboBoxReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxReports.SelectedItem?.ToString() == "User")
            {
                pick_StartDate.IsEnabled = false;
                pick_EndDate.IsEnabled = false;
            }
            else
            {
                pick_StartDate.IsEnabled = true;
                pick_EndDate.IsEnabled = true;
            }
        }

        private void btn_View_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxReports.SelectedItem == null)
            {
                MessageBox.Show("Please select a report from the list.", "No Report Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string reportName = comboBoxReports.SelectedItem.ToString();
            DateTime startDate = pick_StartDate.SelectedDate ?? DateTime.Now;
            DateTime endDate = pick_EndDate.SelectedDate ?? DateTime.Now;

            DataTable reportData = GetReportData(reportName, startDate, endDate);
            DisplayReport(reportData);
        }

        private DataTable GetReportData(string reportName, DateTime startDate, DateTime endDate)
        {
            DataTable reportData = new DataTable();

            string query = reportName == "User"
                ? $"SELECT * FROM [User]"
                : $"SELECT * FROM {reportName} WHERE CONVERT(DATE, DateTime) >= @startDate AND CONVERT(DATE, DateTime) <= @endDate";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (reportName != "User")
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate.Date);
                        cmd.Parameters.AddWithValue("@endDate", endDate.Date);
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(reportData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return reportData;
        }

        private void DisplayReport(DataTable data)
        {
            dataGrid.ItemsSource = data.DefaultView;
        }

        private void btn_Export_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxReports.SelectedItem == null)
            {
                MessageBox.Show("Please select a report from the list.", "No Report Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string reportName = comboBoxReports.SelectedItem.ToString();
            DateTime startDate = pick_StartDate.SelectedDate ?? DateTime.Now;
            DateTime endDate = pick_EndDate.SelectedDate ?? DateTime.Now;

            ExportDataToExcel(reportName, startDate, endDate);
        }

        private void ExportDataToExcel(string reportName, DateTime startDate, DateTime endDate)
        {
            DataTable reportData = GetReportData(reportName, startDate, endDate);

            if (reportData.Rows.Count > 0)
            {
                Excel.Application excelApp = new Excel.Application();
                Excel.Workbook workbook = excelApp.Workbooks.Add(Type.Missing);
                Excel.Worksheet worksheet = workbook.Sheets[1];
                worksheet = workbook.ActiveSheet;
                worksheet.Name = "Report";

                for (int i = 1; i < reportData.Columns.Count + 1; i++)
                {
                    worksheet.Cells[1, i] = reportData.Columns[i - 1].ColumnName;
                }

                for (int i = 0; i < reportData.Rows.Count; i++)
                {
                    for (int j = 0; j < reportData.Columns.Count; j++)
                    {
                        worksheet.Cells[i + 2, j + 1] = reportData.Rows[i][j].ToString();
                    }
                }

                worksheet.Columns.AutoFit();
                excelApp.Visible = true;

                Marshal.ReleaseComObject(worksheet);
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(excelApp);
            }
            else
            {
                MessageBox.Show("No data available for the selected report and date range.", "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btn_Exit_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.ItemsSource = null;
        }
    }
}
