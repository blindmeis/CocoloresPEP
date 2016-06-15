using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CocoloresPEP.Common.WpfCore.Controls
{
    public static class DesignModeTool
    {
        public static readonly DependencyProperty IsHiddenProperty =
            DependencyProperty.RegisterAttached("IsHidden",
                typeof(bool),
                typeof(DesignModeTool),
                new FrameworkPropertyMetadata(false,
                    OnIsHiddenChanged));

        public static void SetIsHidden(FrameworkElement element, bool value)
        {
            element.SetValue(IsHiddenProperty, value);
        }

        public static bool GetIsHidden(FrameworkElement element)
        {
            return (bool)element.GetValue(IsHiddenProperty);
        }

        private static void OnIsHiddenChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(d)) return;
            var element = (FrameworkElement)d;
            element.RenderTransform = (bool)e.NewValue
                ? new ScaleTransform(0, 0)
                : null;
        }
    }
}
