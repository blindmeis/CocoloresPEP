using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.Extensions;

namespace CocoloresPEP.Services
{
    public class SerializationService
    {
        #region Mitarbeiter
        public void WriteMitarbeiterListe(IList<Mitarbeiter> mitarbeiters)
        {
            try
            {
                mitarbeiters.ToFile<List<Mitarbeiter>>(PathMitarbeiter);
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Speichern der Mitarbeiterliste.", ex);
            }
        }

        public IList<Mitarbeiter> ReadMitarbeiterListe()
        {
            try
            {
                var fi = new FileInfo(PathMitarbeiter);

                if (fi.Exists)
                {
                    var daten = ObjectExtensions.FromFile<List<Mitarbeiter>>(PathMitarbeiter);
                    return daten ?? new List<Mitarbeiter>();
                }

                return new List<Mitarbeiter>();
            }
            catch (Exception ex)
            {
                // throw new Exception("Fehler beim Lesen der Mitarbeiterliste.", ex);
                return null;
            }
        }

        private static string PathMitarbeiter
        {
            get
            {
                var path = ConfigurationManager.AppSettings["PfadMitarbeiter"];
                return path;
            }
        }
        #endregion

        #region Planung
        public void WritePlanungListe(IList<Arbeitswoche> planungen)
        {
            try
            {
                planungen.ToFile<List<Arbeitswoche>>(PfadPlanungen);
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Speichern der Planungsliste.", ex);
            }
        }

        public IList<Arbeitswoche> ReadPlanungListe()
        {
            try
            {
                var fi = new FileInfo(PfadPlanungen);

                if (fi.Exists)
                {
                    var daten = ObjectExtensions.FromFile<List<Arbeitswoche>>(PfadPlanungen);
                    return daten ?? new List<Arbeitswoche>();
                }

                return new List<Arbeitswoche>(); 
               
            }
            catch (Exception ex)
            {
                //throw new Exception("Fehler beim Lesen der Planungsliste.", ex);
                return null;
            }
        }
        private static string PfadPlanungen
        {
            get
            {
                var path = ConfigurationManager.AppSettings["PfadPlanungen"];
                return path;
            }
        }

        #endregion
    }
}
