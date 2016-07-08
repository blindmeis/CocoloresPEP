using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.Extensions;
using CocoloresPEP.Common.WpfCore.Commanding;
using CocoloresPEP.Common.WpfCore.Service.MessageBox;
using Itenso.TimePeriod;

namespace CocoloresPEP.Module.Planung
{
    public class ArbeitstagWrapper : ViewmodelBase
    {
        private readonly IMessageBoxService _msgService;
        private PlanungszeitVonBisWrapper _grossteamZeitWrapper;
        private PlanungszeitVonBisWrapper _kernzeitDoppelBesetzungZeitWrapper;
        private Arbeitstag _arbeitstag;

        private Lazy<DelegateCommand<PlanungszeitVonBisWrapper>> _lazyUpdateGrossteamZeitCommand;
        private Lazy<DelegateCommand<PlanungszeitVonBisWrapper>> _lazyUpdateKernzeitDoppelBesetzungZeitCommand;

        

        public ArbeitstagWrapper(IMessageBoxService msgService)
        {
            _msgService = msgService;
            _lazyUpdateGrossteamZeitCommand = new Lazy<DelegateCommand<PlanungszeitVonBisWrapper>>(()=> new DelegateCommand<PlanungszeitVonBisWrapper>(UpdateGrossteamZeitCommandExecute, CanUpdateGrossteamZeitCommandExecute));
            _lazyUpdateKernzeitDoppelBesetzungZeitCommand = new Lazy<DelegateCommand<PlanungszeitVonBisWrapper>>(()=> new DelegateCommand<PlanungszeitVonBisWrapper>(UpdateKernzeitDoppelBesetzungZeitCommandExecute, CanUpdateKernzeitDoppelBesetzungZeitCommandExecute));

        }


        public Arbeitstag Arbeitstag
        {
            get { return _arbeitstag; }
            set
            {
                if (_arbeitstag == value)
                    return;

                _arbeitstag = value;
                OnPropertyChanged();

                RefreshGrossteamZeitWrapper(value);
                RefreshKernzeitDoppelBesetzungZeitWrapper(value);
            }
        }

        private void RefreshGrossteamZeitWrapper(Arbeitstag value)
        {
            GrossteamZeitWrapper = new PlanungszeitVonBisWrapper()
            {
                StundeVon = value.GrossteamStundeVon,
                MinuteVon = value.GrossteamMinuteVon,
                StundeBis = value.GrossteamStundeBis,
                MinuteBis = value.GrossteamMinuteBis,
            };
        }
        private void RefreshKernzeitDoppelBesetzungZeitWrapper(Arbeitstag value)
        {
            KernzeitDoppelBesetzungZeitWrapper = new PlanungszeitVonBisWrapper()
            {
                StundeVon = value.KernzeitDoppelBesetzungStundeVon,
                MinuteVon = value.KernzeitDoppelBesetzungMinuteVon,
                StundeBis = value.KernzeitDoppelBesetzungStundeBis,
                MinuteBis = value.KernzeitDoppelBesetzungMinuteBis,
            };
        }

        public PlanungszeitVonBisWrapper GrossteamZeitWrapper
        {
            get { return _grossteamZeitWrapper; }
            set { _grossteamZeitWrapper = value; }
        }

        public PlanungszeitVonBisWrapper KernzeitDoppelBesetzungZeitWrapper

        {
            get { return _kernzeitDoppelBesetzungZeitWrapper; }
            set { _kernzeitDoppelBesetzungZeitWrapper = value; }
        }

        #region UpdateGrossteamZeitCommand

        public ICommand UpdateGrossteamZeitCommand { get { return _lazyUpdateGrossteamZeitCommand.Value; } }

        private bool CanUpdateGrossteamZeitCommandExecute(PlanungszeitVonBisWrapper arg)
        {
            var start = new DateTime(Arbeitstag.Datum.Year, Arbeitstag.Datum.Month, Arbeitstag.Datum.Day, arg?.StundeVon ?? 0, arg?.MinuteVon ?? 0, 0);
            var ende = new DateTime(Arbeitstag.Datum.Year, Arbeitstag.Datum.Month, Arbeitstag.Datum.Day, arg?.StundeBis ?? 0, arg?.MinuteBis ?? 0, 0);

            return arg != null && start <ende;
        }
        private void UpdateGrossteamZeitCommandExecute(PlanungszeitVonBisWrapper obj)
        {
            if (!CanUpdateGrossteamZeitCommandExecute(obj))
                return;

            try
            {
                var start = new DateTime(Arbeitstag.Datum.Year, Arbeitstag.Datum.Month, Arbeitstag.Datum.Day, obj.StundeVon, obj.MinuteVon, 0);
                var ende = new DateTime(Arbeitstag.Datum.Year, Arbeitstag.Datum.Month, Arbeitstag.Datum.Day, obj.StundeBis, obj.MinuteBis, 0);
                Arbeitstag.Grossteam = new TimeRange(start, ende);

                RefreshGrossteamZeitWrapper(Arbeitstag);
            }
            catch (Exception ex)
            {
                _msgService.ShowError($"Fehler beim Ändern einer Grossteamzeit. {Environment.NewLine}{ex.GetAllErrorMessages()}");
            }
        }
        #endregion

        #region UpdateKernzeitDoppelBesetzungZeitCommand

        public ICommand UpdateKernzeitDoppelBesetzungZeitCommand { get { return _lazyUpdateKernzeitDoppelBesetzungZeitCommand.Value; } }

        private bool CanUpdateKernzeitDoppelBesetzungZeitCommandExecute(PlanungszeitVonBisWrapper arg)
        {
            var start = new DateTime(Arbeitstag.Datum.Year, Arbeitstag.Datum.Month, Arbeitstag.Datum.Day, arg?.StundeVon ?? 0, arg?.MinuteVon ?? 0, 0);
            var ende = new DateTime(Arbeitstag.Datum.Year, Arbeitstag.Datum.Month, Arbeitstag.Datum.Day, arg?.StundeBis ?? 0, arg?.MinuteBis ?? 0, 0);

            return arg != null && start < ende;
        }
        private void UpdateKernzeitDoppelBesetzungZeitCommandExecute(PlanungszeitVonBisWrapper obj)
        {
            if (!CanUpdateKernzeitDoppelBesetzungZeitCommandExecute(obj))
                return;

            try
            {
                var start = new DateTime(Arbeitstag.Datum.Year, Arbeitstag.Datum.Month, Arbeitstag.Datum.Day, obj.StundeVon, obj.MinuteVon, 0);
                var ende = new DateTime(Arbeitstag.Datum.Year, Arbeitstag.Datum.Month, Arbeitstag.Datum.Day, obj.StundeBis, obj.MinuteBis, 0);
                Arbeitstag.KernzeitDoppelBesetzungRange = new TimeRange(start, ende);

                RefreshKernzeitDoppelBesetzungZeitWrapper(Arbeitstag);
            }
            catch (Exception ex)
            {
                _msgService.ShowError($"Fehler beim Ändern einer Kernzeit für die Doppelbesetzung. {Environment.NewLine}{ex.GetAllErrorMessages()}");
            }
        }
        #endregion
    }
}
