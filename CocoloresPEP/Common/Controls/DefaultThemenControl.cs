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
    public class DefaultThemenControl : ComboBox
    {
        public DefaultThemenControl()
        {
            this.ItemsSource = new Dictionary<string, Themen>
            {
                {Themen.None.GetDisplayname(), Themen.None},
                {Themen.Atelier.GetDisplayname(), Themen.Atelier},
                {Themen.Bauen.GetDisplayname(), Themen.Bauen},
                {Themen.Experimente.GetDisplayname(), Themen.Experimente},
                {Themen.Garten.GetDisplayname(), Themen.Garten},
                {Themen.Geschichten.GetDisplayname(), Themen.Geschichten},
                {Themen.Hauswirtschaft.GetDisplayname(), Themen.Hauswirtschaft},
                {Themen.Musik.GetDisplayname(), Themen.Musik},
                {Themen.Theater.GetDisplayname(), Themen.Theater},
            };

        }
    }
}
