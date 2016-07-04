using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CocoloresPEP.Common.WpfCore.Controls
{
    public class ComboBoxWithoutDropdown : ComboBox
    {
        public ComboBoxWithoutDropdown()
        {
            this.Loaded += ComboBoxWithoutDropdown_Loaded;    
        }

        private void ComboBoxWithoutDropdown_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var tgl = Helper.FindChild<ToggleButton>(this);
            tgl.Visibility = Visibility.Collapsed;
            tgl.Width = 0;

            var parent = VisualTreeHelper.GetParent(tgl) as Grid;

            //für windows10
            if (parent?.ColumnDefinitions.Count == 2)
            {
                parent.ColumnDefinitions[1].MinWidth = 0;
            }
        }
    }
}
