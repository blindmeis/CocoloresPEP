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
                var path = ConfigurationManager.AppSettings["PfadMitarbeiter"];

                mitarbeiters.ToFile<List<Mitarbeiter>>(path);
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Speichern der Mitarbeiterliste.", ex);
            }
        }
    }
}
