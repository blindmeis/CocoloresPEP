using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using CocoloresPEP.Common;
using CocoloresPEP.Services;

namespace CocoloresPEP.Module.Mitarbeiter
{
    public class MitarbeiterManager : ViewmodelBase
    {
        private readonly SerializationService _serializationService;
        private ICollectionView _view;
        private Common.Entities.Mitarbeiter _selectedMitarbeiter;

        public MitarbeiterManager()
        {
            _serializationService = new SerializationService();
            MitarbeiterCollection = new ObservableCollection<Common.Entities.Mitarbeiter>(_serializationService.ReadMitarbeiterListe());
            _view = CollectionViewSource.GetDefaultView(MitarbeiterCollection);
            _view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }

        public ObservableCollection<Common.Entities.Mitarbeiter> MitarbeiterCollection { get; private set; }

        public Common.Entities.Mitarbeiter SelectedMitarbeiter
        {
            get { return _selectedMitarbeiter; }
            set
            {
                _selectedMitarbeiter = value;
                OnPropertyChanged();
            }
        }
    }
}
