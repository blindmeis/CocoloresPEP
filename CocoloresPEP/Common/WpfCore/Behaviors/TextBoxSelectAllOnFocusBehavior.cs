using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using System.Windows.Media;

namespace CocoloresPEP.Common.WpfCore.Behaviors
{
    /// <summary>
    /// Markiert alle Zeichen in einer Textbox wenn diese den Focus erhält.
    /// </summary>
    public class TextBoxSelectAllOnFocusBehavior : Behavior<TextBox>
    {
        /// <summary>
        /// SelectAll auch wenn nur leerzeichen in der TextBox stehen
        /// </summary>
        public bool SelectedAllOnWhitespace { get; set; }

        public TextBoxSelectAllOnFocusBehavior()
        {
            SelectedAllOnWhitespace = true;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotKeyboardFocus += AssociatedObjectGotKeyboardFocus;
            AssociatedObject.PreviewMouseDown += SelectivelyIgnoreMouseButton;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.GotKeyboardFocus -= AssociatedObjectGotKeyboardFocus;
            AssociatedObject.PreviewMouseDown -= SelectivelyIgnoreMouseButton;
        }

        /// <summary>
        /// Ignoriert den Klick, wenn der Fokus in der Textbox ist. Ansonsten sucht 
        /// die Methode die Textbox  und gibt ihr den Focus, danach wird der Klick 
        /// nicht weiter behandelt.
        /// </summary>
        private static void SelectivelyIgnoreMouseButton(
            object sender, MouseButtonEventArgs e)
        {
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent == null)
            {
                return;
            }

            var textBox = (TextBox)parent;
            if (!textBox.IsKeyboardFocusWithin)
            {
                textBox.Focus();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Selektiert alles, wenn <see cref="SelectedAllOnWhitespace"/> = true oder 
        /// Text in der Textbox steht.
        /// </summary>
        private void AssociatedObjectGotKeyboardFocus(
            object sender, KeyboardFocusChangedEventArgs e)
        {
            if (SelectedAllOnWhitespace || !string.IsNullOrWhiteSpace(AssociatedObject.Text))
            {
                AssociatedObject.SelectAll();
            }
        }

    }
}
