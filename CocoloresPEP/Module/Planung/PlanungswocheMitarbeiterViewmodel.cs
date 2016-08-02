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
        private PropertyObserver<MitarbeiterViewmodel> _observerMitarbeiter;
        private PropertyObserver<PlanungstagViewmodel> _observerMontag;
        private PropertyObserver<PlanungstagViewmodel> _observerDienstag;
        private PropertyObserver<PlanungstagViewmodel> _observerMittwoch;
        private PropertyObserver<PlanungstagViewmodel> _observerDonnerstag;
        private PropertyObserver<PlanungstagViewmodel> _observerFreitag;
        private MitarbeiterViewmodel _mitarbeiter;
        private PlanungstagViewmodel _montag;
        private PlanungstagViewmodel _dienstag;
        private PlanungstagViewmodel _mittwoch;
        private PlanungstagViewmodel _donnerstag;
        private PlanungstagViewmodel _freitag;

        public MitarbeiterViewmodel Mitarbeiter
        {
            get { return _mitarbeiter; }
            set
            {
                _mitarbeiter = value;
                _observerMitarbeiter = new PropertyObserver<MitarbeiterViewmodel>(_mitarbeiter)
                    .RegisterHandler(n => n.KindFreieZeit, n => {Refresh();})
                    .RegisterHandler(n=> n.WochenStunden, n=> Refresh());
            }
        }

        public PlanungstagViewmodel Montag
        {
            get { return _montag; }
            set
            {
                _montag = value;
                _observerMontag = new PropertyObserver<PlanungstagViewmodel>(_montag)
                    .RegisterHandler(n=>n.HasGrossteam, n=> OnPropertyChanged(nameof(PlusMinusStunden)));
            }
        }

        public PlanungstagViewmodel Dienstag
        {
            get { return _dienstag; }
            set
            {
                _dienstag = value;
                _observerDienstag = new PropertyObserver<PlanungstagViewmodel>(_dienstag)
                   .RegisterHandler(n => n.HasGrossteam, n => OnPropertyChanged(nameof(PlusMinusStunden)));
            }
        }

        public PlanungstagViewmodel Mittwoch
        {
            get { return _mittwoch; }
            set
            {
                _mittwoch = value;
                _observerMittwoch = new PropertyObserver<PlanungstagViewmodel>(_mittwoch)
                  .RegisterHandler(n => n.HasGrossteam, n => OnPropertyChanged(nameof(PlusMinusStunden)));
            }
        }

        public PlanungstagViewmodel Donnerstag
        {
            get { return _donnerstag; }
            set
            {
                _donnerstag = value;
                _observerDonnerstag = new PropertyObserver<PlanungstagViewmodel>(_donnerstag)
                  .RegisterHandler(n => n.HasGrossteam, n => OnPropertyChanged(nameof(PlusMinusStunden)));
            }
        }

        public PlanungstagViewmodel Freitag
        {
            get { return _freitag; }
            set
            {
                _freitag = value;
                _observerFreitag = new PropertyObserver<PlanungstagViewmodel>(_freitag)
                  .RegisterHandler(n => n.HasGrossteam, n => OnPropertyChanged(nameof(PlusMinusStunden)));
            }
        }

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
                var l = new List<PlanungstagViewmodel>()
                {
                    Montag, Dienstag, Mittwoch, Donnerstag, Freitag
                };

                var arbeitAmKindMinuten = l.Where(x=>(x.Planzeit.Dienst & DienstTyp.Frei)!=DienstTyp.Frei)
                                           .Sum(x=>x.Planzeit.GetArbeitsminutenAmKindOhnePause());

                return (decimal)arbeitAmKindMinuten/60;
            }
        }

        public decimal KfzStunden
        {
            get
            {
                var kfz = Mitarbeiter.KindFreieZeit;
               
                return (decimal)kfz;
            }
        }

        public decimal GrossteamStunden
        {
            get
            {
                var montag = Montag.Planzeit.GetGrossteamZeitInMinmuten();
                var dienstag = Dienstag.Planzeit.GetGrossteamZeitInMinmuten();
                var mittwoch = Mittwoch.Planzeit.GetGrossteamZeitInMinmuten();
                var donnerstag = Donnerstag.Planzeit.GetGrossteamZeitInMinmuten();
                var freitag = Freitag.Planzeit.GetGrossteamZeitInMinmuten();

                var gtMinuten = montag + dienstag + mittwoch + donnerstag + freitag;
                       

                return (decimal) gtMinuten/60;
            }
        }

        public decimal PlusMinusStunden
        {
            get
            {
                var l = new List<PlanungstagViewmodel>()
                {
                    Montag, Dienstag, Mittwoch, Donnerstag, Freitag
                };

                var frei = l.Where(x => (x.Planzeit.Dienst & DienstTyp.Frei) == DienstTyp.Frei)
                            .Sum(x => x.Planzeit.GetArbeitsminutenAmKindOhnePause());
                var arbeitAmKindMinuten = ArbeitAmKindStunden*60;
                var grossteam = GrossteamStunden*60;
                var sollWochenstundenMinuten = Mitarbeiter.WochenStunden *60;
                var kfzInMinuten = Mitarbeiter.KindFreieZeit * 60;
                var saldo = (decimal)((int)arbeitAmKindMinuten + frei + kfzInMinuten + grossteam - sollWochenstundenMinuten) /60;

                return saldo;
            }
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(PlusMinusStunden));
            OnPropertyChanged(nameof(ArbeitAmKindStunden));
            OnPropertyChanged(nameof(KfzStunden));
        }
    }
}
