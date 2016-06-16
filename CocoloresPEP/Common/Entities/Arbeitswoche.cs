using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Entities
{
    public class Arbeitswoche
    {
        public Arbeitswoche()
        {
            
        }
        public Arbeitswoche(int jahr, int woche)
        {
            Jahr = jahr;
            KalenderWoche = woche;
            Arbeitstage = new List<Arbeitstag>();
            Mitarbeiter = new List<Mitarbeiter>();
        }
        public int Jahr { get;  set; }
        public int KalenderWoche { get;  set; }

        public List<Arbeitstag> Arbeitstage { get;  set; } 

        public List<Mitarbeiter> Mitarbeiter { get; set; } 
    }
}
