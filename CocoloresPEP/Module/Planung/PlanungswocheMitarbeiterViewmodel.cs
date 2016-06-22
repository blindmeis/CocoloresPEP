using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Module.Mitarbeiter;

namespace CocoloresPEP.Module.Planung
{
    public class PlanungswocheMitarbeiterViewmodel : ViewmodelBase
    {
        public MitarbeiterViewmodel Mitarbeiter { get; set; }

        public PlanungstagViewmodel Montag { get; set; }
        public PlanungstagViewmodel Dienstag { get; set; }
        public PlanungstagViewmodel Mittwoch { get; set; }
        public PlanungstagViewmodel Donnerstag { get; set; }
        public PlanungstagViewmodel Freitag { get; set; }

        public bool HasPlanzeitenEntries
        {
            get
            {
                return (Montag.Planzeiten.Any()
                        || Dienstag.Planzeiten.Any()
                        || Mittwoch.Planzeiten.Any()
                        || Donnerstag.Planzeiten.Any()
                        || Freitag.Planzeiten.Any());
            }
        }

        public decimal PlusMinusStunden
        {
            get
            {
                var sumPlanQuarterticks = Montag.Planzeiten.Sum(x => x.QuarterTicks)
                          + Dienstag.Planzeiten.Sum(x => x.QuarterTicks)
                          + Mittwoch.Planzeiten.Sum(x => x.QuarterTicks)
                          + Donnerstag.Planzeiten.Sum(x => x.QuarterTicks)
                          + Freitag.Planzeiten.Sum(x => x.QuarterTicks);

                var minuten = sumPlanQuarterticks*15;
                if (Mitarbeiter.NichtDaZeiten.Any())
                {
                    var frei = Mitarbeiter.NichtDaZeiten.Count(x => x >= Montag.Datum && x <= Freitag.Datum);
                    minuten += frei*Mitarbeiter.TagesArbeitszeitInMinuten;
                }

                var saldo = (minuten - Mitarbeiter.WochenStunden*60)/60;

                return saldo;
            }
        }
    }
}
