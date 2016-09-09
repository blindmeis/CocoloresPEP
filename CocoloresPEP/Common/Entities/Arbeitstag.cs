using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CocoloresPEP.Common.Extensions;
using Itenso.TimePeriod;

namespace CocoloresPEP.Common.Entities
{
    public class Arbeitstag 
    {
        private TimeRange _grossteam;
        private TimeRange _kernzeitDoppelBesetzungRange;

        private static Tuple<int, int> ZeitFruehdienstStart { get; }
        private static Tuple<int, int> Zeit8UhrdienstStart { get; }
        private static Tuple<int, int> ZeitKernzeitdienstStart { get; }
        private static Tuple<int, int> Zeit9UhrdienstStart { get; }
        private static Tuple<int, int> Zeit10UhrdienstStart { get; }
        private static Tuple<int, int> ZeitKernzeitdienstEnde { get; }
        private static Tuple<int, int> Zeit16UhrdienstEnde { get; }
        private static Tuple<int, int> Zeit16UhrdienstBeiGrossteamEnde { get; }
        private static Tuple<int, int> ZeitSpaetdienstEnde { get; }
        private static Tuple<int, int> ZeitSpaetdienstBeiGrossteamEnde { get; }
        private static Tuple<int, int> ZeitFsjFruehdienstStart { get; }
        private static Tuple<int, int> ZeitFsjSpaetdienstEnde { get; }
        private static Tuple<int, int> ZeitFsjSpaetdienstBeiGrossteamEnde { get; }
        private static Tuple<int, int> ZeitFsjdienstStart { get; }

        /// <summary>
        ///    <add key = "ZeitFruehdienstStart" value="07:00" />
        ///    <add key = "Zeit8UhrdienstStart" value="08:00" />
        ///    <add key = "ZeitKernzeitdienstStart" value="08:30" />
        ///    <add key = "Zeit9UhrdienstStart" value="09:00" />
        ///    <add key = "Zeit10UhrdienstStart" value="10:00" />
        ///    <add key = "ZeitKernzeitdienstEnde" value="15:00" />
        ///    <add key = "Zeit16UhrdienstEnde" value="16:00" />
        ///    <add key = "Zeit16UhrdienstBeiGrossteamEnde" value="15:30" />
        ///    <add key = "ZeitSpaetdienstEnde" value="17:15" />
        ///    <add key = "ZeitSpaetdienstBeiGrossteamEnde" value="16:00" />
        ///    <add key = "ZeitFsjFruehdienstStart" value="07:30" />
        ///    <add key = "ZeitFsjSpaetdienstEnde" value="16:30" />
        ///    <add key = "ZeitFsjSpaetdienstBeiGrossteamEnde" value="17:00" />
        ///    <add key = "ZeitFsjdienstStart" value="08:30" />
        /// </summary>
        static Arbeitstag()
        {
            ZeitFruehdienstStart = ConfigurationManager.AppSettings["ZeitFruehdienstStart"]?.GetZeitTuple() ?? new Tuple<int, int>(7, 0);
            Zeit8UhrdienstStart = ConfigurationManager.AppSettings["Zeit8UhrdienstStart"]?.GetZeitTuple() ?? new Tuple<int, int>(8, 0);
            ZeitKernzeitdienstStart = ConfigurationManager.AppSettings["ZeitKernzeitdienstStart"]?.GetZeitTuple() ?? new Tuple<int, int>(8, 30);
            Zeit9UhrdienstStart = ConfigurationManager.AppSettings["Zeit9UhrdienstStart"]?.GetZeitTuple() ?? new Tuple<int, int>(9, 0);
            Zeit10UhrdienstStart = ConfigurationManager.AppSettings["Zeit10UhrdienstStart"]?.GetZeitTuple() ?? new Tuple<int, int>(10, 0);
            ZeitKernzeitdienstEnde = ConfigurationManager.AppSettings["ZeitKernzeitdienstEnde"]?.GetZeitTuple() ?? new Tuple<int, int>(15, 0);
            Zeit16UhrdienstEnde = ConfigurationManager.AppSettings["Zeit16UhrdienstEnde"]?.GetZeitTuple() ?? new Tuple<int, int>(16, 0);
            Zeit16UhrdienstBeiGrossteamEnde = ConfigurationManager.AppSettings["Zeit16UhrdienstBeiGrossteamEnde"]?.GetZeitTuple() ?? new Tuple<int, int>(15, 30);
            ZeitSpaetdienstEnde = ConfigurationManager.AppSettings["ZeitSpaetdienstEnde"]?.GetZeitTuple() ?? new Tuple<int, int>(17, 15);
            ZeitSpaetdienstBeiGrossteamEnde = ConfigurationManager.AppSettings["ZeitSpaetdienstBeiGrossteamEnde"]?.GetZeitTuple() ?? new Tuple<int, int>(16, 00);
            ZeitFsjFruehdienstStart = ConfigurationManager.AppSettings["ZeitFsjFruehdienstStart"]?.GetZeitTuple() ?? new Tuple<int, int>(7, 30);
            ZeitFsjSpaetdienstEnde = ConfigurationManager.AppSettings["ZeitFsjSpaetdienstEnde"]?.GetZeitTuple() ?? new Tuple<int, int>(16, 30);
            ZeitFsjSpaetdienstBeiGrossteamEnde = ConfigurationManager.AppSettings["ZeitFsjSpaetdienstBeiGrossteamEnde"]?.GetZeitTuple() ?? new Tuple<int, int>(17, 00);
            ZeitFsjdienstStart = ConfigurationManager.AppSettings["ZeitFsjdienstStart"]?.GetZeitTuple() ?? new Tuple<int, int>(8, 30);
        }
        public Arbeitstag()
        {
            
        }
        public Arbeitstag(DateTime dt)
        {
            Datum = dt;
            Planzeiten = new ObservableCollection<PlanItem>();

            Grossteam = GrossteamDefault;
            KernzeitDoppelBesetzungRange = KernzeitDoppelBesetzungRangeDefaut;
        }

