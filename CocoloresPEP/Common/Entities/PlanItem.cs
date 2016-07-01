using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CocoloresPEP.Common.Entities
{
    public class PlanItem
    {
        [IgnoreDataMember]
        public Arbeitstag Arbeitstag { get; set; }
        public DateTime Startzeit { get; set; }

        /// <summary>
        /// Sollte nur per "Hand" gesetzt werden
        /// </summary>
        public DateTime? Endzeit { get; set; }

        public short QuarterTicks { get; set; }

        public short BreakTicks
        {
            get
            {
                if (QuarterTicks * 15 > 540)
                    return 3;

                if (QuarterTicks * 15 > 360)
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

        public int DauerInMinuten { get { return (int) (GetEndzeit() - Startzeit).TotalMinutes; } }

        public DateTime GetEndzeit()
        {
            if (Endzeit.HasValue)
                return Endzeit.Value;

            return Startzeit.AddMinutes(15*AllTicks);
        }

        public int ArbeitszeitOhnePauseInMinuten()
        {
            var minuten = ArbeitszeitMitPauseInMinuten();

            if (minuten > 540)
                return minuten - 45;
            if (minuten > 360)
                return minuten -30;

            return minuten;
        }

        public int ArbeitszeitMitPauseInMinuten()
        {
            var start = Startzeit;
            var endzeit = GetEndzeit();

            var minuten = (int)(endzeit - start).TotalMinutes;

            return minuten;
        }
    }
}
