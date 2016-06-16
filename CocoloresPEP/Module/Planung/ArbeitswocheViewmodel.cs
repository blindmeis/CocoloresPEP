using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common;
using CocoloresPEP.Module.Mitarbeiter;

namespace CocoloresPEP.Module.Planung
{
    public class ArbeitswocheViewmodel : ViewmodelBase
    {
        public ArbeitswocheViewmodel(int jahr, int woche)
        {
            Jahr = jahr;
            KalenderWoche = woche;
            PlanungProMitarbeiterListe = new List<PlanungswocheMitarbeiterViewmodel>();
            Mitarbeiter = new List<MitarbeiterViewmodel>();
        }
        public int Jahr { get; set; }
        public int KalenderWoche { get; set; }

        public List<PlanungswocheMitarbeiterViewmodel> PlanungProMitarbeiterListe { get; set; }

        public List<MitarbeiterViewmodel> Mitarbeiter { get; set; }
    }
}
