using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common;
using CocoloresPEP.Module.Mitarbeiter;

namespace CocoloresPEP
{
    public class MainViewModel : ViewmodelBase
    {
        public MainViewModel()
        {
            MitarbeiterVm = new MitarbeiterManager();
        }

        public MitarbeiterManager MitarbeiterVm { get; set; }
    }
}
