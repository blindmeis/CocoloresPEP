using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Entities
{
    public class PlanItem
    {
        public DateTime Startzeit { get; set; }

        public short QuarterTicks { get; set; }

        public short BreakTicks
        {
            get
            {
                if (QuarterTicks > 4*6)
                    return 2;
                return 0;
            }
        }

        public short AllTicks
        {
            get { return (short) (QuarterTicks + BreakTicks); }
        }

        public Mitarbeiter ErledigtDurch { get; set; }
        public GruppenTyp Gruppe { get; set; }
        public DienstTyp Dienst { get; set; }

        public bool ObGruppenDienst
        {
            get
            {
                return (Dienst & DienstTyp.Frühdienst) == DienstTyp.Frühdienst
                       || (Dienst & DienstTyp.AchtUhrDienst) == DienstTyp.AchtUhrDienst
                       || (Dienst & DienstTyp.KernzeitStartDienst) == DienstTyp.KernzeitStartDienst
                       || (Dienst & DienstTyp.NeunUhrDienst) == DienstTyp.NeunUhrDienst
                       || (Dienst & DienstTyp.ZehnUhrDienst) == DienstTyp.ZehnUhrDienst
                       || (Dienst & DienstTyp.KernzeitEndeDienst) == DienstTyp.KernzeitEndeDienst
                       || (Dienst & DienstTyp.SpätdienstEnde) == DienstTyp.SpätdienstEnde;


            }
        }
    }
}
