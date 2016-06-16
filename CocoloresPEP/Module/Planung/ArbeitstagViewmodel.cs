using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;

namespace CocoloresPEP.Module.Planung
{
    public class ArbeitstagViewmodel : ViewmodelBase
    {

        public ArbeitstagViewmodel(DateTime dt)
        {
            Datum = dt;
            Planzeiten = new ObservableCollection<PlanItem>();
        }

        public DateTime Datum { get; set; }

        public ObservableCollection<PlanItem> Planzeiten { get; set; }
    }
}
