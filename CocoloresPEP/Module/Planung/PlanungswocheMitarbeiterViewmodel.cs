using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.Extensions;
using CocoloresPEP.Common.WpfCore;
using CocoloresPEP.Module.Mitarbeiter;

namespace CocoloresPEP.Module.Planung
{
    public class PlanungswocheMitarbeiterViewmodel : ViewmodelBase
    {
        private PropertyObserver<MitarbeiterViewmodel> _observer;
        private MitarbeiterViewmodel _mitarbeiter;

        public MitarbeiterViewmodel Mitarbeiter
        {
            get { return _mitarbeiter; }
            set
            {
                _mitarbeiter = value; 
                _observer = new PropertyObserver<MitarbeiterViewmodel>(_mitarbeiter)
                    .RegisterHandler(n => n.KindFreieZeit, n => OnPropertyChanged(nameof(PlusMinusStunden)));
            }
        }

        public PlanungstagViewmodel Montag { get; set; }
        public PlanungstagViewmodel Dienstag { get; set; }
        public PlanungstagViewmodel Mittwoch { get; set; }
        public PlanungstagViewmodel Donnerstag { get; set; }
        public PlanungstagViewmodel Freitag { get; set; }

        public bool HasPlanzeitenEntries
        {
            get
            {
                return Montag.Planzeit.ErledigtDurch != null
                        || Dienstag.Planzeit.ErledigtDurch != null
                        || Mittwoch.Planzeit.ErledigtDurch != null
                        || Donnerstag.Planzeit.ErledigtDurch != null
                        || Freitag.Planzeit.ErledigtDurch != null;
            }
        }

        public decimal ArbeitAmKindStunden
        {
            get
            {
                var arbeitAmKindMinuten = Montag.Planzeit.GetAbzurechnendeZeitInMinmuten()
                         + Dienstag.Planzeit.GetAbzurechnendeZeitInMinmuten()
                         + Mittwoch.Planzeit.GetAbzurechnendeZeitInMinmuten()
                         + Donnerstag.Planzeit.GetAbzurechnendeZeitInMinmuten()
                         + Freitag.Planzeit.GetAbzurechnendeZeitInMinmuten();

                return (decimal)arbeitAmKindMinuten/60;
            }
        }

        public decimal PlusMinusStunden
        {
            get
            {
                //inclusive Frei
                var arbeitAmKindMinuten = ArbeitAmKindStunden*60;

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
