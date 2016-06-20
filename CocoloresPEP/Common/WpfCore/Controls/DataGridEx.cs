using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CocoloresPEP.Common.WpfCore.Controls
{
    public class DataGridEx : DataGrid
    {
        public static readonly DependencyProperty SelectedItemExProperty =
            DependencyProperty.Register("SelectedItemEx", typeof(object),typeof(DataGridEx));


        public object SelectedItemEx
        {
            get { return GetValue(SelectedItemExProperty); }
            set { SetValue(SelectedItemExProperty, value); }
        }

        protected override void OnSelectedCellsChanged(SelectedCellsChangedEventArgs e)
        {
            base.OnSelectedCellsChanged(e);

            SelectedItemEx = this.CurrentCell.Item;
        }
    }
}
