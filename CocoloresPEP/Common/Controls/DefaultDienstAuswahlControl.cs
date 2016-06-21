using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.Extensions;

namespace CocoloresPEP.Common.Controls
{
    public class DefaultDienstAuswahlControl : ComboBox
    {
        public DefaultDienstAuswahlControl()
        {
            this.ItemsSource = new Dictionary<string, DienstTyp>
            {
                {DienstTyp.Frühdienst.GetDisplayname(), DienstTyp.Frühdienst},
                {DienstTyp.AchtUhrDienst.GetDisplayname(), DienstTyp.AchtUhrDienst},
                {DienstTyp.KernzeitStartDienst.GetDisplayname(), DienstTyp.KernzeitStartDienst},
                {DienstTyp.NeunUhrDienst.GetDisplayname(), DienstTyp.NeunUhrDienst},
                {DienstTyp.ZehnUhrDienst.GetDisplayname(), DienstTyp.ZehnUhrDienst},
                {DienstTyp.KernzeitEndeDienst.GetDisplayname(), DienstTyp.KernzeitEndeDienst},
                {DienstTyp.SpätdienstEnde.GetDisplayname(), DienstTyp.SpätdienstEnde},
            };
        }
    }
}
