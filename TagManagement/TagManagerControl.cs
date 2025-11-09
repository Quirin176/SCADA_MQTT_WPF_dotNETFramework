using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace TagManagement
{
    public class TagManagerControl : Control
    {
        static TagManagerControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TagManagerControl), new FrameworkPropertyMetadata(typeof(TagManagerControl)));
        }

        public static readonly DependencyProperty OpenTagManagerProperty =
            DependencyProperty.RegisterAttached("OpenTagManager", typeof(bool), typeof(TagManagerControl), new PropertyMetadata(false, OnOpenTagManagerChanged));

        public static void SetOpenTagManager(UIElement element, bool value)
        {
            element.SetValue(OpenTagManagerProperty, value);
        }

        public static bool GetOpenTagManager(UIElement element)
        {
            return (bool)element.GetValue(OpenTagManagerProperty);
        }

        private static void OnOpenTagManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Window_TagManagement tagWindow = new Window_TagManagement();
                tagWindow.Show();
            }
        }
    }
}
