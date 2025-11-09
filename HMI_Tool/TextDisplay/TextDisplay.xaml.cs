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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HMI_Tool.TextDisplay
{
    /// <summary>
    /// Interaction logic for TextDisplay.xaml
    /// </summary>
    public partial class TextDisplay : UserControl
    {
        #region Properties
        public static readonly DependencyProperty DisplayedTextProperty =
            DependencyProperty.Register(nameof(DisplayedText), typeof(string), typeof(TextDisplay), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TagNameProperty =
            DependencyProperty.Register(nameof(TagName), typeof(string), typeof(TextDisplay), new PropertyMetadata(string.Empty, OnTagNameChanged));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(TextDisplay), new PropertyMetadata(null, OnValueChanged));

        public string DisplayedText
        {
            get => (string)GetValue(DisplayedTextProperty);
            set => SetValue(DisplayedTextProperty, value);
        }

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
        #endregion

        public TextDisplay()
        {
            InitializeComponent();
        }

        private static void OnTagNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextDisplay control)
            {
                string newValue = e.NewValue as string;
                control.UpdateBinding();
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextDisplay control)
            {
                control.UpdateText();
            }
        }

        private void UpdateBinding()
        {
            if (string.IsNullOrEmpty(TagName) || string.IsNullOrWhiteSpace(TagName) || MQTT_TagCollection.Tags.Count == 0)
            {
                Debug.WriteLine($"[DisplayValue] Skipping binding update: TagName is empty or no tags available.");
                return;
            }

            if (!MQTT_TagCollection.Tags.ContainsKey(TagName))
            {
                Debug.WriteLine($"[DisplayValue] TagName '{TagName}' not found in Tags dictionary.");
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

            UpdateText();
        }

        private void UpdateText()
        {
            DisplayedText = $"{Value?.ToString() ?? "N/A"}";
        }
    }
}
