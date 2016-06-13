using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CocoloresPEP.Common;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.Extensions;
using CocoloresPEP.Services;

namespace CocoloresPEP
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var mas = new List<Mitarbeiter>();

            mas.Add(new Mitarbeiter() { Name = "Rapha", WochenStunden = 28, DefaultGruppe = SollTyp.Nest });
            mas.Add(new Mitarbeiter() { Name = "Jule", WochenStunden = 30, DefaultGruppe = SollTyp.Nest });
            mas.Add(new Mitarbeiter() { Name = "MaNest", WochenStunden = 30, DefaultGruppe = SollTyp.Nest });
            mas.Add(new Mitarbeiter() { Name = "Fridl", WochenStunden = 28, DefaultGruppe = SollTyp.Gruen });
            mas.Add(new Mitarbeiter() { Name = "Alex", WochenStunden = 32, DefaultGruppe = SollTyp.Gruen });
            mas.Add(new Mitarbeiter() { Name = "MaGruen", WochenStunden = 28, DefaultGruppe = SollTyp.Gruen });
            mas.Add(new Mitarbeiter() { Name = "MaBlau1", WochenStunden = 30, DefaultGruppe = SollTyp.Blau });
            mas.Add(new Mitarbeiter() { Name = "MaBlau2", WochenStunden = 28, DefaultGruppe = SollTyp.Blau});
            mas.Add(new Mitarbeiter() { Name = "MaBlau3", WochenStunden = 35, DefaultGruppe = SollTyp.Blau });
            mas.Add(new Mitarbeiter() { Name = "MaRot1", WochenStunden = 28, DefaultGruppe = SollTyp.Rot });
            mas.Add(new Mitarbeiter() { Name = "MaRot2", WochenStunden = 32, DefaultGruppe = SollTyp.Rot });
            mas.Add(new Mitarbeiter() { Name = "MaRot3", WochenStunden = 30, DefaultGruppe = SollTyp.Rot });

            mas[7].NichtDaZeiten.Add(new DateTime(2016,6,2));

            var fac = new FactoryService();

            var am = new ArbeitswochenService();
            var woche = am.CreateArbeitswoche(2016, 22);

            var sz = new SollzeitenService();

            var montag = woche.Arbeitstage[0];
            sz.AddSollItem(montag,"Frühdienst", new DateTime(montag.Datum.Year, montag.Datum.Month, montag.Datum.Day, 7, 0, 0), 4,SollTyp.Frühdienst);
            sz.AddSollItem(montag,"8Uhr Dienst", new DateTime(montag.Datum.Year, montag.Datum.Month, montag.Datum.Day, 8, 0, 0), 2,SollTyp.AchtUhrDienst);
            sz.AddSollItem(montag,"Nest", new DateTime(montag.Datum.Year, montag.Datum.Month, montag.Datum.Day, 8, 30, 0), 30,SollTyp.Nest);//16uhr
            sz.AddSollItem(montag,"Blau", new DateTime(montag.Datum.Year, montag.Datum.Month, montag.Datum.Day, 8, 30, 0), 30,SollTyp.Blau);//16uhr
            sz.AddSollItem(montag,"Grün", new DateTime(montag.Datum.Year, montag.Datum.Month, montag.Datum.Day, 8, 30, 0), 30,SollTyp.Gruen);//16uhr
            sz.AddSollItem(montag,"Rot", new DateTime(montag.Datum.Year, montag.Datum.Month, montag.Datum.Day, 8, 30, 0), 30,SollTyp.Rot);//16uhr

            for (int i = 1; i < 5; i++)
            {
                var tag = woche.Arbeitstage[i];
                sz.AddSollItem(tag, "Frühdienst", new DateTime(montag.Datum.Year, montag.Datum.Month, montag.Datum.Day, 7, 0, 0), 4, SollTyp.Frühdienst);
                sz.AddSollItem(tag, "8Uhr Dienst", new DateTime(montag.Datum.Year, montag.Datum.Month, montag.Datum.Day, 8, 0, 0), 2, SollTyp.AchtUhrDienst);
                sz.AddSollItem(tag, "Nest", new DateTime(montag.Datum.Year, montag.Datum.Month, montag.Datum.Day, 8, 30, 0), 30, SollTyp.Nest);//16uhr
                sz.AddSollItem(tag, "Blau", new DateTime(montag.Datum.Year, montag.Datum.Month, montag.Datum.Day, 8, 30, 0), 30, SollTyp.Blau);//16uhr
                sz.AddSollItem(tag, "Grün", new DateTime(montag.Datum.Year, montag.Datum.Month, montag.Datum.Day, 8, 30, 0), 30, SollTyp.Gruen);//16uhr
                sz.AddSollItem(tag, "Rot", new DateTime(montag.Datum.Year, montag.Datum.Month, montag.Datum.Day, 8, 30, 0), 30, SollTyp.Rot);//16uhr
            }

            var plan = new PlanService();
            plan.ErstelleWochenplan(woche,mas);

            var iz = new IstService();

            var ms = iz.SchreibeIstZeiten(woche, mas);
            ms.CreateXlsxFileOnTempPath(true);

            woche.ToFile<Arbeitswoche>(@"c:\temp\cocoPEB.xml");


            var ff = ObjectExtensions.FromFile<Arbeitswoche>(@"c:\temp\cocoPEB.xml");
            var msff = iz.SchreibeIstZeiten(ff, mas);
            msff.CreateXlsxFileOnTempPath(true);

        }

    }
}
