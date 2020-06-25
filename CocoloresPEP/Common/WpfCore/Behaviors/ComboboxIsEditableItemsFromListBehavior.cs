using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace CocoloresPEP.Common.WpfCore.Behaviors
{
    /// <summary>
    /// Behavior für eine editierbare Combobox die nur Eingaben zulässt die in der ItemsSource sind.
    /// </summary>
    public class ComboboxIsEditableItemsFromListBehavior : Behavior<ComboBox>
    {
        /// <summary>
        /// Gibt den Modus für das Verhalten an in abh. der ItemsSource
        /// </summary>
        public ComboBoxItemsSourceMode BehaviorMode { get; set; }

        public bool IgnoreInputCase { get; set; }

        public bool SetEnterToHandledTrue { get; set; }

        public ComboboxIsEditableItemsFromListBehavior()
        {
            //default
            this.BehaviorMode = ComboBoxItemsSourceMode.IsString;
            this.IgnoreInputCase = true;
            this.SetEnterToHandledTrue = true;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewTextInput += AssociatedObjectPreviewTextInput;
            AssociatedObject.PreviewKeyDown += AssociatedObjectPreviewKeyDown;

            DataObject.AddPastingHandler(AssociatedObject, Pasting);

        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewTextInput -= AssociatedObjectPreviewTextInput;
            AssociatedObject.PreviewKeyDown -= AssociatedObjectPreviewKeyDown;

            DataObject.RemovePastingHandler(AssociatedObject, Pasting);
        }

        private void Pasting(object sender, DataObjectPastingEventArgs e)
        {
            //einfügen nicht erlaubt
            System.Media.SystemSounds.Beep.Play();
            e.CancelCommand();
        }

        private void AssociatedObjectPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var txt = e.OriginalSource as TextBox;

            if (txt == null)
                return;

            var text = this.GetText(txt, e.Text);

            if (!ObInItemsSource(text))
            {
                System.Media.SystemSounds.Beep.Play();
                e.Handled = true;
            }
        }

        //space kommt nicht über TextInput
        private void AssociatedObjectPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (SetEnterToHandledTrue)
                    e.Handled = true;
                return;
            }

            if (e.Key == Key.Space)
            {
                var txt = e.OriginalSource as TextBox;

                if (txt == null)
                    return;

                var text = this.GetText(txt, " ");

                if (!ObInItemsSource(text))
                {
                    System.Media.SystemSounds.Beep.Play();
                    e.Handled = true;
                }
            }
        }

        private IEnumerable<DictionaryEntry> CastDict(IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                yield return entry;
            }
        }

        private bool ObInItemsSource(string text)
        {
            //wenn keine erwartete Collection angebunden ist, nix zulassen
            if (this.AssociatedObject.ItemsSource == null)
            {
                Debug.WriteLine("ObInItemsSource: " + this.AssociatedObject.ItemsSource);
                return false;
            }
            if (this.BehaviorMode == ComboBoxItemsSourceMode.IsStringDictionary && !(this.AssociatedObject.ItemsSource is IDictionary<string, string>))
            {
                Debug.WriteLine("ObInItemsSource IsStringDictionary: " + this.AssociatedObject.ItemsSource);
                return false;
            }

            if (this.BehaviorMode == ComboBoxItemsSourceMode.IsStringDictionary)
            {
                //bl_erf umgestellt auf dictionary<string,string>
                var collection = (IDictionary<string, string>)this.AssociatedObject.ItemsSource;


                return collection.Any(x => ((string)x.Key).StartsWith(text, this.IgnoreInputCase, CultureInfo.CurrentCulture));
            }
            else if (this.BehaviorMode == ComboBoxItemsSourceMode.IsString)
            {
                var items = CollectionViewSource.GetDefaultView(this.AssociatedObject.ItemsSource);

                if (items == null)
                    return false;

                var collection = items.Cast<string>();

                return collection.Any(x => x.ToLower().StartsWith(text, this.IgnoreInputCase, CultureInfo.CurrentCulture));
            }
            else if (this.BehaviorMode == ComboBoxItemsSourceMode.IsIDictionary)
            {
                var collection = this.AssociatedObject.ItemsSource as IDictionary;

                if (collection == null)
                    return false;

                return collection.Keys.OfType<string>().Any(key => key.ToLower().StartsWith(text, this.IgnoreInputCase, CultureInfo.CurrentCulture));
            }




            return true;
        }


        private string GetText(TextBox txt, string input)
        {
            var realtext = txt.Text.Remove(txt.SelectionStart, txt.SelectionLength);
            var newtext = realtext.Insert(txt.CaretIndex, input);

            return newtext;
        }
    }

    public enum ComboBoxItemsSourceMode
    {
        /// <summary>
        /// ItemsSource List von Strings
        /// </summary>
        IsString,
        /// <summary>
        /// Dictionary ItemsSource, wobei der Key das zu Prüfende Element ist
        /// </summary>
        IsStringDictionary,
        /// <summary>
        /// Irgendein Dictionary und man geht das von aus das der Key string ist
        /// </summary>
        IsIDictionary
    }
}
