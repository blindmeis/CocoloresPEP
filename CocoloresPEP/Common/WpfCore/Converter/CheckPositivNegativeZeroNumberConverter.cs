using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CocoloresPEP.Common.WpfCore.Converter
{
    /// <summary>
    /// Gibt true für Positiv, false Negativ null für 0 zurück
    /// </summary>
    public class CheckPositivNegativeZeroNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal number;

            if (decimal.TryParse(value.ToString(), out number))
            {
                if (number == 0)
                    return null;

                if (number > 0)
                    return true;

                return false;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
