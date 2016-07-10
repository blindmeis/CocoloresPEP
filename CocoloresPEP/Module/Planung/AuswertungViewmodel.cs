using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;

namespace CocoloresPEP.Module.Planung
{
    public class AuswertungViewmodel : ViewmodelBase
    {
        public AuswertungViewmodel()
        {
            Auswertungen = new List<ArbeitstagAuswertung>();
        }
        public List<ArbeitstagAuswertung> Auswertungen { get; set; }
    }

    public class ArbeitstagAuswertung : ViewmodelBase
    {
        public ArbeitstagAuswertung()
        {
            Messages = new List<ValidationMessage>();
        }
        public string Wochentag { get; set; }

        public List<ValidationMessage> Messages { get; set; }
    }

    public class ValidationMessage : ViewmodelBase
    {
        public string Message { get; set; }
    }
}
