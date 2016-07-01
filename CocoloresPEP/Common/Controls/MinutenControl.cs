using System.Collections.Generic;
using System.Windows.Controls;
using CocoloresPEP.Common.WpfCore.Controls;

namespace CocoloresPEP.Common.Controls
{
    public class MinutenControl : ComboBoxWithoutDropdown
    {
        public MinutenControl()
        {
            var stunden = new Dictionary<string, int>();
            for (int i = 0; i < 60; i++)
            {
                stunden.Add(i.ToString().PadLeft(2, '0'), i);
            }

            this.ItemsSource = stunden;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var txt = (TextBox)this.Template.FindName("PART_EditableTextBox", this);
            if (txt != null)
                txt.Width = 24;
        }
    }
}
