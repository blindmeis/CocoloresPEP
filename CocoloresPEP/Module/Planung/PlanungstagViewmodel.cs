using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.WpfCore.Commanding;

namespace CocoloresPEP.Module.Planung
{
    public class PlanungstagViewmodel : ViewmodelBase
    {
        private bool _isFeiertag;
        private Lazy<DelegateCommand<GruppenTyp>> _lazyChangePlanGruppeCommand;
        private Lazy<DelegateCommand<DienstTyp>> _lazyChangePlanzeitCommand;
        public PlanungstagViewmodel()
        {
            _lazyChangePlanGruppeCommand = new Lazy<DelegateCommand<GruppenTyp>>(() => new DelegateCommand<GruppenTyp>(ChangePlanGruppeCommandExecute, CanChangePlanGruppeCommandExecute));
            _lazyChangePlanzeitCommand = new Lazy<DelegateCommand<DienstTyp>>(() => new DelegateCommand<DienstTyp>(ChangePlanzeitCommandExecute, CanChangePlanzeitCommandExecute));
        }

        public ICommand ChangePlanzeitCommand { get { return _lazyChangePlanzeitCommand.Value; } }


        private bool CanChangePlanzeitCommandExecute(DienstTyp arg)
        {
            return Planzeiten.Count() == 1 && !Planzeiten.Single().Dienst.HasFlag(arg);
        }

        private void ChangePlanzeitCommandExecute(DienstTyp obj)
        {
            if (!CanChangePlanzeitCommandExecute(obj))
                return;

            var plan = Planzeiten.Single();

            var at = new Arbeitstag(plan.Startzeit);

            switch (obj)
            {
                case DienstTyp.Frühdienst:
                    plan.Startzeit = at.Frühdienst;
                    break;
                case DienstTyp.AchtUhrDienst:
                    plan.Startzeit = at.AchtUhrDienst;
                    break;
                case DienstTyp.KernzeitStartDienst:
                    plan.Startzeit = at.KernzeitGruppeStart;
                    break;
                case DienstTyp.NeunUhrDienst:
                    plan.Startzeit = at.NeunUhrDienst;
                    break;
                case DienstTyp.ZehnUhrDienst:
                    plan.Startzeit = at.ZehnUhrDienst;
                    break;
                case DienstTyp.KernzeitEndeDienst:
                    plan.Startzeit = at.KernzeitGruppeEnde.AddMinutes(-1*15*plan.AllTicks);
                    break;
                case DienstTyp.SpätdienstEnde:
                    plan.Startzeit = at.SpätdienstEnde.AddMinutes(-1*15*plan.AllTicks);
                    break;
            }

            if(plan.Startzeit.AddMinutes(15*plan.AllTicks)> at.SpätdienstEnde)
                plan.Startzeit = at.SpätdienstEnde.AddMinutes(-1 * 15 * plan.AllTicks);

            OnPropertyChanged(nameof(IstZeitenInfo));
        }

        public ICommand ChangePlanGruppeCommand { get { return _lazyChangePlanGruppeCommand.Value; } }

        private bool CanChangePlanGruppeCommandExecute(GruppenTyp gt)
        {
            return Planzeiten.Any() && Planzeiten.All(x => x.Gruppe != gt);
        }

        private void ChangePlanGruppeCommandExecute(GruppenTyp gt)
        {
            if (!CanChangePlanGruppeCommandExecute(gt))
                return;

            foreach (var item in Planzeiten)
            {
                item.Gruppe = gt;
            }

            OnPropertyChanged(nameof(EingeteiltSollTyp));
        }

        public DateTime Datum { get; set; }
        public IEnumerable<PlanItem> Planzeiten { get; set; }

        public bool IsFeiertag
        {
            get { return _isFeiertag; }
            set
            {
                _isFeiertag = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IstZeitenInfo));
            }
        }

        public string IstZeitenInfo
        {
            get
            {
                if (IsFeiertag)
                    return " ";

                var sb = (from zeiten in Planzeiten
                          let endzeit = zeiten.Startzeit.AddMinutes(15 * zeiten.AllTicks)
                          select $"{zeiten.Startzeit.ToString("HH:mm")}-{endzeit.ToString("HH:mm")}"
                          ).ToList();

                return string.Join(Environment.NewLine, sb);
            }
        }

        public GruppenTyp EingeteiltSollTyp
        {
            get
            {
                var s = GruppenTyp.None;
                foreach (var planItem in Planzeiten)
                {
                    if ((s & planItem.Gruppe) == planItem.Gruppe)
                        continue;

                    s |= planItem.Gruppe;
                }

                return s;
            }
        }

        public string Wochentag
        {
            get
            {
                var culture = new System.Globalization.CultureInfo("de-DE");
                return $"{culture.DateTimeFormat.GetDayName(Datum.DayOfWeek)}: {Datum.ToString("dd.MM.")}";
            }
        }

        public string PausenInfo
        {
            get { return Planzeiten.Any(x => x.BreakTicks > 0) ? "P" : " "; }
        }
    }
}
