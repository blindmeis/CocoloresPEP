﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace CocoloresPEP.Common.WpfCore.Behaviors
{
    /// <summary>
    /// use <Setter Property="behaviours:MyXYZBehaviour.IsEnabledForStyle" Value="True"/>
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    /// <typeparam name="TBehavior"></typeparam>
    public class AttachableForStyleBehavior<TComponent, TBehavior> : Behavior<TComponent>
        where TComponent : System.Windows.DependencyObject
        where TBehavior : AttachableForStyleBehavior<TComponent, TBehavior>, new()
    {
        public static DependencyProperty IsEnabledForStyleProperty =
            DependencyProperty.RegisterAttached("IsEnabledForStyle", typeof(bool),
            typeof(AttachableForStyleBehavior<TComponent, TBehavior>), new FrameworkPropertyMetadata(false, OnIsEnabledForStyleChanged));

        public bool IsEnabledForStyle
        {
            get { return (bool)GetValue(IsEnabledForStyleProperty); }
            set { SetValue(IsEnabledForStyleProperty, value); }
        }

        private static void OnIsEnabledForStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement uie = d as UIElement;

            if (uie != null)
            {
                var behColl = Interaction.GetBehaviors(uie);
                var existingBehavior = behColl.FirstOrDefault(b => b.GetType() ==
                      typeof(TBehavior)) as TBehavior;

                if ((bool)e.NewValue == false && existingBehavior != null)
                {
                    behColl.Remove(existingBehavior);
                }

                else if ((bool)e.NewValue == true && existingBehavior == null)
                {
                    behColl.Add(new TBehavior());
                }
            }
        }
    }
}
