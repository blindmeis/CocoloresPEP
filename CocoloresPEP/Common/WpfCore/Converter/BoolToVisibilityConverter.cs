using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CocoloresPEP.Common.WpfCore.Converter
{
    /// <summary>
    /// Converter für bool - visibility 
    /// </summary>
    /// <remarks>
    /// <example>
    /// <code>
    /// <Converter:BoolToVisibilityConverter x:Key="TrueIfVisibleConverter" Inverted="False" Not="False" />
    /// <Converter:BoolToVisibilityConverter x:Key="TrueIfNotVisibleConverter" Inverted="False" Not="True" />
    /// <Converter:BoolToVisibilityConverter x:Key="VisibleIfTrueConverter" Inverted="True" Not="False" />
    /// <Converter:BoolToVisibilityConverter x:Key="VisibleIfNotTrueConverter" Inverted="True" Not="True" />
    /// </code>
    /// </example>
    /// </remarks>
    public class BoolToVisibilityConverter : IValueConverter
    {

        private bool inverted = false;
        private bool not = false;

        public bool Inverted
        {
            get { return inverted; }
            set { inverted = value; }
        }

        public bool Not
        {
            get { return not; }
            set { not = value; }
        }


        private object VisibilityToBool(object value)
        {

            if (!(value is Visibility))
                return DependencyProperty.UnsetValue;

            return (((Visibility)value) == Visibility.Visible) ^ Not;

        }

        private object BoolToVisibility(object value, object parameter)
        {

            if (!(value is bool))
                return DependencyProperty.UnsetValue;

            var hiddenodercollapsed = Visibility.Collapsed;
            if (parameter != null && parameter.ToString().ToLower().Contains("hidden"))
                hiddenodercollapsed = Visibility.Hidden;

            return ((bool)value ^ Not) ? Visibility.Visible : hiddenodercollapsed;
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Inverted ? BoolToVisibility(value, parameter) : VisibilityToBool(value);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Inverted ? VisibilityToBool(value) : BoolToVisibility(value, parameter);

        }
    }
}
