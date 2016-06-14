using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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
        private ICollectionView _view;
        private Common.Entities.Mitarbeiter _selectedMitarbeiter;

        private readonly Lazy<DelegateCommand> _lazyCreateMitarbeiterCommand;
        private readonly Lazy<DelegateCommand> _lazySaveMitarbeiterCommand;
        private readonly Lazy<DelegateCommand> _lazyDeleteMitarbeiterCommand;

        public MitarbeiterManager()
        {
            _serializationService = new SerializationService();

            MitarbeiterCollection = new ObservableCollection<Common.Entities.Mitarbeiter>(_serializationService.ReadMitarbeiterListe());

            _view = CollectionViewSource.GetDefaultView(MitarbeiterCollection);
            _view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            Wunschdienste = new WunschdiensteCollection();
            GruppenDefaultDictionary = new Dictionary<string, SollTyp>
            {
                {" ", SollTyp.None},
                {"Blau", SollTyp.Blau},
                {"Rot", SollTyp.Rot},
                {"Grün", SollTyp.Gruen},
                {"Nest", SollTyp.Nest}
            };

            _lazyCreateMitarbeiterCommand = new Lazy<DelegateCommand>(()=> new DelegateCommand(CreateMitarbeiterCommandExecute, CanCreateMitarbeiterCommandExecute));
            _lazySaveMitarbeiterCommand = new Lazy<DelegateCommand>(()=> new DelegateCommand(SaveMitarbeiterCommandExecute, CanSaveMitarbeiterCommandExecute));
            _lazyDeleteMitarbeiterCommand = new Lazy<DelegateCommand>(()=> new DelegateCommand(DeleteMitarbeiterCommandExecute, CanDeleteMitarbeiterCommandExecute));
        }

     

        public ObservableCollection<Common.Entities.Mitarbeiter> MitarbeiterCollection { get; private set; }

        public Dictionary<string, SollTyp> GruppenDefaultDictionary { get; set; }

        public WunschdiensteCollection Wunschdienste { get; set; }

        public Common.Entities.Mitarbeiter SelectedMitarbeiter
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

            SelectedMitarbeiter = new Common.Entities.Mitarbeiter() {Name = "Neu"};
            MitarbeiterCollection.Add(SelectedMitarbeiter);
        }

        #endregion

        #region SaveMitarbeiterCommand

        public ICommand SaveMitarbeiterCommand { get { return _lazySaveMitarbeiterCommand.Value; } }

        private bool CanSaveMitarbeiterCommandExecute()
        {
            return true;
        }

        private async void SaveMitarbeiterCommandExecute()
        {
            if(!CanSaveMitarbeiterCommandExecute())
                return;

            try
            {
               await Task.Run( ()=> _serializationService.WriteMitarbeiterListe(MitarbeiterCollection.ToList()));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern der Daten. " + Environment.NewLine + ex.GetAllErrorMessages());
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
    }
}
