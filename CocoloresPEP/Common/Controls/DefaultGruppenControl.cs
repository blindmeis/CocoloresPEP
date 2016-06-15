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
            this.ItemsSource = new Dictionary<string, SollTyp>
            {
                {" ", SollTyp.None},
                {"Blau", SollTyp.Blau},
                {"Rot", SollTyp.Rot},
                {"Grün", SollTyp.Gruen},
                {"Nest", SollTyp.Nest}
            };

        }
    }
}
