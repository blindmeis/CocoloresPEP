using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CocoloresPEP.Common.Extensions;
using CocoloresPEP.Common.WpfCore;

namespace CocoloresPEP
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Helper.SetOnStartUpSettings();

            var dic = BusinessExtensions.GetExternalResources();

            if(dic!=null)
                Application.Current.Resources.MergedDictionaries.Add(dic);

            base.OnStartup(e);
        }
    }
}
