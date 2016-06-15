using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common.Entities;

namespace CocoloresPEP.Module.Planung
{
    public class PlanungswochePreviewViewmodel
    {
        public Common.Entities.Mitarbeiter Mitarbeiter { get; set; }

        public PlanungstagViewmodel Montag { get; set; }
        public PlanungstagViewmodel Dienstag { get; set; }
        public PlanungstagViewmodel Mittwoch { get; set; }
        public PlanungstagViewmodel Donnerstag { get; set; }
        public PlanungstagViewmodel Freitag { get; set; }
    }
}
