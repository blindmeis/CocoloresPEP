﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Itenso.TimePeriod;

namespace CocoloresPEP.Common.Entities
{
    public class Arbeitstag 
    {
        private TimeRange _grossteam;

        public Arbeitstag()
        {
            
        }
        public Arbeitstag(DateTime dt)
        {
            Datum = dt;
            Planzeiten = new ObservableCollection<PlanItem>();

            Grossteam = new TimeRange(new DateTime(Datum.Year, Datum.Month, Datum.Day, 15, 30, 0), new TimeSpan(2, 0, 0));
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
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, 7, 0, 0); }
        }

        public DateTime AchtUhrDienst
        {
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, 8, 0, 0); }
        }
        
        public DateTime NeunUhrDienst
        {
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, 9, 0, 0); }
        }

        public DateTime ZehnUhrDienst
        {
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, 10, 0, 0); }
        }

        public DateTime KernzeitGruppeStart
        {
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, 8, 30, 0); }
        }

        public DateTime KernzeitGruppeEnde
        {
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, 15, 30, 0); }
        }

        public DateTime SpätdienstEnde
        {
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, 17, 15, 0); }
        }

        public DateTime FrühdienstFsj
        {
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, 7, 30, 0); }
        }

        public DateTime SpätdienstEndeFsj
        {
            get { return new DateTime(Datum.Year, Datum.Month, Datum.Day, 16, 30, 0); }
        }

        public TimeRange Grossteam
        {
            get { return _grossteam; }
            set { _grossteam = value; }
        }

        public int GrossteamStundeVon {get { return Grossteam.Start.Hour; } }
        public int GrossteamStundeBis {get { return Grossteam.End.Hour; } }
        public int GrossteamMinuteVon {get { return Grossteam.Start.Minute; } }
        public int GrossteamMinuteBis {get { return Grossteam.End.Minute; } }

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
    }
}
