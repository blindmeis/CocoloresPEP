using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.WpfCore.Commanding;
using CocoloresPEP.Services;

namespace CocoloresPEP.Module.Mitarbeiter
{
    public class MitarbeiterManager : ViewmodelBase
    {
        private readonly SerializationService _serializationService;
        private ICollectionView _view;
        private Lazy<DelegateCommand> _lazyCreateMitarbeiterCommand; 
        private Common.Entities.Mitarbeiter _selectedMitarbeiter;

        public MitarbeiterManager()
        {
            _serializationService = new SerializationService();

            MitarbeiterCollection = new ObservableCollection<Common.Entities.Mitarbeiter>(_serializationService.ReadMitarbeiterListe());
            _view = CollectionViewSource.GetDefaultView(MitarbeiterCollection);
            _view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            GruppenDefaultDictionary = new Dictionary<string, SollTyp>
            {
                {" ", SollTyp.None},
                {"Blau", SollTyp.Blau},
                {"Rot", SollTyp.Rot},
                {"Grün", SollTyp.Gruen},
                {"Nest", SollTyp.Nest}
            };

            _lazyCreateMitarbeiterCommand = new Lazy<DelegateCommand>(()=> new DelegateCommand(CreateMitarbeiterCommandExecute, CanCreateMitarbeiterCommandExecute));
        }

        public ObservableCollection<Common.Entities.Mitarbeiter> MitarbeiterCollection { get; private set; }

        public Dictionary<string, SollTyp> GruppenDefaultDictionary { get; set; } 

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

            SelectedMitarbeiter = new Common.Entities.Mitarbeiter();
        }

        #endregion
    }
}
