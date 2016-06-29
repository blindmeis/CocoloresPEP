using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.Extensions;

namespace CocoloresPEP.Common.Controls
{
    public class DefaultGruppenControl: ComboBox
    {
        public DefaultGruppenControl()
        {
            this.ItemsSource = new Dictionary<string, GruppenTyp>
            {
                {GruppenTyp.None.GetDisplayname(), GruppenTyp.None},
                {GruppenTyp.Gelb.GetDisplayname(), GruppenTyp.Gelb},
                {GruppenTyp.Rot.GetDisplayname(), GruppenTyp.Rot},
                {GruppenTyp.Gruen.GetDisplayname(), GruppenTyp.Gruen},
                {GruppenTyp.Nest.GetDisplayname(), GruppenTyp.Nest}
            };

        }
    }
}
