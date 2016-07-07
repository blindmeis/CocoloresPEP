using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common.Entities;

namespace CocoloresPEP.Services
{
    public static class PlanzeitenValidationService
    {
        public static object ValidateArbeitswoche(this Arbeitswoche woche)
        {
            //Nicht weniger als 4h
            //1 Frühdienst
            //1 8Uhr Dienst
            //2 Spätdienst
            //116Uhr Dienst

            //Kernzeit 1MA 8:30-15:30
            //Kernzeit 2MA 9:00-15:00

            //1 FSJ 7:30

            // Großteam
            //2 bis 16:00
            //alle FSJ bis 17:00


            return null;
        }
    }
}
