using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.Extensions;

namespace CocoloresPEP.Common.Controls
{
    public class WunschdienstControl : ItemsControl
    {
        public WunschdienstControl()
        {
            var fd = new WunschdienstWrapper() {Dienst = DienstTyp.Frühdienst, Displayname = DienstTyp.Frühdienst.GetDisplayname() };
            var d8 = new WunschdienstWrapper() {Dienst = DienstTyp.AchtUhrDienst, Displayname = DienstTyp.AchtUhrDienst.GetDisplayname() };
            var d830 = new WunschdienstWrapper() {Dienst = DienstTyp.KernzeitStartDienst, Displayname = DienstTyp.KernzeitStartDienst.GetDisplayname() };
            var d9 = new WunschdienstWrapper() {Dienst = DienstTyp.NeunUhrDienst, Displayname = DienstTyp.NeunUhrDienst.GetDisplayname() };
            var d10 = new WunschdienstWrapper() {Dienst = DienstTyp.ZehnUhrDienst, Displayname = DienstTyp.ZehnUhrDienst.GetDisplayname() };
            var d1530 = new WunschdienstWrapper() {Dienst = DienstTyp.KernzeitEndeDienst, Displayname = DienstTyp.KernzeitEndeDienst.GetDisplayname() };
            var d16 = new WunschdienstWrapper() {Dienst = DienstTyp.SechszehnUhrDienst, Displayname = DienstTyp.SechszehnUhrDienst.GetDisplayname() };
            var sd = new WunschdienstWrapper() {Dienst = DienstTyp.SpätdienstEnde, Displayname = DienstTyp.SpätdienstEnde.GetDisplayname() };

            fd.PropertyChanged -= SelectionChanged;
            fd.PropertyChanged += SelectionChanged;
            d8.PropertyChanged -= SelectionChanged;
            d8.PropertyChanged += SelectionChanged;
            d830.PropertyChanged -= SelectionChanged;
            d830.PropertyChanged += SelectionChanged;
            d9.PropertyChanged -= SelectionChanged;
            d9.PropertyChanged += SelectionChanged;
            d10.PropertyChanged -= SelectionChanged;
            d10.PropertyChanged += SelectionChanged;
            d1530.PropertyChanged -= SelectionChanged;
            d1530.PropertyChanged += SelectionChanged;
            d16.PropertyChanged -= SelectionChanged;
            d16.PropertyChanged += SelectionChanged;
            sd.PropertyChanged -= SelectionChanged;
            sd.PropertyChanged += SelectionChanged;

            this.Items.Add(fd);
            this.Items.Add(d8);
            this.Items.Add(d830);
            this.Items.Add(d9);
            this.Items.Add(d10);
            this.Items.Add(d1530);
            this.Items.Add(d16);
            this.Items.Add(sd);

          
        }

        internal bool _handleIsSelectedChanged = true;
        private void SelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_handleIsSelectedChanged && e.PropertyName == "IsSelected")
            {
                if (Items.Cast<WunschdienstWrapper>().Any(x => x.IsSelected))
                {
                    var wd = Items.Cast<WunschdienstWrapper>().Where(x => x.IsSelected).ToList();
                    DienstTyp st = (DienstTyp) 0;
                    foreach (var item in wd)
                    {
                        st |= item.Dienst;
                    }
                    Wunschdienste = st;
                }
                else
                {
                    Wunschdienste = DienstTyp.None;
                }

            }
        }


        public static readonly DependencyProperty WunschdiensteProperty =
            DependencyProperty.Register("Wunschdienste", typeof(DienstTyp), typeof(WunschdienstControl), new FrameworkPropertyMetadata(OnWunschdienstChanged));

        public DienstTyp Wunschdienste
        {
            get { return (DienstTyp)GetValue(WunschdiensteProperty); }
            set { SetValue(WunschdiensteProperty, value); }
        }

        private static void OnWunschdienstChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wd = d as WunschdienstControl;

            if (e.NewValue is DienstTyp && wd !=null)
            {
                var neu = (DienstTyp)e.NewValue;

                //sync enum und Checkboxen
                wd._handleIsSelectedChanged = false;
                foreach (var item in wd.Items.Cast<WunschdienstWrapper>())
                {
                    if ((neu & item.Dienst) == item.Dienst)
                        item.IsSelected = true;
                    else
                        item.IsSelected = false;
                }
                wd._handleIsSelectedChanged = true;
            }
        }
    }

    public class WunschdienstWrapper : ViewmodelBase
    {
        private bool _isSelected;

        public string Displayname { get; set; }
        public DienstTyp Dienst { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;

                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }
}
