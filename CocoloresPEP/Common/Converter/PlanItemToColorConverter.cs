using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.Extensions;

namespace CocoloresPEP.Common.Converter
{
    public class PlanItemToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var planitem = value as PlanItem;

            if (planitem == null)
                return Binding.DoNothing;

            if (planitem.Gruppe != planitem.ErledigtDurch?.DefaultGruppe)
            {
                return planitem.Gruppe.GetFarbeFromResources();
            }

            return planitem.Dienst.GetFarbeFromResources();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
