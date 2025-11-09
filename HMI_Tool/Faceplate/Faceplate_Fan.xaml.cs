using MQTT_Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HMI_Tool.Faceplate
{
    /// <summary>
    /// Interaction logic for Faceplate_Fan.xaml
    /// </summary>
    public partial class Faceplate_Fan : Window
    {
        public string FanTagName { get; set; }
        public string SwitchTagName { get; set; }
        public string PowerTagName { get; set; }
        public string SpeedTagName { get; set; }

        public Faceplate_Fan(string fanTag, string switchTag, string powerTag, string speedTag)
        {
            InitializeComponent();
            FanTagName = fanTag;
            SwitchTagName = switchTag;
            PowerTagName = powerTag;
            SpeedTagName = speedTag;

            DataContext = this;
        }

        private void Faceplate_Loaded(object sender, RoutedEventArgs e)
        {
            string currentValue = MQTT_TagCollection.Tags[FanTagName].Value;
            if (currentValue == null)
            {
                return;
            }

            if (currentValue.Trim().ToLower() == "" || currentValue.Trim().ToLower() == "0" || currentValue.Trim().ToLower() == "false")
            {
                Switch1.Value = false;
                TextDisplay1.Foreground = Brushes.Red;
            }
            else
            {
                Switch1.Value = true;
                TextDisplay1.Foreground = Brushes.LimeGreen;
            }
            TextEditor1.TextBox.Text = MQTT_TagCollection.Tags[PowerTagName].Value;
        }
    }
}
