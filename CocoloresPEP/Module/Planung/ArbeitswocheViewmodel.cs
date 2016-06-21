using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common;
using CocoloresPEP.Module.Mitarbeiter;

namespace CocoloresPEP.Module.Planung
{
    public class ArbeitswocheViewmodel : ViewmodelBase
    {
        private bool _isMontagFeiertag;
        private bool _isDienstagFeiertag;
        private bool _isMittwochFeiertag;
        private bool _isDonnerstagFeiertag;
        private bool _isFreitagFeiertag;

        public ArbeitswocheViewmodel(int jahr, int woche)
        {
            Jahr = jahr;
            KalenderWoche = woche;
            PlanungProMitarbeiterListe = new List<PlanungswocheMitarbeiterViewmodel>();
            Mitarbeiter = new List<MitarbeiterViewmodel>();
        }
        public int Jahr { get; set; }
        public int KalenderWoche { get; set; }

        public bool IsMontagFeiertag
        {
            get { return _isMontagFeiertag; }
            set
            {
                if (_isMontagFeiertag == value)
                    return;

                _isMontagFeiertag = value; 
                OnPropertyChanged();

                foreach (var l in PlanungProMitarbeiterListe)
                {
                    l.Montag.IsFeiertag = value;
                }
            }
        }
        public bool IsDienstagFeiertag
        {
            get { return _isDienstagFeiertag; }
            set
            {
                if (_isDienstagFeiertag == value)
                    return;

                _isDienstagFeiertag = value;
                OnPropertyChanged();

                foreach (var l in PlanungProMitarbeiterListe)
                {
                    l.Dienstag.IsFeiertag = value;
                }
            }
        }

        public bool IsMittwochFeiertag
        {
            get { return _isMittwochFeiertag; }
            set
            {
                if (_isMittwochFeiertag == value)
                    return;

                _isMittwochFeiertag = value;
                OnPropertyChanged();

                foreach (var l in PlanungProMitarbeiterListe)
                {
                    l.Mittwoch.IsFeiertag = value;
                }
            }
        }

        public bool IsDonnerstagFeiertag
        {
            get { return _isDonnerstagFeiertag; }
            set
            {
                if (_isDonnerstagFeiertag == value)
                    return;

                _isDonnerstagFeiertag = value;
                OnPropertyChanged();

                foreach (var l in PlanungProMitarbeiterListe)
                {
                    l.Donnerstag.IsFeiertag = value;
                }
            }
        }

        public bool IsFreitagFeiertag
        {
            get { return _isFreitagFeiertag; }
            set
            {
                if (_isFreitagFeiertag == value)
                    return;

                _isFreitagFeiertag = value;
                OnPropertyChanged();

                foreach (var l in PlanungProMitarbeiterListe)
                {
                    l.Freitag.IsFeiertag = value;
                }
                
            }
        }

        public List<PlanungswocheMitarbeiterViewmodel> PlanungProMitarbeiterListe { get; set; }

        public List<MitarbeiterViewmodel> Mitarbeiter { get; set; }
    }
}
