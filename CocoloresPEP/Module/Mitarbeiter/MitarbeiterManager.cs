using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
using CocoloresPEP.Services;

namespace CocoloresPEP.Module.Mitarbeiter
{
    public class MitarbeiterManager : ViewmodelBase
    {
        private readonly SerializationService _serializationService;
        private MitarbeiterViewmodel _selectedMitarbeiter;

        private readonly Lazy<DelegateCommand> _lazyCreateMitarbeiterCommand;
        private readonly Lazy<DelegateCommand> _lazySaveMitarbeiterCommand;
        private readonly Lazy<DelegateCommand> _lazyDeleteMitarbeiterCommand;

        public MitarbeiterManager()
        {
            _serializationService = new SerializationService();

            var mitarbeiters = _serializationService.ReadMitarbeiterListe() ?? new List<Common.Entities.Mitarbeiter>();
            MitarbeiterCollection = new ObservableCollection<MitarbeiterViewmodel>(mitarbeiters.Select(x=> x.MapMitarbeiterToViewmodel()));

            MitarbeiterView = CollectionViewSource.GetDefaultView(MitarbeiterCollection);
            MitarbeiterView.SortDescriptions.Add(new SortDescription("DefaultGruppe", ListSortDirection.Ascending));
            MitarbeiterView.SortDescriptions.Add(new SortDescription("IsHelfer", ListSortDirection.Ascending));
            MitarbeiterView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            
            _lazyCreateMitarbeiterCommand = new Lazy<DelegateCommand>(()=> new DelegateCommand(CreateMitarbeiterCommandExecute, CanCreateMitarbeiterCommandExecute));
            _lazySaveMitarbeiterCommand = new Lazy<DelegateCommand>(()=> new DelegateCommand(SaveMitarbeiterCommandExecute, CanSaveMitarbeiterCommandExecute));
            _lazyDeleteMitarbeiterCommand = new Lazy<DelegateCommand>(()=> new DelegateCommand(DeleteMitarbeiterCommandExecute, CanDeleteMitarbeiterCommandExecute));

        }

     

        public ObservableCollection<MitarbeiterViewmodel> MitarbeiterCollection { get; private set; }

        public ICollectionView MitarbeiterView { get; }


        public MitarbeiterViewmodel SelectedMitarbeiter
        {
            get { return _selectedMitarbeiter; }
            set
            {
                _selectedMitarbeiter = value;
                OnPropertyChanged();
            }
        }

        #region CreateMitarbeiterCommand

        public ICommand CreateMitarbeiterCommand { get { return _lazyCreateMitarbeiterCommand.Value; } }

        private bool CanCreateMitarbeiterCommandExecute()
        {
            return true;
        }

        private void CreateMitarbeiterCommandExecute()
        {
            if (!CanCreateMitarbeiterCommandExecute())
                return;

            SelectedMitarbeiter = new MitarbeiterViewmodel() {Name = ""};
            MitarbeiterCollection.Add(SelectedMitarbeiter);
            MitarbeiterView.Refresh();

            FocusToBindingPath = "Name";
        }

        #endregion

        #region SaveMitarbeiterCommand

        public ICommand SaveMitarbeiterCommand { get { return _lazySaveMitarbeiterCommand.Value; } }

        private bool CanSaveMitarbeiterCommandExecute()
        {
            return !IsBusy && MitarbeiterCollection.Count > 0;
        }

        private async void SaveMitarbeiterCommandExecute()
        {
            if(!CanSaveMitarbeiterCommandExecute())
                return;

            IsBusy = true;
            try
            {
                await Task.Run(() =>_serializationService.WriteMitarbeiterListe(MitarbeiterCollection.Select(x => x.MapViewmodelToMitarbeiter()).ToList()));

                MitarbeiterView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern der Daten. " + Environment.NewLine + ex.GetAllErrorMessages());
            }
            finally
            {
                IsBusy = false;
            }
            
        }

        #endregion

        #region DeleteMitarbeiterCommand
    
        public ICommand DeleteMitarbeiterCommand { get { return _lazyDeleteMitarbeiterCommand.Value; }}
        
        private bool CanDeleteMitarbeiterCommandExecute()
        {
            return SelectedMitarbeiter != null;
        }

        private void DeleteMitarbeiterCommandExecute()
        {
            if (!CanDeleteMitarbeiterCommandExecute())
                return;

            var dlg = MessageBox.Show($"Wollen Sie den Mitarbeiter: {SelectedMitarbeiter.Name} wirklich löschen?",
                "Mitarbeiter löschen", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dlg==MessageBoxResult.Yes)
                MitarbeiterCollection.Remove(SelectedMitarbeiter);
        }
        #endregion

        public bool HasChanges()
        {
            var mitarbeiters = _serializationService.ReadMitarbeiterListe() ?? new List<Common.Entities.Mitarbeiter>();
            var savedCollection = new ObservableCollection<MitarbeiterViewmodel>(mitarbeiters.Select(x => x.MapMitarbeiterToViewmodel()));

            if (savedCollection.Count != MitarbeiterCollection.Count)
                return true;

            var savedOrdered = savedCollection.OrderBy(x => x.Name).ToList();
            var checkOrdered = MitarbeiterCollection.OrderBy(x => x.Name).ToList();

            for (int i = 0; i < savedOrdered.Count; i++)
            {
                var saved = savedOrdered[i];
                var check = checkOrdered[i];

                if (saved.Name != check.Name
                    || saved.DefaultGruppe != check.DefaultGruppe
                    || saved.WochenStunden != check.WochenStunden
                    || saved.Wunschdienste != check.Wunschdienste
                    || saved.KindFreieZeit != check.KindFreieZeit)
                    return true;

                if (saved.NichtDaZeiten.Count != check.NichtDaZeiten.Count
                    || saved.NichtDaZeiten.Intersect(check.NichtDaZeiten).Count()!= saved.NichtDaZeiten.Count)
                    return true;
            }

            return false;
        }
        
    }
}
