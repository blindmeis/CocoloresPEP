﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;

namespace CocoloresPEP.Module.Planung
{
    public class AuswertungViewmodel : ViewmodelBase
    {
        public AuswertungViewmodel()
        {
            Auswertungen = new List<ArbeitstagAuswertung>();
        }
        public List<ArbeitstagAuswertung> Auswertungen { get; set; }
    }

    public class ArbeitstagAuswertung : ViewmodelBase
    {
        public ArbeitstagAuswertung()
        {
            ValidierungsItems = new List<ValidierungsItem>();
        }
        public string Wochentag { get; set; }

        public List<ValidierungsItem> ValidierungsItems { get; set; }
    }

    public class ValidierungsItem : ViewmodelBase
    {
        public ValidierungsItem()
        {
            Messages = new List<ValidationMessage>();
        }

        public string Displayname { get; set; }

        public List<ValidationMessage> Messages { get; set; }
    }

    public class ValidationMessage : ViewmodelBase
    {
        public string Message { get; set; }
    }
}
