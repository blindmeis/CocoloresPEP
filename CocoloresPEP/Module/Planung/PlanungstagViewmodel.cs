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

namespace CocoloresPEP.Module.Planung
{
    public class PlanungstagViewmodel : ViewmodelBase
    {
        private readonly IMessageBoxService _msgService;
        private readonly Action _refreshAction;
        private readonly Action _action;
        private bool _isFeiertag;
        private readonly Lazy<ObservableCollection<PlanungszeitVonBisWrapper>> _lazyPlanVonBisZeiten;
        private readonly Lazy<DelegateCommand<GruppenTyp>> _lazyChangePlanGruppeCommand;
        private readonly Lazy<DelegateCommand<DienstTyp>> _lazyChangePlanzeitCommand;
        private readonly Lazy<DelegateCommand<PlanungszeitVonBisWrapper>> _lazyDeletePlanzeitCommand;
        private readonly Lazy<DelegateCommand<PlanungszeitVonBisWrapper>> _lazyUpdatePlanzeitCommand;
        public PlanungstagViewmodel(IMessageBoxService msgService, Action refreshAction)
        {
            _msgService = msgService;
            _refreshAction = refreshAction;
            _lazyChangePlanGruppeCommand = new Lazy<DelegateCommand<GruppenTyp>>(() => new DelegateCommand<GruppenTyp>(ChangePlanGruppeCommandExecute, CanChangePlanGruppeCommandExecute));
            _lazyChangePlanzeitCommand = new Lazy<DelegateCommand<DienstTyp>>(() => new DelegateCommand<DienstTyp>(ChangePlanzeitCommandExecute, CanChangePlanzeitCommandExecute));
            _lazyDeletePlanzeitCommand = new Lazy<DelegateCommand<PlanungszeitVonBisWrapper>>(()=> new DelegateCommand<PlanungszeitVonBisWrapper>(DeletePlanzeitCommandExecute, CanDeletePlanzeitCommandExecute));
            _lazyUpdatePlanzeitCommand = new Lazy<DelegateCommand<PlanungszeitVonBisWrapper>>(()=> new DelegateCommand<PlanungszeitVonBisWrapper>(UpdatePlanzeitCommandExecute, CanUpdatePlanzeitCommandExecute));
            _lazyPlanVonBisZeiten = new Lazy<ObservableCollection<PlanungszeitVonBisWrapper>>(()=> CreatePlanVonBisZeiten());
        }

        private ObservableCollection<PlanungszeitVonBisWrapper> CreatePlanVonBisZeiten()
        {
            var l = new ObservableCollection<PlanungszeitVonBisWrapper>();
            foreach (var planItem in Planzeiten)
            {
                l.Add(new PlanungszeitVonBisWrapper()
                {
                    StundeVon = planItem.Startzeit.Hour,
                    MinuteVon = planItem.Startzeit.Minute,
                    StundeBis = planItem.GetEndzeit().Hour,
                    MinuteBis = planItem.GetEndzeit().Minute,
                });
            }

            return l;
        }

        public ICommand UpdatePlanzeitCommand { get { return _lazyUpdatePlanzeitCommand.Value; } }

        private bool CanUpdatePlanzeitCommandExecute(PlanungszeitVonBisWrapper arg)
        {
            return arg != null 
                && new DateTime(1,1,1,arg.StundeVon,arg.MinuteVon,0)< new DateTime(1,1,1,arg.StundeBis,arg.MinuteBis,0)
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
                planitem.Startzeit = new DateTime(planitem.Startzeit.Year, planitem.Startzeit.Month, planitem.Startzeit.Day,obj.StundeVon,obj.MinuteVon,0);
                planitem.Endzeit = new DateTime(planitem.Startzeit.Year, planitem.Startzeit.Month, planitem.Startzeit.Day, obj.StundeBis, obj.MinuteBis, 0);

                OnPropertyChanged(nameof(PlanZeitenInfo));
                OnPropertyChanged(nameof(EingeteiltSollTyp));
                OnPropertyChanged(nameof(PausenInfo));
                _refreshAction();
            }
            catch (Exception ex)
            {
                _msgService.ShowError($"Fehler beim Änder einer Planzeit. {Environment.NewLine}{ex.GetAllErrorMessages()}");
            }
        }

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
                    plan.Startzeit = at.KernzeitGruppeEnde.AddMinutes(-1 * 15 * plan.AllTicks);
                    break;
                case DienstTyp.SpätdienstEnde:
                    plan.Startzeit = at.SpätdienstEnde.AddMinutes(-1 * 15 * plan.AllTicks);
                    break;
            }

            if (plan.Startzeit.AddMinutes(15 * plan.AllTicks) > at.SpätdienstEnde)
                plan.Startzeit = at.SpätdienstEnde.AddMinutes(-1 * 15 * plan.AllTicks);

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
        public ObservableCollection<PlanItem> Planzeiten { get; set; }
        public ObservableCollection<PlanungszeitVonBisWrapper> PlanVonBisZeiten { get { return _lazyPlanVonBisZeiten.Value; } }
             
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

        public string PlanZeitenInfo
        {
            get
            {
                if (IsFeiertag)
                    return " ";

                var sb = (from zeiten in Planzeiten
                          where (zeiten.Dienst & DienstTyp.Frei) != DienstTyp.Frei
                          let endzeit = zeiten.GetEndzeit()
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
