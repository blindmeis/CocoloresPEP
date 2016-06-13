using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using CocoloresPEP.Common.WpfCore.Adorner;

namespace CocoloresPEP.Common.WpfCore.Behaviors.Focus
{
    public class FocusBorderAdornerBehavior : AttachableForStyleBehavior<FrameworkElement, FocusBorderAdornerBehavior>
    {
        private BorderAdorner _adorner;

        public FocusBorderAdornerBehavior()
        {
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObjectOnLoaded;
            AssociatedObject.PreviewGotKeyboardFocus += AssociatedObject_GotKeyboardFocus;
            AssociatedObject.PreviewLostKeyboardFocus += AssociatedObjectOnLostKeyboardFocus;
            AssociatedObject.LostFocus += AssociatedObjectLostFocus;
        }

        private void AssociatedObjectLostFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("FocusBorderAdornerBehavior - AssociatedObjectLostFocus " + AssociatedObject);

            var iinputelement = AssociatedObject as IInputElement;
            var keyboardfoucusWithin = iinputelement != null && iinputelement.IsKeyboardFocusWithin;

            if (_adorner != null && !keyboardfoucusWithin)
                _adorner.Visibility = Visibility.Collapsed;
        }

        private void AssociatedObjectOnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs keyboardFocusChangedEventArgs)
        {
            Debug.WriteLine("FocusBorderAdornerBehavior - AssociatedObjectOnLostKeyboardFocus " + AssociatedObject);

            if (_adorner != null)
                _adorner.Visibility = Visibility.Collapsed;

        }

        void AssociatedObject_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            Debug.WriteLine("FocusBorderAdornerBehavior - AssociatedObject_GotKeyboardFocus " + AssociatedObject);

            CreateAdorner();

            if (_adorner != null)
                _adorner.Visibility = Visibility.Visible;
        }

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            CreateAdorner();
        }

        private void CreateAdorner()
        {
            if (_adorner != null)
                return;

            if (AssociatedObject == null)
                return;

            var adornerLayer = AdornerLayer.GetAdornerLayer(this.AssociatedObject);
            if (adornerLayer == null)
            {
                return;
            }

            _adorner = new BorderAdorner(this.AssociatedObject);
            _adorner.Visibility = Visibility.Collapsed;

            adornerLayer.Add(_adorner);
        }
    }

}
