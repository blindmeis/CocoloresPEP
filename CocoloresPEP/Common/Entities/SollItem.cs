using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Entities
{
    public class SollItem
    {
        public SollItem()
        {
            
        }
        public SollItem(string displayname, DateTime startzeit, short quarterticks, SollTyp typ)
        {
            Displayname = displayname;
            Startzeit = startzeit;
            QuarterTicks = quarterticks;
            Typ = typ;
        }
        public string Displayname { get; set; }

        public  DateTime Startzeit { get; set; }

        public short QuarterTicks { get; set; }

        public SollTyp Typ { get; set; }
      
    }
}
