using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CocoloresPEP.Common.Entities;

namespace CocoloresPEP.Common.Controls
{
    public class DefaultGruppenControl: ComboBox
    {
        public DefaultGruppenControl()
        {
            this.ItemsSource = new Dictionary<string, GruppenTyp>
            {
                {" ", GruppenTyp.None},
                {"Blau", GruppenTyp.Gelb},
                {"Rot", GruppenTyp.Rot},
                {"Grün", GruppenTyp.Gruen},
                {"Nest", GruppenTyp.Nest}
            };

        }
    }
}
