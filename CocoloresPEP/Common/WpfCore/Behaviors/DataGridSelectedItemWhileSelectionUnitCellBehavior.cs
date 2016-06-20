using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace CocoloresPEP.Common.WpfCore.Behaviors
{
    public class DataGridSelectedItemWhileSelectionUnitCellBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty SelectedItemExProperty =
               DependencyProperty.Register("SelectedItemEx", typeof(object),
               typeof(DataGridSelectedItemWhileSelectionUnitCellBehavior));


        public object SelectedItemEx
        {
            get { return GetValue(SelectedItemExProperty); }
            set { SetValue(SelectedItemExProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.CurrentCellChanged += AssociatedObjectOnCurrentCellChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.CurrentCellChanged -= AssociatedObjectOnCurrentCellChanged;
        }

        private void AssociatedObjectOnCurrentCellChanged(object sender, EventArgs eventArgs)
        {
            SelectedItemEx = AssociatedObject.CurrentCell.Item;
        }
    }
}
