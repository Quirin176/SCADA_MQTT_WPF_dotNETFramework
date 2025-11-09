using HMI_Tool.Faceplate;
using MQTT_Protocol;
using MQTT_Protocol.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HMI_Tool.Light
{
    /// <summary>
    /// Interaction logic for Light.xaml
    /// </summary>
    public partial class Light : UserControl
    {
        #region Properties
        public static readonly DependencyProperty TagNameProperty =
            DependencyProperty.Register(nameof(TagName), typeof(string), typeof(Light), new PropertyMetadata(string.Empty, OnTagNameChanged));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(Light), new PropertyMetadata("false", OnValueChanged));

        public static readonly DependencyProperty OnColorProperty =
            DependencyProperty.Register(nameof(OnColor), typeof(Brush), typeof(Light), new PropertyMetadata(Brushes.Blue, OnColorChanged));

        public static readonly DependencyProperty OffColorProperty =
            DependencyProperty.Register(nameof(OffColor), typeof(Brush), typeof(Light), new PropertyMetadata(Brushes.Orange, OnColorChanged));

        public static readonly DependencyProperty SwitchTagNameProperty =
            DependencyProperty.Register(nameof(SwitchTagName), typeof(string), typeof(Light), new PropertyMetadata(string.Empty));

        public string TagName
        {
            get => (string)GetValue(TagNameProperty);
            set => SetValue(TagNameProperty, value);
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value ?? "false");
        }

        public Brush OnColor
        {
            get => (Brush)GetValue(OnColorProperty);
            set => SetValue(OnColorProperty, value);
        }

        public Brush OffColor
        {
            get => (Brush)GetValue(OffColorProperty);
            set => SetValue(OffColorProperty, value);
        }

        public string SwitchTagName
        {
            get => (string)GetValue(SwitchTagNameProperty);
            set => SetValue(SwitchTagNameProperty, value);
        }
        #endregion

        public Light()
        {
            InitializeComponent();
            UpdateState();
        }

        private static void OnTagNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Light control)
            {
                control.UpdateBinding();
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Light control)
            {
                control.UpdateState();
            }
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Light control)
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

        private void UpdateState()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(UpdateState);
                return;
            }

            string currentValue = (Value ?? "false").Trim().ToLower();
            bool isOn = currentValue == "true" || currentValue == "1";

            LightIndicator.Fill = isOn ? OnColor : OffColor;
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
                var faceplate = new Faceplate_Light(TagName, SwitchTagName);
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
