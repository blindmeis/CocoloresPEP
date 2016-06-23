using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CocoloresPEP.Common.WpfCore
{
    public static class Helper
    {
        /// <summary>
        /// Standard Settings die für eine Wpf Applikation gelten sollen
        /// Culture, ButtonClick, ...
        /// </summary>
        public static void SetOnStartUpSettings()
        {
            var ci = new CultureInfo("de-DE", false);
            //damit bei Datumeingabe mit 2Jahreszahlen immer die 20 davor gesetzt wird
            ci.Calendar.TwoDigitYearMax = 2099;

            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;

            //Problem von TextBox LostFocus Binding und Button IsDefault oder Keybinding umgehen
            EventManager.RegisterClassHandler(typeof(Button), ButtonBase.ClickEvent, new RoutedEventHandler(
                (sender, args) =>
                {
                    var btn = sender as Button;
                    if (btn != null)
                    {
                        if (!btn.Focus())
                            if (MoveKeyboardFocusToNext())
                                MoveKeyboardFocusToPrevious();
                    }
                }));


            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        }

        /// <summary>
        /// Erzeugt ein Wpf Fenster was quasi unsichtbar ist, 
        /// aber gebraucht wird wenn man vor dem MainWindow schon Messageboxen anzeigen will
        /// </summary>
        /// <returns></returns>
        public static Window GetBugWindow()
        {
            return new Window()
            {
                AllowsTransparency = true,
                Background = System.Windows.Media.Brushes.Transparent,
                WindowStyle = WindowStyle.None,
                Top = 0,
                Left = 0,
                Width = 1,
                Height = 1,
                ShowInTaskbar = false
            };
        }

        /// <summary>
        /// Erzeugt eine DeepCopy für object die serialisierbar sind.
        /// </summary>
        /// <param name="objToCopy">object to copy</param>
        /// <returns>copy of object</returns>
        public static object GetDeepCopy(object objToCopy)
        {
            // Create a "deep" clone of 
            // an object. That is, copy not only
            // the object and its pointers
            // to other objects, but create 
            // copies of all the subsidiary 
            // objects as well. This code even 
            // handles recursive relationships.

            object objResult = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, objToCopy);

                // Rewind back to the beginning 
                // of the memory stream. 
                // Deserialize the data, then
                // close the memory stream.
                ms.Position = 0;
                objResult = bf.Deserialize(ms);
            }
            return objResult;
        }

        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the queried item.</param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, a null reference is being returned.</returns>
        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            // get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // we’ve reached the end of the tree
            if (parentObject == null) return null;

            // check if the parent matches the type we’re looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                // use recursion to proceed with next level
                return FindVisualParent<T>(parentObject);
            }
        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName = "")
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// Probiert vom aktuellen KeyBoard.FocusedElement den Focus eins weiter zu rücken
        /// </summary>
        public static bool MoveKeyboardFocusToNext()
        {
            var fe = Keyboard.FocusedElement as UIElement;

            return fe?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)) ?? false;
        }

        public static bool MoveKeyboardFocusToPrevious()
        {
            var fe = Keyboard.FocusedElement as UIElement;

            return fe?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous)) ?? false;
        }


    }
}
