using MQTT_Protocol;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for Faceplate_Light.xaml
    /// </summary>
    public partial class Faceplate_Light : Window
    {
        public string LightTagName { get; set; }
        public string SwitchTagName { get; set; }
        public Faceplate_Light(string lightTag, string switchTag)
        {
            InitializeComponent();
            LightTagName = lightTag;
            SwitchTagName = switchTag;
            DataContext = this;
        }

        private void Faceplate_Loaded(object sender, RoutedEventArgs e)
        {
            string currentValue = MQTT_TagCollection.Tags[LightTagName].Value;
            if (currentValue == null)
            {
                return;
            }

            if (currentValue.Trim().ToLower() == null || currentValue.Trim().ToLower() == "" || currentValue.Trim().ToLower() == "0" || currentValue.Trim().ToLower() == "false")
            {
                Switch1.Value = false;
                TextDisplay1.Foreground = Brushes.Red;
            }
            else
            {
                Switch1.Value = true;
                TextDisplay1.Foreground = Brushes.LimeGreen;
            }
        }
    }
}
