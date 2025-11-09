using Microsoft.Web.WebView2.Core;
using MQTT_Protocol;
using MQTT_Protocol.Devices;
using MyWPFApp.LoginWindow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MyWPFApp
{
    /// <summary>
    /// Interaction logic for Window_Main.xaml
    /// </summary>
    public partial class Window_Main : Window, INotifyPropertyChanged
    {
        private static bool isTagsInitialized = false;

        private string _UserName;
        public string UserName
        {
            get => _UserName;
            set
            {
                _UserName = value;
                OnPropertyChanged(nameof(UserName));
                SetUserDataForControls(this);

            }
        }

        private int _userPrivilege;
        public int UserPrivilege
        {
            get => _userPrivilege;
            set
            {
                _userPrivilege = value;
                OnPropertyChanged(nameof(UserPrivilege));
                SetUserDataForControls(this);

            }
        }

        private string _Role;
        public string Role
        {
            get { return _Role; }
            set { _Role = value; }
        }

        private void InitializeTags()
        {
            Driver_Tool.Manager.Building_Manager.XmlPath = @"D:\Visual_Studio\Visual_Studio\Projects\DEMO\DotNETFramework\WPF\CS\MyWPFApp\TagCollection.xml";
            List<Building> buildings = Driver_Tool.Manager.Building_Manager.GetBuildings();
            MQTT_Service.InitializeService(buildings);

            MQTT_Service.eventCatchExceptionDelegate += new CatchExceptionDelegate((ex) =>
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            });

            MQTT_Service.Start();
        }

        private DateTime _currentDateTime;
        private DispatcherTimer timer;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public DateTime CurrentDateTime
        {
            get => _currentDateTime;
            set
            {
                _currentDateTime = value;
                OnPropertyChanged(nameof(CurrentDateTime));
            }
        }

        public Window_Main()
        {
            if (!isTagsInitialized)
            {
                InitializeTags();
                isTagsInitialized = true;
            }

            InitializeComponent();

            DataContext = this;
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            CurrentDateTime = DateTime.Now;
            timer.Tick += (s, e) => CurrentDateTime = DateTime.Now;
            timer.Start();
        }

        private void Window_Main_Loaded(object sender, RoutedEventArgs e)
        {
            // Now, UserName and Role will have correct values
            txt_UserInfo.Text = UserName + " (" + Role + ")";
            SetUserDataForControls(this);
        }

        private void SetUserDataForControls(DependencyObject parent)
        {
            if (parent == null) return;

            foreach (var child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is DependencyObject depChild)
                {
                    // Apply to UserControls
                    if (depChild is UserControl uc)
                    {
                        var userNameProp = uc.GetType().GetProperty("UserName", BindingFlags.Public | BindingFlags.Instance);
                        userNameProp?.SetValue(uc, UserName);

                        var userPrivilegeProp = uc.GetType().GetProperty("UserPrivilege", BindingFlags.Public | BindingFlags.Instance);
                        userPrivilegeProp?.SetValue(uc, UserPrivilege);
                    }

                    // Recurse into deeper logical children
                    SetUserDataForControls(depChild);
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to log out?", "Log out", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true; // Cancel the close event
                return;
            }

            // Reopen login window if user confirmed logout
            Window_Login loginWindow = new Window_Login();
            loginWindow.Show();
        }
    }
}
