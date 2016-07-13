using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Itenso.TimePeriod;

namespace CocoloresPEP.Common.Entities
{
    public class PlanItem
    {
        [IgnoreDataMember]
        public Arbeitstag Arbeitstag { get; set; }
        public TimeRange Zeitraum { get; set; }
        public Mitarbeiter ErledigtDurch { get; set; }
        public GruppenTyp Gruppe { get; set; }
        public Themen Thema { get; set; }
        public DienstTyp Dienst { get; set; }
        public bool HatGrossteam { get; set; }

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
                       || (Dienst & DienstTyp.SechszehnUhrDienst) == DienstTyp.SechszehnUhrDienst
                       || (Dienst & DienstTyp.SpätdienstEnde) == DienstTyp.SpätdienstEnde
                       || (Dienst & DienstTyp.FsjFrühdienst) == DienstTyp.FsjFrühdienst
                       || (Dienst & DienstTyp.FsjSpätdienst) == DienstTyp.FsjSpätdienst;


            }
        }
    }
}
