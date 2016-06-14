using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace CocoloresPEP.Common.Entities
{
    public class WunschdiensteCollection : ReadOnlyObservableCollection<WunschdienstWrapper>
    {
        public WunschdiensteCollection() : this(new ObservableCollection<WunschdienstWrapper>())
        {
            
        }
       
        private WunschdiensteCollection(ObservableCollection<WunschdienstWrapper> list) : base(list)
        {
            this.Items.Add(new WunschdienstWrapper() {Dienst = SollTyp.Frühdienst, Displayname = "Frühdienst"});
            this.Items.Add(new WunschdienstWrapper() {Dienst = SollTyp.AchtUhrDienst, Displayname = "08.00Uhr Dienst" });
            this.Items.Add(new WunschdienstWrapper() {Dienst = SollTyp.AchtUhr30Dienst, Displayname = "08.30Uhr Dienst" });
            this.Items.Add(new WunschdienstWrapper() {Dienst = SollTyp.NeunUhrDienst, Displayname = "09.00Uhr Dienst" });
            this.Items.Add(new WunschdienstWrapper() {Dienst = SollTyp.ZehnUhrDienst, Displayname = "10.00Uhr Dienst" });
            this.Items.Add(new WunschdienstWrapper() {Dienst = SollTyp.Spätdienst, Displayname = "Spätdienst" });
        }

    }

    public class WunschdienstWrapper : ViewmodelBase
    {
        private bool _isSelected;

        public string Displayname { get; set; }
        public SollTyp Dienst { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value; 
                OnPropertyChanged();
            }
        }
    }

    public class WunschdiensteBehavior : Behavior<ItemsControl> 
    {
      
    }
}
