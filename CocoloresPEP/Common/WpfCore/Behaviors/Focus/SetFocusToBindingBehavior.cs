using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace CocoloresPEP.Common.WpfCore.Behaviors.Focus
{
    public class SetFocusToBindingBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty SetFocusToBindingPathProperty =
          DependencyProperty.Register("SetFocusToBindingPath", typeof(string), typeof(SetFocusToBindingBehavior), new FrameworkPropertyMetadata(SetFocusToBindingPathPropertyChanged));

        public string SetFocusToBindingPath
        {
            get { return (string)GetValue(SetFocusToBindingPathProperty); }
            set { SetValue(SetFocusToBindingPathProperty, value); }
        }

        private static void SetFocusToBindingPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as SetFocusToBindingBehavior;
            var bindingpath = (e.NewValue as string) ?? string.Empty;

            if (behavior == null || string.IsNullOrWhiteSpace(bindingpath))
                return;

            behavior.SetFocusTo(behavior.AssociatedObject, bindingpath, null);
            //wenn alles vorbei ist dann binding path zurücksetzen auf string.empty, 
            //ansonsten springt PropertyChangedCallback nicht mehr an wenn wieder zum gleichen Propertyname der Focus gesetzt werden soll
            behavior.SetFocusToBindingPath = string.Empty;
        }

        public static readonly DependencyProperty SetFocusToBindingPathWithStartDependencyObjectProperty =
          DependencyProperty.Register("SetFocusToBindingPathWithStartDependencyObject", typeof(Tuple<string, object>), typeof(SetFocusToBindingBehavior), new FrameworkPropertyMetadata(SetFocusToBindingPathWithStartDependencyObjectPropertyChanged));

        public Tuple<string, object> SetFocusToBindingPathWithStartDependencyObject
        {
            get { return (Tuple<string, object>)GetValue(SetFocusToBindingPathWithStartDependencyObjectProperty); }
            set { SetValue(SetFocusToBindingPathWithStartDependencyObjectProperty, value); }
        }

        private static void SetFocusToBindingPathWithStartDependencyObjectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as SetFocusToBindingBehavior;
            var bindingpathTuple = (e.NewValue as Tuple<string, object>) ?? new Tuple<string, object>("", null);

            if (behavior == null || string.IsNullOrWhiteSpace(bindingpathTuple.Item1))
                return;

            behavior.SetFocusTo(behavior.AssociatedObject, bindingpathTuple.Item1, bindingpathTuple.Item2 as DependencyObject);
            //wenn alles vorbei ist dann binding path zurücksetzen auf string.empty, 
            //ansonsten springt PropertyChangedCallback nicht mehr an wenn wieder zum gleichen Propertyname der Focus gesetzt werden soll
            behavior.SetFocusToBindingPath = string.Empty;
        }

        private void SetFocusTo(DependencyObject obj, string bindingpath, DependencyObject ersteSucheObject)
        {
            if (string.IsNullOrWhiteSpace(bindingpath))
                return;

            DependencyObject ctrl = null;

            if (ersteSucheObject != null)
            {
                ctrl = CheckForBinding(ersteSucheObject, bindingpath);
            }

            //erstmal vom aktuellen fokusierten Element aus gucken
            if (ctrl == null || !(ctrl is IInputElement))
                ctrl = CheckForBinding(Keyboard.FocusedElement as DependencyObject, bindingpath);

            //wenn nix gefunden dann nochmal von ganz oben alles durchschauen
            if (ctrl == null || !(ctrl is IInputElement))
                ctrl = CheckForBinding(obj, bindingpath);

            //Tjo Pech nix gefunden
            if (ctrl == null || !(ctrl is IInputElement))
                return;

            var iie = (IInputElement)ctrl;

            ctrl.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!iie.Focus())
                {
                    //zb. bei IsEditable=true Comboboxen funzt .Focus() nicht, daher Keyboard.Focus probieren
                    Keyboard.Focus(iie);

                    if (!iie.IsKeyboardFocusWithin)
                    {
                        Debug.WriteLine("Focus konnte nicht auf Bindingpath: " + bindingpath + " gesetzt werden.");
                        var tNext = new TraversalRequest(FocusNavigationDirection.Next);
                        var uie = iie as UIElement;

                        if (uie != null)
                        {
                            uie.MoveFocus(tNext);
                        }
                    }
                }
            }), DispatcherPriority.Background);
        }

        public string BindingName { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObjectLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObjectLoaded;
        }

        private void AssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            SetFocusTo(AssociatedObject, this.BindingName, null);
        }

        private DependencyObject CheckForBinding(DependencyObject obj, string bindingpath)
        {
            if (obj == null || string.IsNullOrWhiteSpace(bindingpath))
                return null;

            var properties = TypeDescriptor.GetProperties(obj, new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.All) });

            if (obj is IInputElement && ((IInputElement)obj).Focusable)
            {
                foreach (PropertyDescriptor property in properties)
                {
                    var prop = DependencyPropertyDescriptor.FromProperty(property);

                    if (prop == null) continue;

                    var ex = BindingOperations.GetBindingExpression(obj, prop.DependencyProperty);
                    if (ex == null) continue;

                    Debug.WriteLine("Control Bindingpath: {0} == {1} --> {2} (Control: {3}.{4})",
                        ex.ParentBinding.Path.Path, bindingpath, ex.ParentBinding.Path.Path == bindingpath, obj, property.Name);

                    if (ex.ParentBinding.Path.Path == bindingpath)
                        return obj;
                }
            }

            //TabControl TabItems sind nicht zwangsläufig vollständig im Visualtree
            //daher für jedes TabItem erstmal UpdateLayout() machen
            if (obj is TabControl)
            {
                var tab = obj as TabControl;

                foreach (TabItem tt in tab.Items.OfType<TabItem>())
                {
                    if (tt == null)
                        continue;

                    tt.UpdateLayout();
                    var result = CheckForBinding(tt, bindingpath);
                    if (result != null)
                        return result;
                }
            }

            //ich hatte einen Absturz wenn ein Hyperlink bei VisualTreeHelper.GetChildrenCount(obj) übergeben wird 
            //daher das folgende if
            if (obj is Visual || obj is Visual3D)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    var result = CheckForBinding(VisualTreeHelper.GetChild(obj, i), bindingpath);
                    if (result != null)
                        return result;
                }
            }



            return null;
        }
    }
}
