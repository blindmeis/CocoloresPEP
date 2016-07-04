using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.Extensions;
using CocoloresPEP.Common.WpfCore.Commanding;
using CocoloresPEP.Common.WpfCore.Service.MessageBox;
using CocoloresPEP.Module.Mitarbeiter;
using Itenso.TimePeriod;

namespace CocoloresPEP.Module.Planung
{
    public class PlanungstagViewmodel : ViewmodelBase
    {
        private readonly MitarbeiterViewmodel _mitarbeiter;
        private readonly IMessageBoxService _msgService;
        private readonly Action _refreshAction;
        private bool _isFeiertag;
        private readonly Lazy<DelegateCommand<GruppenTyp>> _lazyChangePlanGruppeCommand;
        private readonly Lazy<DelegateCommand<DienstTyp>> _lazyChangePlanzeitCommand;
        private readonly Lazy<DelegateCommand<PlanungszeitVonBisWrapper>> _lazyDeletePlanzeitCommand;
        private readonly Lazy<DelegateCommand<PlanungszeitVonBisWrapper>> _lazyUpdatePlanzeitCommand;
        private readonly Lazy<DelegateCommand<PlanungszeitVonBisWrapper>> _lazyAddPlanzeitCommand;
        private ObservableCollection<PlanItem> _planzeiten;
        private bool _hasGrossteam;

        public PlanungstagViewmodel(MitarbeiterViewmodel mitarbeiter, IMessageBoxService msgService, Action refreshAction)
        {
            _mitarbeiter = mitarbeiter;
            _msgService = msgService;
            _refreshAction = refreshAction;
            _lazyChangePlanGruppeCommand = new Lazy<DelegateCommand<GruppenTyp>>(() => new DelegateCommand<GruppenTyp>(ChangePlanGruppeCommandExecute, CanChangePlanGruppeCommandExecute));
            _lazyChangePlanzeitCommand = new Lazy<DelegateCommand<DienstTyp>>(() => new DelegateCommand<DienstTyp>(ChangePlanzeitCommandExecute, CanChangePlanzeitCommandExecute));
            _lazyDeletePlanzeitCommand = new Lazy<DelegateCommand<PlanungszeitVonBisWrapper>>(()=> new DelegateCommand<PlanungszeitVonBisWrapper>(DeletePlanzeitCommandExecute, CanDeletePlanzeitCommandExecute));
            _lazyUpdatePlanzeitCommand = new Lazy<DelegateCommand<PlanungszeitVonBisWrapper>>(()=> new DelegateCommand<PlanungszeitVonBisWrapper>(UpdatePlanzeitCommandExecute, CanUpdatePlanzeitCommandExecute));
            _lazyAddPlanzeitCommand = new Lazy<DelegateCommand<PlanungszeitVonBisWrapper>>(()=> new DelegateCommand<PlanungszeitVonBisWrapper>(AddPlanzeitCommandExecute, CanAddPlanzeitCommandExecute));

            PlanVonBisZeiten = new ObservableCollection<PlanungszeitVonBisWrapper>();
        }

        private void RefreshPlanVonBisZeiten()
        {
            PlanVonBisZeiten.Clear();
            foreach (var planItem in Planzeiten)
            {
                PlanVonBisZeiten.Add(new PlanungszeitVonBisWrapper()
                {
                    StundeVon = planItem.Zeitraum.Start.Hour,
                    MinuteVon = planItem.Zeitraum.Start.Minute,
                    StundeBis = planItem.Zeitraum.End.Hour,
                    MinuteBis = planItem.Zeitraum.End.Minute,
                });
            }
        }

        #region AddPlanzeitCommand
        public ICommand AddPlanzeitCommand { get { return _lazyAddPlanzeitCommand.Value; } }
        private bool CanAddPlanzeitCommandExecute(PlanungszeitVonBisWrapper arg)
        {
            DateTime start;
            DateTime ende;
            GetStartAndEndzeit(arg, out start, out ende);

            return arg != null
                   && start < ende
                   && !Planzeiten.Any(x => (start >= x.Zeitraum.Start && start <= x.Zeitraum.End)
                                          || (ende >= x.Zeitraum.Start && ende <= x.Zeitraum.End));
        }

