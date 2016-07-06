using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Module.Mitarbeiter;
using Itenso.TimePeriod;

namespace CocoloresPEP.Module.Planung
{
    public class ArbeitswocheViewmodel : ViewmodelBase
    {
        private bool _isMontagFeiertag;
        private bool _isDienstagFeiertag;
        private bool _isMittwochFeiertag;
        private bool _isDonnerstagFeiertag;
        private bool _isFreitagFeiertag;
        private bool _hasMontagGrossteam;
        private bool _hasDienstagGrossteam;
        private bool _hasMittwochGrossteam;
        private bool _hasDonnerstagGrossteam;
        private bool _hasFreitagGrossteam;

        public ArbeitswocheViewmodel(int jahr, int woche)
        {
            Jahr = jahr;
            KalenderWoche = woche;
            PlanungProMitarbeiterListe = new List<PlanungswocheMitarbeiterViewmodel>();
            Mitarbeiter = new List<MitarbeiterViewmodel>();
        }
        public int Jahr { get; set; }

        public int KalenderWoche { get; set; }

        public ArbeitstagWrapper Montag { get; set; }
        public ArbeitstagWrapper Dienstag { get; set; }
        public ArbeitstagWrapper Mittwoch { get; set; }
        public ArbeitstagWrapper Donnerstag { get; set; }
        public ArbeitstagWrapper Freitag { get; set; }

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

        public bool HasMontagGrossteam
        {
            get { return _hasMontagGrossteam; }
            set
            {
                if (_hasMontagGrossteam == value)
                    return;
                _hasMontagGrossteam = value;

                OnPropertyChanged();

                foreach (var l in PlanungProMitarbeiterListe)
                {
                    l.Montag.HasGrossteam = value;
                }
            }
        }

        public bool HasDienstagGrossteam
        {
            get { return _hasDienstagGrossteam; }
            set
            {
                if (_hasDienstagGrossteam == value)
                    return;
                _hasDienstagGrossteam = value;

                OnPropertyChanged();

                foreach (var l in PlanungProMitarbeiterListe)
                {
                    l.Dienstag.HasGrossteam = value;
                }
            }
        }

        public bool HasMittwochGrossteam
        {
            get { return _hasMittwochGrossteam; }
            set
            {
                if (_hasMittwochGrossteam == value)
                    return;
                _hasMittwochGrossteam = value;

                OnPropertyChanged();

                foreach (var l in PlanungProMitarbeiterListe)
                {
                    l.Mittwoch.HasGrossteam = value;
                }
            }
        }

        public bool HasDonnerstagGrossteam
        {
            get { return _hasDonnerstagGrossteam; }
            set
            {
                if (_hasDonnerstagGrossteam == value)
                    return;
                _hasDonnerstagGrossteam = value;

                OnPropertyChanged();

                foreach (var l in PlanungProMitarbeiterListe)
                {
                    l.Donnerstag.HasGrossteam = value;
                }
            }
        }

        public bool HasFreitagGrossteam
        {
            get { return _hasFreitagGrossteam; }
            set
            {
                if (_hasFreitagGrossteam == value)
                    return;
                _hasFreitagGrossteam = value;

                OnPropertyChanged();

                foreach (var l in PlanungProMitarbeiterListe)
                {
                    l.Freitag.HasGrossteam = value;
                }
            }
        }

        public List<PlanungswocheMitarbeiterViewmodel> PlanungProMitarbeiterListe { get; set; }

        public List<MitarbeiterViewmodel> Mitarbeiter { get; set; }
    }
}
