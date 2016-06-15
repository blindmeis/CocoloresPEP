using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;

namespace CocoloresPEP.Module.Mitarbeiter
{
    public class MitarbeiterViewmodel : ViewmodelBase
    {
        private SollTyp _wunschdienste;
        private string _name;
        private SollTyp _defaultGruppe;

        public MitarbeiterViewmodel()
        {
            NichtDaZeiten = new ObservableCollection<DateTime>();
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public decimal WochenStunden { get; set; }

        public decimal SaldoWochenStunden { get; set; }
        
        public ObservableCollection<DateTime> NichtDaZeiten { get; set; }

        public SollTyp DefaultGruppe
        {
            get { return _defaultGruppe; }
            set
            {
                _defaultGruppe = value;
                OnPropertyChanged();
            }
        }

        public SollTyp Wunschdienste
        {
            get { return _wunschdienste; }
            set
            {
                _wunschdienste = value;
                OnPropertyChanged();
            }
        }
    }
}