        [OnDeserialized()]
        internal void OnSerializedMethod(StreamingContext context)
        {
            // Setting this as parent property for Child object
            foreach (var planItem in Planzeiten)
            {
                planItem.Arbeitstag = this;
            }
        }

        public DateTime Datum { get;  set; }
        
        public ObservableCollection<PlanItem> Planzeiten { get;  set; }

        public bool IsFeiertag { get; set; }
        public bool HasGrossteam { get; set; }

        public DateTime Frühdienst
        {
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, ZeitFruehdienstStart.Item1, ZeitFruehdienstStart.Item2, 0); }
        }

        public DateTime AchtUhrDienst
        {
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, Zeit8UhrdienstStart.Item1, Zeit8UhrdienstStart.Item2, 0); }
        }
        
        public DateTime NeunUhrDienst
        {
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, Zeit9UhrdienstStart.Item1, Zeit9UhrdienstStart.Item2, 0); }
        }

        public DateTime ZehnUhrDienst
        {
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, Zeit10UhrdienstStart.Item1, Zeit10UhrdienstStart.Item2, 0); }
        }

        public DateTime KernzeitGruppeStart
        {
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, ZeitKernzeitdienstStart.Item1, ZeitKernzeitdienstStart.Item2, 0); }
        }

        public DateTime KernzeitGruppeEnde
        {
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, ZeitKernzeitdienstEnde.Item1, ZeitKernzeitdienstEnde.Item2, 0); }
        }
        public DateTime SechzehnUhrDienst
        {
            get { return HasGrossteam
                    ? new DateTime(Datum.Year, Datum.Month, Datum.Day, Zeit16UhrdienstBeiGrossteamEnde.Item1, Zeit16UhrdienstBeiGrossteamEnde.Item2, 0)
                    : new DateTime(Datum.Year, Datum.Month, Datum.Day, Zeit16UhrdienstEnde.Item1, Zeit16UhrdienstEnde.Item2, 0); }
        }
        /// <summary>
        /// Ende Spätdienst in abhängigkeit vom Großteam
        /// </summary>
        public DateTime SpätdienstEnde
        {
            get { return HasGrossteam
                    ? new DateTime(Datum.Year, Datum.Month, Datum.Day, ZeitSpaetdienstBeiGrossteamEnde.Item1, ZeitSpaetdienstBeiGrossteamEnde.Item2, 0)
                    : new DateTime(Datum.Year, Datum.Month, Datum.Day, ZeitSpaetdienstEnde.Item1, ZeitSpaetdienstEnde.Item2, 0); }
        }

        public DateTime FrühdienstFsj
        {
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, ZeitFsjFruehdienstStart.Item1, ZeitFsjFruehdienstStart.Item2, 0); }
        }
        
        /// <summary>
        /// Ende Spätdienst in abhängigkeit vom Großteam
        /// </summary>
        public DateTime SpätdienstEndeFsj
        {
            get { return HasGrossteam 
                    ? new DateTime(Datum.Year, Datum.Month, Datum.Day, ZeitFsjSpaetdienstBeiGrossteamEnde.Item1, ZeitFsjSpaetdienstBeiGrossteamEnde.Item2, 0)
                    : new DateTime(Datum.Year, Datum.Month, Datum.Day, ZeitFsjSpaetdienstEnde.Item1, ZeitFsjSpaetdienstEnde.Item2, 0); }
        }

        public DateTime NormaldienstFsj
        {
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, ZeitFsjdienstStart.Item1, ZeitFsjdienstStart.Item2, 0); }
        }

        public TimeRange KernzeitBasisRange
        {
            get
            {
                return new TimeRange(KernzeitGruppeStart,KernzeitGruppeEnde);
            }
        }

        public TimeRange KernzeitDoppelBesetzungRangeDefaut
        { get
        {
            return new TimeRange(new DateTime(Datum.Year, Datum.Month, Datum.Day, 9, 0, 0), new TimeSpan(4, 0, 0));
        } }

        public TimeRange KernzeitDoppelBesetzungRange
        {
            get { return _kernzeitDoppelBesetzungRange ?? KernzeitDoppelBesetzungRangeDefaut; }
            set { _kernzeitDoppelBesetzungRange = value; }
        }

        public int KernzeitDoppelBesetzungStundeVon { get { return KernzeitDoppelBesetzungRange?.Start.Hour??0; } }
        public int KernzeitDoppelBesetzungStundeBis { get { return KernzeitDoppelBesetzungRange?.End.Hour ?? 0; } }
        public int KernzeitDoppelBesetzungMinuteVon { get { return KernzeitDoppelBesetzungRange?.Start.Minute ?? 0; } }
        public int KernzeitDoppelBesetzungMinuteBis { get { return KernzeitDoppelBesetzungRange?.End.Minute ?? 0; } }

        public TimeRange GrossteamDefault
        { get
        {
            return new TimeRange(new DateTime(Datum.Year, Datum.Month, Datum.Day, 15, 30, 0), new TimeSpan(2, 0, 0));
        } }   
        public TimeRange Grossteam
        {
            get { return _grossteam ?? GrossteamDefault; }
            set { _grossteam = value; }
        }

        public int GrossteamStundeVon {get { return Grossteam?.Start.Hour ?? 0; } }
        public int GrossteamStundeBis {get { return Grossteam?.End.Hour ?? 0; } }
        public int GrossteamMinuteVon {get { return Grossteam?.Start.Minute ?? 0; } }
        public int GrossteamMinuteBis {get { return Grossteam?.End.Minute ?? 0; } }

        public PlanItem EmptyPlanzeitOhneMitarbeiter
        {
            get
            {
                return new PlanItem()
                {
                    Arbeitstag = this,
                    Dienst = DienstTyp.None,
                    Zeitraum = new TimeRange(new DateTime(this.Datum.Year, this.Datum.Month, this.Datum.Day, 0, 0, 0), new TimeSpan(0, 0, 0)),

                };
            } }

        public string Wochentag
        {
            get
            {
                return $"{Datum.GetWochentagName()}";
            }
        }
    }
}
