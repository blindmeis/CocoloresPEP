using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.Extensions;
using CocoloresPEP.Common.WpfCore.Commanding;
using CocoloresPEP.Services;
using CocoloresPEP.Views.Mitarbeiter;

namespace CocoloresPEP.Module.Planung
{
    public class PlanungManager: ViewmodelBase
    {
        private readonly SerializationService _serializationService;
        private int _kalenderWoche;
        private int _jahr;
        private Arbeitswoche _selectedArbeitswoche;
        private ICollectionView _arbeitswocheVorschau;


        private readonly Lazy<DelegateCommand> _lazyCreatePlanungswocheCommand;
        private readonly Lazy<DelegateCommand> _lazySavePlanungswocheCommand;

        public PlanungManager()
        {
            _serializationService = new SerializationService();

            var planungen = _serializationService.ReadPlanungListe();
            ArbeitswochenCollection = new ObservableCollection<Arbeitswoche>(planungen);

            PlanungView = CollectionViewSource.GetDefaultView(ArbeitswochenCollection);
            PlanungView.SortDescriptions.Add(new SortDescription("Jahr", ListSortDirection.Ascending));
            PlanungView.SortDescriptions.Add(new SortDescription("KalenderWoche", ListSortDirection.Ascending));

            PlanungView.GroupDescriptions.Add(new PropertyGroupDescription("Jahr"));

            _lazyCreatePlanungswocheCommand = new Lazy<DelegateCommand>(()=> new DelegateCommand(CreatePlanungswocheCommandExecute, CanCreatePlanungswocheCommandExecute));
            _lazySavePlanungswocheCommand = new Lazy<DelegateCommand>(()=> new DelegateCommand(SavePlanungswocheCommandExecute, CanSavePlanungswocheCommandExecute));

            Jahr = DateTime.Now.Year;
        }
        
       
        public ICollectionView PlanungView { get; set; }

        public ObservableCollection<Arbeitswoche> ArbeitswochenCollection { get; set; }

        public Arbeitswoche SelectedArbeitswoche
        {
            get { return _selectedArbeitswoche; }
            set
            {
                _selectedArbeitswoche = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedArbeitswocheInfo));
                OnPropertyChanged(nameof(ArbeitswocheVorschau));
            }
        }

        public string SelectedArbeitswocheInfo
        {
            get
            {
                if (SelectedArbeitswoche == null)
                    return "Neue Arbeitswoche anlegen oder bestehende auswählen.";

                var von = DateTimeExtensions.FirstDateOfWeekIso8601(SelectedArbeitswoche.Jahr,SelectedArbeitswoche.KalenderWoche);
                var bis = von.AddDays(5);
                return
                    $"Jahr: {SelectedArbeitswoche.Jahr}  Kalendarwoche: {SelectedArbeitswoche.KalenderWoche}  vom {von.ToString("dd.MM.")}-{bis.ToString("dd.MM.")}";

            }
        }

        public ICollectionView ArbeitswocheVorschau
        {
            get
            {
               return  TransformArbeitswocheToPreview(SelectedArbeitswoche, GetMitarbeiters());
            }
          
        }

        public int Jahr
        {
            get { return _jahr; }
            set
            {
                _jahr = value;
                OnPropertyChanged();
            }
        }

        public int KalenderWoche
        {
            get { return _kalenderWoche; }
            set
            {
                _kalenderWoche = value; 
                OnPropertyChanged();
            }
        }

       
        #region SavePlanungswocheCommand

        public ICommand SavePlanungswocheCommand { get { return _lazySavePlanungswocheCommand.Value; } }

        private bool CanSavePlanungswocheCommandExecute()
        {
            return true;
        }

        private void SavePlanungswocheCommandExecute()
        {
            if (!CanSavePlanungswocheCommandExecute())
                return;

            var sr = new SerializationService();
            sr.WritePlanungListe(ArbeitswochenCollection.ToList());
        }
        #endregion

        #region CreatePlanungswocheCommand

        public ICommand CreatePlanungswocheCommand { get { return _lazyCreatePlanungswocheCommand.Value; } }

        private bool CanCreatePlanungswocheCommandExecute()
        {
            if (KalenderWoche > 0 && KalenderWoche < 53 &&
                !ArbeitswochenCollection.Any(x => x.Jahr == Jahr && x.KalenderWoche == KalenderWoche))
                return true;

            return false;
        }

        private void CreatePlanungswocheCommandExecute()
        {
            if(!CanCreatePlanungswocheCommandExecute())
                return;
            
            var am = new ArbeitswochenService();
            var aw = am.CreateArbeitswoche(Jahr, KalenderWoche);

            ArbeitswochenCollection.Add(aw);
            SelectedArbeitswoche = aw;

            var ps = new PlanService();
            var mitarbeiters = GetMitarbeiters();
            ps.ErstelleWochenplan(SelectedArbeitswoche,mitarbeiters);
            
            OnPropertyChanged(nameof(ArbeitswocheVorschau));
        }
        #endregion


        private ICollectionView TransformArbeitswocheToPreview(Arbeitswoche woche, IList<Common.Entities.Mitarbeiter> mitarbeiters)
        {
            var ppvms = new List<PlanungswochePreviewViewmodel>();

            foreach (Common.Entities.Mitarbeiter mitarbeiter in mitarbeiters)
            {
               var pp = new PlanungswochePreviewViewmodel();
               pp.Mitarbeiter = mitarbeiter;
                
                if(woche==null)
                    continue;

                foreach (var tag in woche.Arbeitstage)
                {
                    var dow = tag.Datum.DayOfWeek;
                    var ptvm = new PlanungstagViewmodel()
                    {
                        Datum = tag.Datum,
                        IstZeiten = tag.Istzeiten.Where(x => x.ErledigtDurch.Name == pp.Mitarbeiter.Name).ToList()
                    };
                    switch (dow)
                    {
                           case DayOfWeek.Monday:
                            pp.Montag = ptvm;
                            break;
                            case DayOfWeek.Tuesday:
                            pp.Dienstag = ptvm;
                            break;
                        case DayOfWeek.Wednesday:
                            pp.Mittwoch = ptvm;
                            break;
                        case DayOfWeek.Thursday:
                            pp.Donnerstag = ptvm;
                            break;
                        case DayOfWeek.Friday:
                            pp.Freitag = ptvm;
                            break;
                       case DayOfWeek.Saturday:
                           
                            break;
                        case DayOfWeek.Sunday:
                            
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                ppvms.Add(pp);
            }
            var ordered = ppvms.OrderBy(x => x.Mitarbeiter.DefaultGruppe).ThenBy(x => x.Mitarbeiter.Name).ToList();
            var view = CollectionViewSource.GetDefaultView(ordered);
            return view;
        }

        private IList<Common.Entities.Mitarbeiter> GetMitarbeiters()
        {
            var ms = new SerializationService();
            return ms.ReadMitarbeiterListe();
        }

    }
}
