using HMI_Alarm.Manager;
using MQTT_Protocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace HMI_Alarm
{
    /// <summary>
    /// Interaction logic for Alarm_View.xaml
    /// </summary>
    public partial class Alarm_View : UserControl
    {
        private readonly DispatcherTimer timer;
        private readonly SqlConnection connection;
        private bool allAcknowledged;
        public ObservableCollection<Alarm> Alarms { get; set; } = new ObservableCollection<Alarm>();
        private readonly string connectionString = @"Data Source=DESKTOP\SQLEXPRESS;Initial Catalog=LVTN;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        // Analog xml paths
        public static readonly DependencyProperty AnalogXmlPathProperty =
            DependencyProperty.Register(nameof(AnalogXmlPath), typeof(string), typeof(Alarm_View), new PropertyMetadata(null, OnAnalogXmlPathChanged));

        public string AnalogXmlPath
        {
            get => (string)GetValue(AnalogXmlPathProperty);
            set => SetValue(AnalogXmlPathProperty, value);
        }

        private static void OnAnalogXmlPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string path && !string.IsNullOrWhiteSpace(path))
            {
                AnalogDevice_Manager.XmlPath = path;
            }
        }

        // Digital xml paths
        public static readonly DependencyProperty DigitalXmlPathProperty =
            DependencyProperty.Register(nameof(DigitalXmlPath), typeof(string), typeof(Alarm_View), new PropertyMetadata(null, OnDigitalXmlPathChanged));

        public string DigitalXmlPath
        {
            get => (string)GetValue(DigitalXmlPathProperty);
            set => SetValue(DigitalXmlPathProperty, value);
        }

        private static void OnDigitalXmlPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string path && !string.IsNullOrWhiteSpace(path))
            {
                DigitalDevice_Manager.XmlPath = path;
            }
        }

        public static readonly DependencyProperty DeviceNameProperty =
            DependencyProperty.Register(nameof(DeviceName), typeof(string), typeof(Alarm_View), new PropertyMetadata(string.Empty));

        public string DeviceName
        {
            get => (string)GetValue(DeviceNameProperty);
            set => SetValue(DeviceNameProperty, value);
        }

        public static readonly DependencyProperty PrivilegeProperty =
            DependencyProperty.Register(nameof(Privilege), typeof(int), typeof(Alarm_View), new PropertyMetadata(0, OnPrivilegeChanged));

        public int Privilege
        {
            get => (int)GetValue(PrivilegeProperty);
            set => SetValue(PrivilegeProperty, value);
        }

        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register(nameof(UserName), typeof(string), typeof(Alarm_View), new PropertyMetadata(string.Empty, OnUserNameChanged));

        public string UserName
        {
            get => (string)GetValue(UserNameProperty);
            set => SetValue(UserNameProperty, value);
        }
        private static void OnUserNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Alarm_View control && e.NewValue is string newUser)
            {
                // When AcknowledgedBy changes, also update UserName internally
                control.UserName = newUser;
            }
        }

        public static readonly DependencyProperty UserPrivilegeProperty =
            DependencyProperty.Register(nameof(UserPrivilege), typeof(int), typeof(Alarm_View), new PropertyMetadata(0, OnPrivilegeChanged));

        public int UserPrivilege
        {
            get => (int)GetValue(UserPrivilegeProperty);
            set => SetValue(UserPrivilegeProperty, value);
        }

        private static void OnPrivilegeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Alarm_View control)
                control.CheckPrivilege();
        }

        private void CheckPrivilege()
        {
            IsEnabled = UserPrivilege >= Privilege;
        }

        public Alarm_View()
        {
            InitializeComponent();
            DataContext = this;

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;

            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error establishing database connection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!Alarm_SettingsMode.TimerTicking) return;
            Check_Analog_Alarms();
            Check_Digital_Alarms();
        }

        private void Check_Analog_Alarms()
        {
            try
            {
                if (string.IsNullOrEmpty(AnalogXmlPath) || string.IsNullOrWhiteSpace(AnalogXmlPath)) return;
                InitializeAnalogData(AnalogXmlPath);
                AnalogDevice_Manager.XmlPath = AnalogXmlPath;
                AnalogDevice_Manager.GetDeviceAnalogs();

                Device_Analog adevice = AnalogDevice_Manager.GetByDeviceAnalogName(DeviceName);
                List<Alarm_Analog> aalarmList = adevice.AlarmAnalogs;

                foreach (Alarm_Analog tg in aalarmList)
                {
                    string[] row = {
                        string.Format("{0}", tg.AlarmId),
                        tg.AlarmName,
                        tg.Source,
                        string.Format("{0}", tg.HighHigh),
                        string.Format("{0}", tg.High),
                        string.Format("{0}", tg.Low),
                        string.Format("{0}", tg.LowLow)};

                    if (tg.Source == MQTT_TagCollection.Tags[tg.Source].Topic)
                    {
                        //string alarmName = MQTT_TagCollection.Tags[tg.Source].TagName;
                        double currentValue = Convert.ToDouble(MQTT_TagCollection.Tags[tg.Source].Value);
                        //Console.WriteLine(currentValue.ToString());

                        if (currentValue >= tg.HighHigh)
                        {
                            LogAlarm(tg.AlarmId, tg.AlarmName, tg.Source, "HIGH HIGH");
                        }
                        if (currentValue >= tg.High & currentValue < tg.HighHigh)
                        {
                            LogAlarm(tg.AlarmId, tg.AlarmName, tg.Source, "HIGH");
                        }
                        if (currentValue >= tg.Low & currentValue < tg.High)
                        {
                            LogAlarm(tg.AlarmId, tg.AlarmName, tg.Source, "");
                        }
                        if (currentValue >= tg.LowLow & currentValue < tg.Low)
                        {
                            LogAlarm(tg.AlarmId, tg.AlarmName, tg.Source, "LOW");
                        }
                        if (0 < currentValue & currentValue < tg.LowLow)
                        {
                            LogAlarm(tg.AlarmId, tg.AlarmName, tg.Source, "LOW LOW");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("An error occurred while checking analog alarm: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw ex;
            }
        }

        private void Check_Digital_Alarms()
        {
            try
            {
                if (string.IsNullOrEmpty(DigitalXmlPath) || string.IsNullOrWhiteSpace(DigitalXmlPath))
                    return;

                InitializeDigitalData(DigitalXmlPath);
                DigitalDevice_Manager.GetDeviceDigitals();

                Device_Digital ddevice = DigitalDevice_Manager.GetByDeviceDigitalName(DeviceName);
                List<Alarm_Digital> dalarmList = ddevice.AlarmDigitals;

                foreach (Alarm_Digital tg in dalarmList)
                {
                    string[] row = {
                        string.Format("{0}", tg.AlarmId),
                        tg.AlarmName,
                        tg.Source};

                    if (tg.Source == MQTT_TagCollection.Tags[tg.Source].Topic)
                    {
                        //string alarmName = MQTT_TagCollection.Tags[tg.Source].TagName;
                        string currentValue = MQTT_TagCollection.Tags[tg.Source].Value;
                        if (string.IsNullOrEmpty(currentValue.Trim()))
                            continue;

                        if (currentValue == "1" || currentValue == "true" || currentValue == "TRUE" || currentValue == "True")
                        {
                            LogAlarm(tg.AlarmId, tg.AlarmName, tg.Source, "ON");
                        }
                        else
                        {
                            LogAlarm(tg.AlarmId, tg.AlarmName, tg.Source, "OFF");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("An error occurred while checking digital alarm: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw ex;
            }
        }

        private void InitializeAnalogData(string xmlPath)
        {
            AnalogDevice_Manager.DeviceAnalogs.Clear();
            AnalogDevice_Manager.XmlPath = xmlPath;
        }

        private void InitializeDigitalData(string xmlPath)
        {
            DigitalDevice_Manager.DeviceDigitals.Clear();
            DigitalDevice_Manager.XmlPath = xmlPath;
        }

        private void LogAlarm(int alarmID, string alarmName, string source, string state)
        {
            Alarm existingAlarm = Alarms.FirstOrDefault(a => a.Source == source);

            if (existingAlarm != null)            //Alarm is existing
            {
                if (existingAlarm.State != state) // Status has changed
                {
                    if (state != "HIGH HIGH" && state != "LOW LOW")
                    {
                        //player.Stop();
                    }

                    existingAlarm.ID = alarmID;
                    existingAlarm.AlarmName = alarmName;
                    existingAlarm.State = state;
                    existingAlarm.DateTime = DateTime.Now; // Update DateTime
                    existingAlarm.Acknowledged = "NO";
                    existingAlarm.AcknowledgedBy = "";

                    allAcknowledged = false;

                    InsertAlarmToDatabase(existingAlarm);
                }

                if (existingAlarm.State == "HIGH HIGH" || existingAlarm.State == "LOW LOW")
                {
                    if (!allAcknowledged)
                    {
                        Thread alarmThread = new Thread(() =>
                        {
                            Console.Beep(5000, 600);
                            Thread.Sleep(200);
                        });

                        alarmThread.IsBackground = true;
                        alarmThread.Start();
                    }
                    else
                    {
                        //player.Stop();
                    }
                }
                AlarmDataGrid.Items.Refresh();
            }
            else
            {
                Alarm alarm = new Alarm
                {
                    ID = alarmID,
                    AlarmName = alarmName,
                    Source = source,
                    State = state,
                    DateTime = DateTime.Now,
                };
                Alarms.Add(alarm);

                InsertAlarmToDatabase(alarm); // Insert new alarm to database
            }
        }

        private void btn_Acknowledge_Click(object sender, RoutedEventArgs e)
        {
            if (AlarmDataGrid.SelectedItem is Alarm selectedAlarm)
            {
                selectedAlarm.DateTime = DateTime.Now;
                selectedAlarm.Acknowledged = "YES";
                selectedAlarm.AcknowledgedBy = UserName;

                InsertAlarmToDatabase(selectedAlarm);
                AlarmDataGrid.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Please select an alarm to acknowledge.");
            }

            AllAcknowledged_StopSound();
        }

        private void AllAcknowledged_StopSound()
        {
            allAcknowledged = Alarms
                .Where(a => a.State == "HIGH HIGH" || a.State == "LOW LOW")
                .All(a => !string.IsNullOrEmpty(a.AcknowledgedBy));
        }

        private void InsertAlarmToDatabase(Alarm alarm)
        {
            try
            {
                string query = $"INSERT INTO Alarm_{DeviceName} (ID, AlarmName, Source, State, DateTime, AcknowledgedBy) " +
                               "VALUES (@ID, @AlarmName, @Source, @State, @DateTime, @AcknowledgedBy)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", alarm.ID);
                    command.Parameters.AddWithValue("@AlarmName", alarm.AlarmName);
                    command.Parameters.AddWithValue("@Source", alarm.Source);
                    command.Parameters.AddWithValue("@State", alarm.State);
                    command.Parameters.AddWithValue("@DateTime", alarm.DateTime);
                    command.Parameters.AddWithValue("@AcknowledgedBy", (object)alarm.AcknowledgedBy ?? DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inserting alarm data to database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
