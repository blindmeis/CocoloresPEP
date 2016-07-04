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

        public decimal ArbeitAmKindStunden
        {
            get
            {
                var arbeitAmKindMinuten = Montag.Planzeiten.Sum(x => x.Zeitraum.Duration.TotalMinutes)
                         + Dienstag.Planzeiten.Sum(x => x.Zeitraum.Duration.TotalMinutes)
                         + Mittwoch.Planzeiten.Sum(x => x.Zeitraum.Duration.TotalMinutes)
                         + Donnerstag.Planzeiten.Sum(x => x.Zeitraum.Duration.TotalMinutes)
                         + Freitag.Planzeiten.Sum(x => x.Zeitraum.Duration.TotalMinutes);

                return (decimal)arbeitAmKindMinuten/60;
            }
        }

        public decimal PlusMinusStunden
        {
            get
            {
                //inclusive Frei
                var arbeitAmKindMinuten = Montag.Planzeiten.Sum(x =>x.Zeitraum.Duration.TotalMinutes)
                          + Dienstag.Planzeiten.Sum(x => x.Zeitraum.Duration.TotalMinutes)
                          + Mittwoch.Planzeiten.Sum(x => x.Zeitraum.Duration.TotalMinutes)
                          + Donnerstag.Planzeiten.Sum(x => x.Zeitraum.Duration.TotalMinutes)
                          + Freitag.Planzeiten.Sum(x => x.Zeitraum.Duration.TotalMinutes);

                var sollWochenstundenMinuten = Mitarbeiter.WochenStunden *60;
                var kfzInMinuten = Mitarbeiter.KindFreieZeit * 60;
                var saldo = (decimal)((int)arbeitAmKindMinuten + kfzInMinuten - sollWochenstundenMinuten) /60;

                return saldo;
            }
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(PlusMinusStunden));
            OnPropertyChanged(nameof(ArbeitAmKindStunden));

        }
    }
}
