using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TagManagement
{
    public static class DesignTimeHelper
    {
        public static readonly DependencyProperty OpenTagManagerProperty =
            DependencyProperty.RegisterAttached(
                "OpenTagManager",
                typeof(bool),
                typeof(DesignTimeHelper),
                new PropertyMetadata(false, OnOpenTagManagerChanged));

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
            if ((bool)e.NewValue && System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
            {
                Window_TagManagement tagWindow = new Window_TagManagement();
                tagWindow.Show();
            }
        }
    }
}
