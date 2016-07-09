using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayname<T>(this T myEnum) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T muss ein Enum sein");
            }

            var fieldInfo = myEnum.GetType().GetField(myEnum.ToString(CultureInfo.CurrentCulture));

            var descriptionAttributes = fieldInfo?.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];

            if (descriptionAttributes == null) return string.Empty;
            return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : myEnum.ToString(CultureInfo.InvariantCulture);
        }
    }
}
