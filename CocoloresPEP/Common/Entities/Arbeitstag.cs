using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Entities
{
    public class Arbeitstag
    {
        public Arbeitstag()
        {
            
        }
        public Arbeitstag(DateTime dt)
        {
            Datum = dt;
            SollItems = new ObservableCollection<SollItem>();
            Istzeiten = new ObservableCollection<Ist>();
        }

        public DateTime Datum { get;  set; }
        public bool IsLocked { get; set; }

        public ObservableCollection<SollItem> SollItems { get;  set; }
        public ObservableCollection<Ist> Istzeiten { get;  set; }
    }
}
