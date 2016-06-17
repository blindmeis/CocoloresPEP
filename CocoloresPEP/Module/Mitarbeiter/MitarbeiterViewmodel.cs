﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;

namespace CocoloresPEP.Module.Mitarbeiter
{
    public class MitarbeiterViewmodel : ViewmodelBase
    {
        private DienstTyp _wunschdienste;
        private string _name;
        private GruppenTyp _defaultGruppe;
        private bool _isHelfer;

        public MitarbeiterViewmodel()
        {
            NichtDaZeiten = new ObservableCollection<DateTime>();
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public decimal WochenStunden { get; set; }

        public decimal SaldoWochenStunden { get; set; }
        
        public ObservableCollection<DateTime> NichtDaZeiten { get; set; }

        public GruppenTyp DefaultGruppe
        {
            get { return _defaultGruppe; }
            set
            {
                _defaultGruppe = value;
                OnPropertyChanged();
            }
        }

        public DienstTyp Wunschdienste
        {
            get { return _wunschdienste; }
            set
            {
                _wunschdienste = value;
                OnPropertyChanged();
            }
        }

        public bool IsHelfer
        {
            get { return _isHelfer; }
            set
            {
                _isHelfer = value;
                OnPropertyChanged();
            }
        }
    }
}
