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
        private readonly Lazy<DelegateCommand<Themen>> _lazyChangeThemaCommand;
        private readonly Lazy<DelegateCommand<PlanungszeitVonBisWrapper>> _lazyDeletePlanzeitCommand;
        private readonly Lazy<DelegateCommand<PlanungszeitVonBisWrapper>> _lazyUpdatePlanzeitCommand;
        private PlanItem _planzeit;
        private bool _hasGrossteam;
        private PlanungszeitVonBisWrapper _planVonBisZeit;

        public PlanungstagViewmodel(MitarbeiterViewmodel mitarbeiter, IMessageBoxService msgService, Action refreshAction)
        {
            _mitarbeiter = mitarbeiter;
            _msgService = msgService;
            _refreshAction = refreshAction;
            _lazyChangePlanGruppeCommand = new Lazy<DelegateCommand<GruppenTyp>>(() => new DelegateCommand<GruppenTyp>(ChangePlanGruppeCommandExecute, CanChangePlanGruppeCommandExecute));
            _lazyChangePlanzeitCommand = new Lazy<DelegateCommand<DienstTyp>>(() => new DelegateCommand<DienstTyp>(ChangePlanzeitCommandExecute, CanChangePlanzeitCommandExecute));
            _lazyDeletePlanzeitCommand = new Lazy<DelegateCommand<PlanungszeitVonBisWrapper>>(()=> new DelegateCommand<PlanungszeitVonBisWrapper>(DeletePlanzeitCommandExecute, CanDeletePlanzeitCommandExecute));
            _lazyUpdatePlanzeitCommand = new Lazy<DelegateCommand<PlanungszeitVonBisWrapper>>(()=> new DelegateCommand<PlanungszeitVonBisWrapper>(UpdatePlanzeitCommandExecute, CanUpdatePlanzeitCommandExecute));
            _lazyChangeThemaCommand = new Lazy<DelegateCommand<Themen>>(()=> new DelegateCommand<Themen>(ChangeThemaCommandExecute, CanChangeThemaCommandExecute));
        }

        private void RefreshPlanVonBisZeiten()
        {
            PlanVonBisZeit =new PlanungszeitVonBisWrapper()
                {
                    StundeVon = Planzeit?.Zeitraum.Start.Hour ?? 0,
                    MinuteVon = Planzeit?.Zeitraum.Start.Minute ?? 0,
                    StundeBis = Planzeit?.Zeitraum.End.Hour ?? 0,
                    MinuteBis = Planzeit?.Zeitraum.End.Minute ?? 0,
                };
            
        }

        #region UpdatePlanzeitCommand

        public ICommand UpdatePlanzeitCommand { get { return _lazyUpdatePlanzeitCommand.Value; } }

        private bool CanUpdatePlanzeitCommandExecute(PlanungszeitVonBisWrapper arg)
        {
            DateTime start;
            DateTime ende;
            GetStartAndEndzeit(arg, out start, out ende);

            return arg != null
                && start < ende;
        }
        private void UpdatePlanzeitCommandExecute(PlanungszeitVonBisWrapper obj)
        {
            if (!CanUpdatePlanzeitCommandExecute(obj))
                return;

            try
            {
                var start = new DateTime(Planzeit.Zeitraum.Start.Year, Planzeit.Zeitraum.Start.Month, Planzeit.Zeitraum.Start.Day, obj.StundeVon, obj.MinuteVon, 0);
                var ende = new DateTime(Planzeit.Zeitraum.Start.Year, Planzeit.Zeitraum.Start.Month, Planzeit.Zeitraum.Start.Day, obj.StundeBis, obj.MinuteBis, 0);
                Planzeit.Zeitraum = new TimeRange(start,ende);

                OnPropertyChanged(nameof(PlanZeitenInfo));
                OnPropertyChanged(nameof(EingeteiltSollTyp));
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
            return arg != null;
        }

        private void DeletePlanzeitCommandExecute(PlanungszeitVonBisWrapper obj)
        {
            if (!CanDeletePlanzeitCommandExecute(obj))
                return;

            try
            {
                var at = Planzeit.Arbeitstag;

                Planzeit = at.EmptyPlanzeitOhneMitarbeiter;

                OnPropertyChanged(nameof(PlanZeitenInfo));
                OnPropertyChanged(nameof(EingeteiltSollTyp));
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
            return Planzeit.Dienst != arg;
        }

        private void ChangePlanzeitCommandExecute(DienstTyp obj)
        {
            if (!CanChangePlanzeitCommandExecute(obj))
                return;

            var plan = Planzeit;
            
            var dauer = (int)plan.Zeitraum.Duration.TotalMinutes;

            if (dauer == 0)
                dauer = _mitarbeiter.TagesArbeitszeitInMinuten;

            switch (obj)
            {
                case DienstTyp.Frühdienst:
                    plan.Dienst = DienstTyp.Frühdienst;
                    plan.Zeitraum  = new TimeRange(plan.Arbeitstag.Frühdienst, plan.Arbeitstag.Frühdienst.AddMinutes(dauer));
                    break;
                case DienstTyp.AchtUhrDienst:
                    plan.Dienst = DienstTyp.AchtUhrDienst;
                    plan.Zeitraum = new TimeRange(plan.Arbeitstag.AchtUhrDienst, plan.Arbeitstag.AchtUhrDienst.AddMinutes(dauer));
                    break;
                case DienstTyp.KernzeitStartDienst:
                    plan.Dienst = DienstTyp.KernzeitStartDienst;
                    plan.Zeitraum = new TimeRange(plan.Arbeitstag.KernzeitGruppeStart, plan.Arbeitstag.KernzeitGruppeStart.AddMinutes(dauer));
                    break;
                case DienstTyp.NeunUhrDienst:
                    plan.Dienst = DienstTyp.NeunUhrDienst;
                    plan.Zeitraum = new TimeRange(plan.Arbeitstag.NeunUhrDienst, plan.Arbeitstag.NeunUhrDienst.AddMinutes(dauer));
                    break;
                case DienstTyp.ZehnUhrDienst:
                    plan.Dienst = DienstTyp.ZehnUhrDienst;
                    plan.Zeitraum = new TimeRange(plan.Arbeitstag.ZehnUhrDienst, plan.Arbeitstag.ZehnUhrDienst.AddMinutes(dauer));
                    break;
                case DienstTyp.KernzeitEndeDienst:
                    plan.Dienst = DienstTyp.KernzeitEndeDienst;
                    plan.Zeitraum = new TimeRange(plan.Arbeitstag.KernzeitGruppeEnde.AddMinutes(-1 * dauer), plan.Arbeitstag.KernzeitGruppeEnde);
                    break;
                case DienstTyp.SechszehnUhrDienst:
                    plan.Dienst = DienstTyp.SechszehnUhrDienst;
                    plan.Zeitraum = new TimeRange(plan.Arbeitstag.SechzehnUhrDienst.AddMinutes(-1 * dauer), plan.Arbeitstag.SechzehnUhrDienst);
                    break;
                case DienstTyp.SpätdienstEnde:
                    plan.Dienst = DienstTyp.SpätdienstEnde;
                    plan.Zeitraum = new TimeRange(plan.Arbeitstag.SpätdienstEnde.AddMinutes(-1 * dauer), plan.Arbeitstag.SpätdienstEnde);
                    break;
                case DienstTyp.Frei:
                    plan.Dienst = DienstTyp.Frei;
                    plan.Gruppe = GruppenTyp.None;
                    plan.Zeitraum = new TimeRange(plan.Arbeitstag.KernzeitGruppeStart, plan.Arbeitstag.KernzeitGruppeStart.AddMinutes((int)(_mitarbeiter.WochenStunden * 60 / 5)));
                    OnPropertyChanged(nameof(EingeteiltSollTyp));
                    _refreshAction();
                    break;
            }
            RefreshPlanVonBisZeiten();
            OnPropertyChanged(nameof(PlanZeitenInfo));
            OnPropertyChanged(nameof(Planzeit));
           
        }
        #endregion

        #region ChangePlanGruppeCommand
        public ICommand ChangePlanGruppeCommand { get { return _lazyChangePlanGruppeCommand.Value; } }

        private bool CanChangePlanGruppeCommandExecute(GruppenTyp gt)
        {
            return Planzeit.Gruppe!= gt;
        }

        private void ChangePlanGruppeCommandExecute(GruppenTyp gt)
        {
            if (!CanChangePlanGruppeCommandExecute(gt))
                return;

            Planzeit.Gruppe = gt;

            OnPropertyChanged(nameof(EingeteiltSollTyp));
            OnPropertyChanged(nameof(Planzeit));
        }
        #endregion

        #region ChangeThemaCommand
        public ICommand ChangeThemaCommand { get { return _lazyChangeThemaCommand.Value; } }

        private bool CanChangeThemaCommandExecute(Themen t)
        {
            return Planzeit.Thema != t;
        }

        private void ChangeThemaCommandExecute(Themen t)
        {
            if (!CanChangeThemaCommandExecute(t))
                return;

            Planzeit.Thema = t;
            OnPropertyChanged(nameof(Thema));
        } 
        #endregion

        public DateTime Datum { get; set; }

        public PlanItem Planzeit
        {
            get { return _planzeit; }
            set
            {
                _planzeit = value;

                if(value != null)
                    RefreshPlanVonBisZeiten();
            }
        }

        public PlanungszeitVonBisWrapper PlanVonBisZeit
        {
            get { return _planVonBisZeit; }
            set
            {
                _planVonBisZeit = value; 
                OnPropertyChanged();
            }
        }

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
                if (value && Planzeit.CanSetHatGrossteam())
                {
                    _hasGrossteam = true;
                    Planzeit.SetHatGrossteam();
                }

                if (!value)
                {
                    _hasGrossteam = false;
                    Planzeit.HatGrossteam = false;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(PlanZeitenInfo));
            }
        }

        public string PlanZeitenInfo
        {
            get
            {
                return Planzeit.GetInfoPlanzeitInfo();
            }
        }

        public GruppenTyp EingeteiltSollTyp
        {
            get
            {
                return Planzeit?.Gruppe ?? GruppenTyp.None;
            }
        }

        public Themen Thema
        {
            get
            {
                return Planzeit?.Thema ?? Themen.None;
            }
        }
        public string Wochentag
        {
            get
            {
                return $"{Datum.GetWochentagName()}: {Datum.ToString("dd.MM.")}";
            }
        }

        public decimal Zak
        {
            get { return (decimal)Planzeit.GetArbeitsminutenAmKindOhnePause()/60; }
        }

        private void GetStartAndEndzeit(PlanungszeitVonBisWrapper arg, out DateTime start, out DateTime ende)
        {
            start = new DateTime(Datum.Year, Datum.Month, Datum.Day, arg?.StundeVon ?? 0, arg?.MinuteVon ?? 0, 0);
            ende = new DateTime(Datum.Year, Datum.Month, Datum.Day, arg?.StundeBis ?? 0, arg?.MinuteBis ?? 0, 0);
        }
    }
}