        private void AddPlanzeitCommandExecute(PlanungszeitVonBisWrapper obj)
        {
            if (!CanAddPlanzeitCommandExecute(obj))
                return;

            try
            {
                DateTime start;
                DateTime ende;
                GetStartAndEndzeit(obj, out start, out ende);

                Planzeiten.Add(new PlanItem()
                {
                    Dienst = DienstTyp.None,
                    Zeitraum = new TimeRange(start,ende),
                    ErledigtDurch = _mitarbeiter.MapViewmodelToMitarbeiter(),
                    Gruppe = _mitarbeiter.DefaultGruppe,
                });

                RefreshPlanVonBisZeiten();

                OnPropertyChanged(nameof(PlanZeitenInfo));
                OnPropertyChanged(nameof(EingeteiltSollTyp));
                OnPropertyChanged(nameof(PausenInfo));
                _refreshAction();
            }
            catch (Exception ex)
            {
                _msgService.ShowError($"Fehler beim Hinzufügen einer Planzeit. {Environment.NewLine}{ex.GetAllErrorMessages()}");
            }
        }

        #endregion

        #region UpdatePlanzeitCommand

        public ICommand UpdatePlanzeitCommand { get { return _lazyUpdatePlanzeitCommand.Value; } }

        private bool CanUpdatePlanzeitCommandExecute(PlanungszeitVonBisWrapper arg)
        {
            DateTime start;
            DateTime ende;
            GetStartAndEndzeit(arg, out start, out ende);

            return arg != null
                && start < ende
                && Planzeiten.Count == PlanVonBisZeiten.Count;
        }
        private void UpdatePlanzeitCommandExecute(PlanungszeitVonBisWrapper obj)
        {
            if (!CanUpdatePlanzeitCommandExecute(obj))
                return;

            try
            {
                var index = PlanVonBisZeiten.IndexOf(obj);
                var planitem = Planzeiten[index];
                var start = new DateTime(planitem.Zeitraum.Start.Year, planitem.Zeitraum.Start.Month, planitem.Zeitraum.Start.Day, obj.StundeVon, obj.MinuteVon, 0);
                var ende = new DateTime(planitem.Zeitraum.Start.Year, planitem.Zeitraum.Start.Month, planitem.Zeitraum.Start.Day, obj.StundeBis, obj.MinuteBis, 0);
                planitem.Zeitraum = new TimeRange(start,ende);

                OnPropertyChanged(nameof(PlanZeitenInfo));
                OnPropertyChanged(nameof(EingeteiltSollTyp));
                OnPropertyChanged(nameof(PausenInfo));
                _refreshAction();
            }
            catch (Exception ex)
            {
                _msgService.ShowError($"Fehler beim Ändern einer Planzeit. {Environment.NewLine}{ex.GetAllErrorMessages()}");
            }
        } 
        #endregion

        #region DeletePlanzeitCommand
        public ICommand DeletePlanzeitCommand { get { return _lazyDeletePlanzeitCommand.Value; } }

        private bool CanDeletePlanzeitCommandExecute(PlanungszeitVonBisWrapper arg)
        {
            return arg != null
                && Planzeiten.Count == PlanVonBisZeiten.Count;
        }

        private void DeletePlanzeitCommandExecute(PlanungszeitVonBisWrapper obj)
        {
            if (!CanDeletePlanzeitCommandExecute(obj))
                return;

            try
            {
                var index = PlanVonBisZeiten.IndexOf(obj);
                Planzeiten.RemoveAt(index);

                OnPropertyChanged(nameof(PlanZeitenInfo));
                OnPropertyChanged(nameof(EingeteiltSollTyp));
                OnPropertyChanged(nameof(PausenInfo));
                _refreshAction();
            }
            catch (Exception ex)
            {
                _msgService.ShowError($"Fehler beim Löschen einer Planzeit. {Environment.NewLine}{ex.GetAllErrorMessages()}");
            }
        } 
        #endregion

        #region ChangePlanzeitCommand
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
            var dauer = plan.Zeitraum.Duration.TotalMinutes;


