using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace CocoloresPEP.Common.WpfCore.Behaviors
{
     public class ExpanderCommandOnExpandedBehavior : Behavior<Expander>
    {
        public static readonly DependencyProperty CommandOnExpandedProperty =
           DependencyProperty.Register("CommandOnExpanded", typeof(ICommand), typeof(ExpanderCommandOnExpandedBehavior));


        public ICommand CommandOnExpanded
        {
            get { return (ICommand)GetValue(CommandOnExpandedProperty); }
            set { SetValue(CommandOnExpandedProperty, value); }
        }
        protected override void OnAttached()
         {
             base.OnAttached();
            AssociatedObject.Expanded += AssociatedObject_Expanded;
         }

         protected override void OnDetaching()
         {
             base.OnDetaching();
            AssociatedObject.Expanded -= AssociatedObject_Expanded;
        }

         private void AssociatedObject_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            if(AssociatedObject.IsExpanded)
                CommandOnExpanded?.Execute(null);
        }
    }
}
