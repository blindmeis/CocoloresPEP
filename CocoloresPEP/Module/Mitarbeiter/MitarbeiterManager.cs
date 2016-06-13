using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common;

namespace CocoloresPEP.Module.Mitarbeiter
{
    public class MitarbeiterManager : ViewmodelBase
    {
        public MitarbeiterManager()
        {
            MitarbeitercCollection = new ObservableCollection<Common.Entities.Mitarbeiter>();
        }

        public ObservableCollection<Common.Entities.Mitarbeiter> MitarbeitercCollection { get; private set; } 
    }
}