            switch (obj)
            {
                case DienstTyp.Frühdienst:
                    plan.Zeitraum.Start = plan.Arbeitstag.Frühdienst;
                    plan.Zeitraum.End = plan.Zeitraum.Start.AddMinutes(dauer);
                    break;
                case DienstTyp.AchtUhrDienst:
                    plan.Zeitraum.Start = plan.Arbeitstag.AchtUhrDienst;
                    plan.Zeitraum.End = plan.Zeitraum.Start.AddMinutes(dauer);
                    break;
                case DienstTyp.KernzeitStartDienst:
                    plan.Zeitraum.Start = plan.Arbeitstag.KernzeitGruppeStart;
                    plan.Zeitraum.End = plan.Zeitraum.Start.AddMinutes(dauer);
                    break;
                case DienstTyp.NeunUhrDienst:
                    plan.Zeitraum.Start = plan.Arbeitstag.NeunUhrDienst;
                    plan.Zeitraum.End = plan.Zeitraum.Start.AddMinutes(dauer);
                    break;
                case DienstTyp.ZehnUhrDienst:
                    plan.Zeitraum.Start = plan.Arbeitstag.ZehnUhrDienst;
                    plan.Zeitraum.End = plan.Zeitraum.Start.AddMinutes(dauer);
                    break;
                case DienstTyp.KernzeitEndeDienst:
                    plan.Zeitraum.End = plan.Arbeitstag.KernzeitGruppeEnde;
                    plan.Zeitraum.Start = plan.Zeitraum.End.AddMinutes(-1*dauer);
                    break;
                case DienstTyp.SpätdienstEnde:
                    plan.Zeitraum.End = plan.Arbeitstag.SpätdienstEnde;
                    plan.Zeitraum.Start = plan.Zeitraum.End.AddMinutes(-1 * dauer);
                    break;
            }
            OnPropertyChanged(nameof(PlanZeitenInfo));
        }
        #endregion

        #region ChangePlanGruppeCommand
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
        #endregion

        public DateTime Datum { get; set; }

        public ObservableCollection<PlanItem> Planzeiten
        {
            get { return _planzeiten; }
            set
            {
                _planzeiten = value;
                if(value != null)
                    RefreshPlanVonBisZeiten();
            }
        }

        public ObservableCollection<PlanungszeitVonBisWrapper> PlanVonBisZeiten { get; private set; }
             
        public bool IsFeiertag
        {
            get { return _isFeiertag; }
            set
            {
                _isFeiertag = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PlanZeitenInfo));
            }
        }

        public bool HasGrossteam
        {
            get { return _hasGrossteam; }
            set
            {
                _hasGrossteam = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PlanZeitenInfo));
            }
        }

        public string PlanZeitenInfo
        {
            get
            {
                if (IsFeiertag || Planzeiten.All(x=>(x.Dienst&DienstTyp.Frei)==DienstTyp.Frei))
                    return "";

                var sb = (from zeiten in Planzeiten
                          where (zeiten.Dienst & DienstTyp.Frei) != DienstTyp.Frei && (zeiten.Dienst & DienstTyp.Großteam) != DienstTyp.Großteam
                          let endzeit = zeiten.Zeitraum.End
                          select $"{zeiten.Zeitraum.Start.ToString("HH:mm")}-{endzeit.ToString("HH:mm")}"
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
            get
            {
                var tr = Planzeiten.Select(x => x.Zeitraum);
                TimePeriodCollection periods = new TimePeriodCollection(tr);
                TimePeriodCombiner<TimeRange> periodCombiner = new TimePeriodCombiner<TimeRange>();
                ITimePeriodCollection combinedPeriods = periodCombiner.CombinePeriods(periods);

                return Planzeiten.Any(x => x.Zeitraum.Duration.GetArbeitsminutenOhnePause()!=(int)x.Zeitraum.Duration.TotalMinutes) ? "P" : " ";
            }
        }

        private void GetStartAndEndzeit(PlanungszeitVonBisWrapper arg, out DateTime start, out DateTime ende)
        {
            start = new DateTime(Datum.Year, Datum.Month, Datum.Day, arg.StundeVon, arg.MinuteVon, 0);
            ende = new DateTime(Datum.Year, Datum.Month, Datum.Day, arg.StundeBis, arg.MinuteBis, 0);
        }
    }
}
