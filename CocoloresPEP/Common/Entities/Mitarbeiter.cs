using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Entities
{
    public class Mitarbeiter
    {
        public Mitarbeiter()
        {
            NichtDaZeiten = new ObservableCollection<DateTime>();
        }
        public string Name { get; set; }

        public decimal WochenStunden { get; set; }

        public decimal SaldoWochenStunden { get; set; }

        public short TagesQuarterTicks
        {
            get
            {
                var wocheInTicks = WochenStunden * 4;
                var result = Math.Floor(wocheInTicks / 5);
                return (short)result;
            }
        }

        public ObservableCollection<DateTime> NichtDaZeiten { get; set; } 

        public SollTyp DefaultGruppe { get; set; }

        public SollTyp Wunschdienste { get; set; }
    }
}
