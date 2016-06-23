using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CocoloresPEP.Common.WpfCore.Converter
{
    public class StundenToMinutenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal number;

            if (decimal.TryParse(value.ToString(), out number))
            {
                return number*60;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal number;

            if (decimal.TryParse(value.ToString(), out number))
            {
                return number / 60;
            }

            return value;
        }
    }
}
