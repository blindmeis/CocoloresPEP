using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Entities
{
    public class Arbeitstag
    {
        public Arbeitstag()
        {
            
        }
        public Arbeitstag(DateTime dt)
        {
            Datum = dt;
            Planzeiten = new ObservableCollection<PlanItem>();
        }

        public DateTime Datum { get;  set; }
        
        public ObservableCollection<PlanItem> Planzeiten { get;  set; }

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
    }
}
