using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Entities
{
    public class Ist
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
        public SollTyp Typ { get; set; }
    }
}
