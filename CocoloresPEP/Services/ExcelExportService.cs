using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CocoloresPEP.Common.Entities;
using CocoloresPEP.Common.Extensions;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace CocoloresPEP.Services
{
    public class ExcelExportService
    {
        public static string PfadExcel
        {
            get
            {
                if (ConfigurationManager.AppSettings.AllKeys.Any(x => x == "PfadExcel"))
                {
                    var pfad = ConfigurationManager.AppSettings["PfadExcel"];
                    var di = new DirectoryInfo(pfad);

                    if (!di.Exists)
                        di.Create();

                    return pfad;            
                }

                return "";
            }
        }

        public MemoryStream SchreibeIstZeiten(Arbeitswoche woche, IList<Mitarbeiter> mitarbeiters, bool showThemen = false)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var xls = new ExcelPackage())
            {
                var ws = xls.Workbook.Worksheets.Add($"Woche {woche.KalenderWoche}");
                ws.Workbook.CalcMode = ExcelCalcMode.Automatic;
                ws.Cells["A:XFD"].Style.Font.Name = "Consolas";


                var row = 1;
                ws.Cells[row, 1, 1, 9].Merge = true;
                ws.Cells[row, 1].Value = $"Arbeitswoche:  {woche.KalenderWoche}  vom {woche.Arbeitstage.First().Datum.ToString("dd.MM.yyyy")} bis {woche.Arbeitstage.Last().Datum.ToString("dd.MM.yyyy")}";
                ws.Cells[row, 1, 1, 13].Style.Font.Bold = true;

                row++;
                row++;
                for (int i = 0; i < woche.Arbeitstage.Count; i++)
                {
                    var datum = woche.Arbeitstage[i].Datum;
                    if(datum.DayOfWeek == DayOfWeek.Saturday || datum.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    int col = i + 2;
                    ws.Cells[row, col].Value = datum.GetWochentagName();
                }

                var colAngeordneteStunden = 7;
                ws.Cells[row, colAngeordneteStunden].Value = "ang. Std.";
                ws.Cells[row, colAngeordneteStunden].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                var colKfz = 8;
                ws.Cells[row, colKfz].Value = "KFZ";
                ws.Cells[row, colKfz].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                var sortedMa = mitarbeiters.OrderBy(x => x.DefaultGruppe).ThenBy(x => x.IsHelfer).ThenBy(x => x.Name).ToList();
                for (int i = 0; i < mitarbeiters.Count; i++)
                {
                    row ++;
                    var obThemenRow = false;
                    var ma = sortedMa[i];
                    ws.Cells[row,1].Value = ma.Name;

                    var color = ma.DefaultGruppe.GetFarbeFromResources();
                    ws.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(color.Color.A, color.Color.R, color.Color.G, color.Color.B));

                    if (showThemen && woche.Arbeitstage.Any(a=>a.Planzeiten.Any(x => x.ErledigtDurch.Name == ma.Name && x.Thema != Themen.None)))
                    {
                        ws.Cells[row+1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row+1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(color.Color.A, color.Color.R, color.Color.G, color.Color.B));
                    }

                    for (int j = 0; j < woche.Arbeitstage.Count; j++)
                    {
                        var tag = woche.Arbeitstage[j];

                        //var tmp =mitarbeiters.SingleOrDefault(x => x.Name == "Robert");
                        //if (tag.Datum == new DateTime(2020, 7, 3) && tag.Planzeiten.Any(x=>x.ErledigtDurch==null))
                        //{
                        //    var pp = tag.Planzeiten.Where(x => x.ErledigtDurch == null);
                        //    pp.First().ErledigtDurch = tmp;
                        //}
                       
                        foreach (var zeiten in tag.Planzeiten.Where(x=>x.ErledigtDurch.Name==ma.Name))
                        {
                            if(zeiten.Dienst ==DienstTyp.Frei)
                                continue;
                            
                            ws.Cells[row, j + 2].Value = zeiten.GetInfoPlanzeitInfo(true);

                            var c = zeiten.Gruppe != zeiten.ErledigtDurch.DefaultGruppe ? zeiten.Gruppe.GetFarbeFromResources() : zeiten.Dienst.GetFarbeFromResources();
                            ws.Cells[row, j + 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[row, j + 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(c.Color.A, c.Color.R, c.Color.G, c.Color.B));

                            //if ((zeiten.Dienst & DienstTyp.Frühdienst)== DienstTyp.Frühdienst
                            //    || (zeiten.Dienst & DienstTyp.SpätdienstEnde) == DienstTyp.SpätdienstEnde)
                            //    ws.Cells[row, j + 2].Style.Font.Bold = true;

                            if (showThemen && zeiten.Thema !=Themen.None)
                            {
                                ws.Cells[row+1, j + 2].Value = zeiten.Thema.GetDisplayname();
                                obThemenRow = true;
                            }

                        }
                    }

                    ws.Cells[row, colAngeordneteStunden].Value = ma.WochenStunden;
                    ws.Cells[row, colAngeordneteStunden].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[row, colKfz].Value = ma.KindFreieZeit;
                    ws.Cells[row, colKfz].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    var oben = ws.Cells[row, 1, row, 8];
                    oben.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    oben.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    oben.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    if (obThemenRow)
                        row++;

                    var unten = ws.Cells[row, 1, row, 8];
                    unten.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    unten.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    unten.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                if (woche.Arbeitstage.Any(x => x.HasGrossteam))
                {
                    row++;
                    row++;
                    var gts = woche.Arbeitstage.Where(x => x.HasGrossteam).ToList();

                    foreach (var at in gts)
                    {
                        var index = woche.Arbeitstage.IndexOf(at);
                        ws.Cells[row, index + 2].Value = $"Großteam {Environment.NewLine}{at.Grossteam.Start.ToString("HH:mm")}-{at.Grossteam.End.ToString("HH:mm")}";
                        ws.Cells[row, index + 2].Style.Font.Bold = true;
                        ws.Cells[row, index + 2].Style.WrapText = true;

                        var border = ws.Cells[row, index + 2];
                        border.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        border.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        border.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        border.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                    
                }

                

                ws.Cells.AutoFitColumns();
                ws.View.ShowGridLines = false;

                var ms = new MemoryStream();
                xls.SaveAs(ms);
                
                return ms;
            }
        }
    }
}
