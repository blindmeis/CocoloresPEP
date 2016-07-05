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
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.Extensions;
using CocoloresPEP.Common.WpfCore.Commanding;
using CocoloresPEP.Common.WpfCore.Service.MessageBox;
using CocoloresPEP.Module.Mitarbeiter;
using CocoloresPEP.Services;
using CocoloresPEP.Views.Mitarbeiter;

namespace CocoloresPEP.Module.Planung
{
    public class PlanungManager: ViewmodelBase
    {
        private readonly SerializationService _serializationService;
        private int _kalenderWoche;
        private int _jahr;
        private ArbeitswocheViewmodel _selectedArbeitswoche;


        private readonly Lazy<DelegateCommand> _lazyCreatePlanungswocheCommand;
        private readonly Lazy<DelegateCommand> _lazySavePlanungswocheCommand;
        private readonly Lazy<DelegateCommand> _lazyDeletePlanungswocheCommand;
        private readonly Lazy<DelegateCommand> _lazyRunPlanungCommand;
        private readonly Lazy<DelegateCommand> _lazyRunPlanungCheckCommand;
        private readonly Lazy<DelegateCommand> _lazyExportPlanungCommand;
        private PlanungswocheMitarbeiterViewmodel _selectedPlanungswocheMitarbeiterItem;
        private WpfMessageBoxService _msg;


        public PlanungManager()
        {
            _serializationService = new SerializationService();
            _msg = new WpfMessageBoxService();

            var planungen = _serializationService.ReadPlanungListe() ?? new List<Arbeitswoche>();
            ArbeitswochenCollection = new ObservableCollection<ArbeitswocheViewmodel>(planungen.Select(x=>x.MapArbeitswocheToViewmodel()));

            PlanungView = CollectionViewSource.GetDefaultView(ArbeitswochenCollection);
            PlanungView.SortDescriptions.Add(new SortDescription("Jahr", ListSortDirection.Ascending));
            PlanungView.SortDescriptions.Add(new SortDescription("KalenderWoche", ListSortDirection.Ascending));

            PlanungView.GroupDescriptions.Add(new PropertyGroupDescription("Jahr"));

            _lazyCreatePlanungswocheCommand = new Lazy<DelegateCommand>(()=> new DelegateCommand(CreatePlanungswocheCommandExecute, CanCreatePlanungswocheCommandExecute));
            _lazySavePlanungswocheCommand = new Lazy<DelegateCommand>(()=> new DelegateCommand(SavePlanungswocheCommandExecute, CanSavePlanungswocheCommandExecute));
            _lazyDeletePlanungswocheCommand = new Lazy<DelegateCommand>(()=> new DelegateCommand(DeletePlanungswocheCommandExecute, CanDeletePlanungswocheCommandExecute));
            _lazyRunPlanungCommand = new Lazy<DelegateCommand>(()=> new DelegateCommand(RunPlanungCommandExecute, CanRunPlanungCommandExecute));
            _lazyRunPlanungCheckCommand = new Lazy<DelegateCommand>(()=> new DelegateCommand(RunPlanungCheckCommandExecute, CanRunPlanungCheckCommandExecute));
            _lazyExportPlanungCommand = new Lazy<DelegateCommand>(()=> new DelegateCommand(ExportPlanungCommandExecute, CanExportPlanungCommandExecute));


            Jahr = DateTime.Now.Year;
        }

      

        public ICollectionView PlanungView { get; set; }

        public ObservableCollection<ArbeitswocheViewmodel> ArbeitswochenCollection { get; set; }

        public ArbeitswocheViewmodel SelectedArbeitswoche
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

