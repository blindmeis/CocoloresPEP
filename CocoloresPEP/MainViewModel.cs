using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common;
using CocoloresPEP.Module.Mitarbeiter;
using CocoloresPEP.Module.Planung;

namespace CocoloresPEP
{
    public class MainViewModel : ViewmodelBase
    {
        public MainViewModel()
        {
            MitarbeiterVm = new MitarbeiterManager();
            PlanungVm = new PlanungManager();
        }

        public MitarbeiterManager MitarbeiterVm { get; set; }
        public PlanungManager PlanungVm { get; set; }
    }
}
