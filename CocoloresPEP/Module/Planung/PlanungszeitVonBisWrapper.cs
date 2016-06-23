using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common;

namespace CocoloresPEP.Module.Planung
{
    public class PlanungszeitVonBisWrapper : ViewmodelBase
    {
        private int _stundeVon;
        private int _stundeBis;
        private int _minuteVon;
        private int _minuteBis;

        public int StundeVon
        {
            get { return _stundeVon; }
            set
            {
                _stundeVon = value;
                OnPropertyChanged();
            }
        }

        public int StundeBis
        {
            get { return _stundeBis; }
            set
            {
                _stundeBis = value;
                OnPropertyChanged();
            }
        }

        public int MinuteVon
        {
            get { return _minuteVon; }
            set
            {
                _minuteVon = value;
                OnPropertyChanged();
            }
        }

        public int MinuteBis
        {
            get { return _minuteBis; }
            set
            {
                _minuteBis = value; 
                OnPropertyChanged();
            }
        }
    }
}
