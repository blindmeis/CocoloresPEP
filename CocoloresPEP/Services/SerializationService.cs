using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.Extensions;

namespace CocoloresPEP.Services
{
    public class SerializationService
    {
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
                var daten = ObjectExtensions.FromFile<List<Mitarbeiter>>(PathMitarbeiter);
                return daten ?? new List<Mitarbeiter>();
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Lesen der Mitarbeiterliste.", ex);
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
    }
}
