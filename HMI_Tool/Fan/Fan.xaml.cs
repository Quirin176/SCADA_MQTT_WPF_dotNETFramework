using HMI_Tool.Faceplate;
using MQTT_Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HMI_Tool.Fan
{
    /// <summary>
    /// Interaction logic for Fan.xaml
    /// </summary>
    public partial class Fan : UserControl
    {
        #region Properties

        public static readonly DependencyProperty TagNameProperty =
            DependencyProperty.Register(nameof(TagName), typeof(string), typeof(Fan), new PropertyMetadata(string.Empty, OnTagNameChanged));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(Fan), new PropertyMetadata("false", OnValueChanged));

        public static readonly DependencyProperty SwitchTagNameProperty =
            DependencyProperty.Register(nameof(SwitchTagName), typeof(string), typeof(Fan), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PowerTagNameProperty =
            DependencyProperty.Register(nameof(PowerTagName), typeof(string), typeof(Fan), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SpeedTagNameProperty =
            DependencyProperty.Register(nameof(SpeedTagName), typeof(string), typeof(Fan), new PropertyMetadata(string.Empty));


        public string TagName
        {
            get => (string)GetValue(TagNameProperty);
            set => SetValue(TagNameProperty, value);
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string SwitchTagName
        {
            get => (string)GetValue(SwitchTagNameProperty);
            set => SetValue(SwitchTagNameProperty, value);
        }

        public string PowerTagName
        {
            get => (string)GetValue(PowerTagNameProperty);
            set => SetValue(PowerTagNameProperty, value);
        }

        public string SpeedTagName
        {
            get => (string)GetValue(SpeedTagNameProperty);
            set => SetValue(SpeedTagNameProperty, value);
        }

        #endregion

        private static void OnTagNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Fan control)
            {
                control.UpdateBinding();
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Fan control)
            {
                control.UpdateState();
            }
        }

        private void UpdateBinding()
        {
            if (string.IsNullOrEmpty(TagName) || string.IsNullOrWhiteSpace(TagName) || MQTT_TagCollection.Tags.Count == 0)
            {
                return;
            }

            if (!MQTT_TagCollection.Tags.ContainsKey(TagName))
            {
                return;
            }

            var tagObject = MQTT_TagCollection.Tags[TagName];

            BindingOperations.ClearBinding(this, ValueProperty);
            Binding binding = new Binding("Value")
            {
                Source = tagObject,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            BindingOperations.SetBinding(this, ValueProperty, binding);

            UpdateState();
        }

        private Storyboard _rotationStoryboard;

        public Fan()
        {
            InitializeComponent();
            CreateRotationAnimation();
            UpdateState();
        }

        private void UpdateState()
        {
            string currentValue = (Value ?? "false").Trim().ToLower();
            bool isOn = currentValue == "true" || currentValue == "1";

            FanOnImage.Visibility = isOn ? Visibility.Visible : Visibility.Collapsed;
            FanOffImage.Visibility = isOn ? Visibility.Collapsed : Visibility.Visible;

            if (isOn)
                _rotationStoryboard?.Begin();
            else
                _rotationStoryboard?.Stop();
        }

        private void CreateRotationAnimation()
        {
            DoubleAnimation rotationAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = new Duration(TimeSpan.FromSeconds(1)),
                RepeatBehavior = RepeatBehavior.Forever
            };

            _rotationStoryboard = new Storyboard();
            _rotationStoryboard.Children.Add(rotationAnimation);
            Storyboard.SetTarget(rotationAnimation, FanRotation);
            Storyboard.SetTargetProperty(rotationAnimation, new PropertyPath(RotateTransform.AngleProperty));
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TagName))
                {
                    MessageBox.Show("No TagName defined for this light.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Create and show the Faceplate window
                var faceplate = new Faceplate_Fan(TagName, SwitchTagName, PowerTagName, SpeedTagName);
                faceplate.Owner = Application.Current.MainWindow;
                faceplate.ShowDialog();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Light] Failed to open faceplate: {ex.Message}");
            }
        }
    }
}
