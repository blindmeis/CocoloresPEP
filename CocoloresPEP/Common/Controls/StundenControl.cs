using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common.WpfCore.Controls;

namespace CocoloresPEP.Common.Controls
{
     public class StundenControl : ComboBoxWithoutDropdown
    {
         public StundenControl()
         {
            var stunden = new Dictionary<string,int>();
             for (int i = 0; i < 24; i++)
             {
                stunden.Add(i.ToString().PadLeft(2,'0'),i);
             }

             this.ItemsSource = stunden;
         }
    }
}
