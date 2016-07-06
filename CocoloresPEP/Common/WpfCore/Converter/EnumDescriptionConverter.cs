using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CocoloresPEP.Common.WpfCore.Converter
{
    public class EnumDescriptionConverter : IValueConverter
    {
        private string GetEnumDescription(Enum myEnum)
        {
            var fieldInfo = myEnum.GetType().GetField(myEnum.ToString(CultureInfo.CurrentCulture));

            var descriptionAttributes = fieldInfo?.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];

            if (descriptionAttributes == null) return string.Empty;
            return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : myEnum.ToString(CultureInfo.InvariantCulture);
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum myEnum = (Enum)value;
            string description = GetEnumDescription(myEnum);
            return description;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
