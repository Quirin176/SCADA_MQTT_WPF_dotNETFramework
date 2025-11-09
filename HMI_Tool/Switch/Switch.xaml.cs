using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MQTT_Protocol;
using MQTT_Protocol.Devices;

namespace HMI_Tool.Switch
{
    /// <summary>
    /// Interaction logic for Switch.xaml
    /// </summary>
    public partial class Switch : UserControl
    {
        public Switch()
        {
            InitializeComponent();
            UpdateVisualState(false);
        }

        #region Dependency Properties
        public static readonly DependencyProperty TagNameProperty =
            DependencyProperty.Register(nameof(TagName), typeof(string), typeof(Switch), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(bool), typeof(Switch), new PropertyMetadata(false, OnValueChanged));

        public static readonly DependencyProperty OnBackColorProperty =
            DependencyProperty.Register(nameof(OnBackColor), typeof(Brush), typeof(Switch), new PropertyMetadata(Brushes.LimeGreen));

        public static readonly DependencyProperty OffBackColorProperty =
            DependencyProperty.Register(nameof(OffBackColor), typeof(Brush), typeof(Switch), new PropertyMetadata(Brushes.LightCoral));

        public string TagName
        {
            get { return (string)GetValue(TagNameProperty); }
            set { SetValue(TagNameProperty, value); }
        }

        public bool Value
        {
            get { return (bool)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public Brush OnBackColor
        {
            get { return (Brush)GetValue(OnBackColorProperty); }
            set { SetValue(OnBackColorProperty, value); }
        }

        public Brush OffBackColor
        {
            get { return (Brush)GetValue(OffBackColorProperty); }
            set { SetValue(OffBackColorProperty, value); }
        }
        #endregion

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggle = (Switch)d;
            toggle.UpdateVisualState(true);
        }

        private void Switch_Click(object sender, RoutedEventArgs e)
        {
            Value = !Value;
            if (Value)
            {
                MQTT_Service.PublishToTopic(TagName, 1);
            }
            else
            {
                MQTT_Service.PublishToTopic(TagName, 0);
            }
        }

        private void UpdateVisualState(bool animate)
        {
            SwitchBackground.Background = Value ? OnBackColor : OffBackColor;

            double targetX = Value ? (this.ActualWidth - 40) : 3;

            if (animate)
            {
                var animation = new DoubleAnimation(targetX, TimeSpan.FromMilliseconds(200));
                SwitchTranslateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
            }
            else
            {
                SwitchTranslateTransform.X = targetX;
            }
        }
    }
}