        public PlanungswocheMitarbeiterViewmodel SelectedPlanungswocheMitarbeiterItem
        {
            get { return _selectedPlanungswocheMitarbeiterItem; }
            set
            {
                _selectedPlanungswocheMitarbeiterItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedPlanungswocheMitarbeiter));
            }
        }
        public MitarbeiterViewmodel SelectedPlanungswocheMitarbeiter
        {
            get
            {
                return SelectedPlanungswocheMitarbeiterItem?.Mitarbeiter;
            }

        }

        public ICollectionView ArbeitswocheVorschau
        {
            get
            {
               return  TransformArbeitswocheToPreview(SelectedArbeitswoche);
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
            return !IsBusy && ArbeitswochenCollection.Count > 0;
        }

        private async void SavePlanungswocheCommandExecute()
        {
            if (!CanSavePlanungswocheCommandExecute())
                return;

            IsBusy = true;

            try
            {
                await Task.Run(() =>
                {
                    var sr = new SerializationService();
                    sr.WritePlanungListe(ArbeitswochenCollection.Select(x=>x.MapViewmodelToArbeitswoche()).ToList());
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern der Planung.{Environment.NewLine}{ex.GetAllErrorMessages()}");
            }
            finally
            {
                IsBusy = false;
                CommandManager.InvalidateRequerySuggested();
            }
           
        }
        #endregion

        #region DeletePlanungswocheCommand
        public ICommand DeletePlanungswocheCommand { get { return _lazyDeletePlanungswocheCommand.Value; } }
        
        private bool CanDeletePlanungswocheCommandExecute()
        {
            return !IsBusy && SelectedArbeitswoche != null;
        }

        private void DeletePlanungswocheCommandExecute()
        {
            if (!CanDeletePlanungswocheCommandExecute())
                return;

            ArbeitswochenCollection.Remove(SelectedArbeitswoche);
            if(ArbeitswochenCollection.Count==0)
                return;

            SelectedArbeitswoche = ArbeitswochenCollection.First();
            ArbeitswocheVorschau.Refresh();
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
            var ms = new SerializationService();
            aw.Mitarbeiter = new List<Common.Entities.Mitarbeiter>(ms.ReadMitarbeiterListe());
            var vm = aw.MapArbeitswocheToViewmodel();
            ArbeitswochenCollection.Add(vm);
            SelectedArbeitswoche = vm;

            ArbeitswocheVorschau.Refresh();
        }
        #endregion

        #region RunPlanungCommand
        public ICommand RunPlanungCommand { get { return _lazyRunPlanungCommand.Value; } }
        private bool CanRunPlanungCommandExecute()
        {
            return !IsBusy && SelectedArbeitswoche != null;
        }

        private async void RunPlanungCommandExecute()
        {
            if(!CanRunPlanungCommandExecute())
                return;

            if (SelectedArbeitswoche.PlanungProMitarbeiterListe.Any(x => x.HasPlanzeitenEntries))
            {
                var dlg = _msg.ShowYesNo("Wollen Sie eine neue Planung durchführen?", CustomDialogIcons.Question);
                if (dlg == CustomDialogResults.No)
                    return;
            }

            IsBusy = true;

            try
            {
                var woche = SelectedArbeitswoche.MapViewmodelToArbeitswoche();

                await Task.Run(() => PlanService.ErstelleWochenplan(woche, woche.Mitarbeiter));

                var neu = woche.MapArbeitswocheToViewmodel();

                ArbeitswochenCollection.Remove(SelectedArbeitswoche);
                ArbeitswochenCollection.Add(neu);
                SelectedArbeitswoche = neu;

                ArbeitswocheVorschau.Refresh();

                SelectedPlanungswocheMitarbeiterItem = SelectedArbeitswoche.PlanungProMitarbeiterListe.First();

                FocusToBindingPath = nameof(ArbeitswocheVorschau);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Ausführen der Planung.{Environment.NewLine}{ex.GetAllErrorMessages()}");
            }
            finally
            {
                IsBusy = false;
                CommandManager.InvalidateRequerySuggested();
            }

           
        }

        #endregion

        #region RunPlanungCheckCommand

        public ICommand RunPlanungCheckCommand { get { return _lazyRunPlanungCheckCommand.Value; } }

        private bool CanRunPlanungCheckCommandExecute()
        {
            return !IsBusy && SelectedArbeitswoche != null;
        }

        private async void RunPlanungCheckCommandExecute()
        {
            if (!CanRunPlanungCheckCommandExecute())
                return;

            IsBusy = true;

            try
            {
                var woche = SelectedArbeitswoche.MapViewmodelToArbeitswoche();
                //await Task.Run(() => PlanService.CheckPlanung(woche));

                //var neu = woche.MapArbeitswocheToViewmodel();

                //ArbeitswochenCollection.Remove(SelectedArbeitswoche);
                //ArbeitswochenCollection.Add(neu);
                //SelectedArbeitswoche = neu;

                //ArbeitswocheVorschau.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Ausführen der Planung.{Environment.NewLine}{ex.GetAllErrorMessages()}");
            }
            finally
            {
                IsBusy = false;
            }

        }
        #endregion

        #region ExportPlanungCommand
        
        public ICommand ExportPlanungCommand { get { return _lazyExportPlanungCommand.Value; } }
        private bool CanExportPlanungCommandExecute()
        {
            return !IsBusy && SelectedArbeitswoche!=null && SelectedArbeitswoche.PlanungProMitarbeiterListe.Any(x => x.HasPlanzeitenEntries);
        }

        private async void ExportPlanungCommandExecute()
        {
            if (!CanExportPlanungCommandExecute())
                return;
            IsBusy = true;
            try
            {
                var woche = SelectedArbeitswoche.MapViewmodelToArbeitswoche();
                var ms = await Task.Run(() =>
                {
                    var service = new ExcelExportService();
                    return service.SchreibeIstZeiten(woche, woche.Mitarbeiter);
                });

                ms.CreateXlsxFileOnTempPath(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Export der Planung.{Environment.NewLine}{ex.GetAllErrorMessages()}");
            }
            finally
            {
                IsBusy = false;
                CommandManager.InvalidateRequerySuggested();
            }
        } 
        #endregion

        private ICollectionView TransformArbeitswocheToPreview(ArbeitswocheViewmodel woche)
        {
            if (woche == null)
                return null;
          
            var ordered = woche.PlanungProMitarbeiterListe
                               .OrderBy(x => x.Mitarbeiter.DefaultGruppe)
                               .ThenBy(x => x.Mitarbeiter.IsHelfer)
                               .ThenBy(x => x.Mitarbeiter.Name)
                               .ToList();

            var view = CollectionViewSource.GetDefaultView(ordered);
            return view;
        }

        public bool HasChanges()
        {
            var planungen = _serializationService.ReadPlanungListe() ?? new List<Arbeitswoche>();
            var savedCollection = new ObservableCollection<ArbeitswocheViewmodel>(planungen.Select(x => x.MapArbeitswocheToViewmodel()));

            if (savedCollection.Count != ArbeitswochenCollection.Count)
                return true;

            var savedOrdered = savedCollection.OrderBy(x => x.Jahr).ThenBy(x=>x.KalenderWoche).ToList();
            var checkOrdered = ArbeitswochenCollection.OrderBy(x => x.Jahr).ThenBy(x => x.KalenderWoche).ToList();

            for (int i = 0; i < savedOrdered.Count; i++)
            {
                var saved = savedOrdered[i];
                var check = checkOrdered[i];

                if(saved.Jahr != check.Jahr || saved.KalenderWoche != check.KalenderWoche)
                    return true;

                if (saved.Mitarbeiter.Count != check.Mitarbeiter.Count)
                    return true;

                for (int j = 0; j < saved.Mitarbeiter.Count; j++)
                {
                    var savedMitarbeiter = saved.Mitarbeiter[j];
                    var checkMitarbeiter = check.Mitarbeiter[j];

                    if (savedMitarbeiter.Name != checkMitarbeiter.Name
                        || savedMitarbeiter.DefaultGruppe != checkMitarbeiter.DefaultGruppe
                        || savedMitarbeiter.WochenStunden != checkMitarbeiter.WochenStunden
                        || savedMitarbeiter.Wunschdienste != checkMitarbeiter.Wunschdienste
                        || savedMitarbeiter.KindFreieZeit != checkMitarbeiter.KindFreieZeit)
                        return true;

                    if (savedMitarbeiter.NichtDaZeiten.Count != checkMitarbeiter.NichtDaZeiten.Count
                        || savedMitarbeiter.NichtDaZeiten.Intersect(checkMitarbeiter.NichtDaZeiten).Count() != savedMitarbeiter.NichtDaZeiten.Count)
                        return true;

                    var savedPlanzeiten = saved.PlanungProMitarbeiterListe.SingleOrDefault(x => x.Mitarbeiter == savedMitarbeiter);
                    var checkPlanzeiten = check.PlanungProMitarbeiterListe.SingleOrDefault(x => x.Mitarbeiter == checkMitarbeiter);

                    if (savedPlanzeiten == null && checkPlanzeiten != null
                        || savedPlanzeiten != null && checkPlanzeiten == null)
                        return true;

                    if(savedPlanzeiten == null && checkPlanzeiten == null)
                        continue;

                    var savedPlanitems = savedPlanzeiten.Montag.Planzeiten
                        .Union(savedPlanzeiten.Dienstag.Planzeiten)
                        .Union(savedPlanzeiten.Mittwoch.Planzeiten)
                        .Union(savedPlanzeiten.Donnerstag.Planzeiten)
                        .Union(savedPlanzeiten.Freitag.Planzeiten).ToList();

                    var checkPlanitems = checkPlanzeiten.Montag.Planzeiten
                        .Union(checkPlanzeiten.Dienstag.Planzeiten)
                        .Union(checkPlanzeiten.Mittwoch.Planzeiten)
                        .Union(checkPlanzeiten.Donnerstag.Planzeiten)
                        .Union(checkPlanzeiten.Freitag.Planzeiten).ToList();

                    if (savedPlanitems.Count != checkPlanitems.Count)
                        return true;

                    for (int k = 0; k < savedPlanitems.Count; k++)
                    {
                        var savedPl = savedPlanitems[i];
                        var checkPl = checkPlanitems[i];

                        if (!savedPl.Zeitraum.Equals(checkPl.Zeitraum)
                            || savedPl.Dienst != checkPl.Dienst
                            || savedPl.Gruppe != checkPl.Gruppe)
                            return true;
                    }
                }

                if (saved.IsMontagFeiertag != check.IsMontagFeiertag
                    || saved.IsDienstagFeiertag != check.IsDienstagFeiertag
                    || saved.IsMittwochFeiertag != check.IsMittwochFeiertag
                    || saved.IsDonnerstagFeiertag != check.IsDonnerstagFeiertag
                    || saved.IsFreitagFeiertag != check.IsFreitagFeiertag
                    || saved.HasMontagGrossteam != check.HasMontagGrossteam
                    || saved.HasDienstagGrossteam != check.HasDienstagGrossteam
                    || saved.HasMittwochGrossteam != check.HasMittwochGrossteam
                    || saved.HasDonnerstagGrossteam != check.HasDonnerstagGrossteam
                    || saved.HasFreitagGrossteam != check.HasFreitagGrossteam)
                    return true;

            }

            return false;
        }

    }
}
