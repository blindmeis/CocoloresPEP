﻿using System;
using System.Collections.Generic;
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
        public MemoryStream SchreibeIstZeiten(Arbeitswoche woche, IList<Mitarbeiter> mitarbeiters)
        {
            using (var xls = new ExcelPackage())
            {
                var ws = xls.Workbook.Worksheets.Add($"Woche {woche.KalenderWoche}");
                ws.Workbook.CalcMode = ExcelCalcMode.Automatic;

                ws.Cells[1, 1, 1, 13].Merge = true;
                ws.Cells[1, 1].Value = $"Arbeitswoche:  {woche.KalenderWoche}  vom {woche.Arbeitstage.First().Datum.ToString("dd.MM.yyyy")} bis {woche.Arbeitstage.Last().Datum.ToString("dd.MM.yyyy")}";
                ws.Cells[1, 1, 1, 13].Style.Font.Bold = true;

                for (int i = 0; i < woche.Arbeitstage.Count; i++)
                {
                    var datum = woche.Arbeitstage[i].Datum;
                    if(datum.DayOfWeek == DayOfWeek.Saturday || datum.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    int col = i + 2;
                    ws.Cells[3,col].Value = datum.GetWochentagName();
                }
                var colAngeordneteStunden = 7;
                ws.Cells[3, colAngeordneteStunden].Value = "ang. Std.";
                ws.Cells[3, colAngeordneteStunden].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                var colKfz = 8;
                ws.Cells[3, colKfz].Value = "KFZ";
                ws.Cells[3, colKfz].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                var sortedMa = mitarbeiters.OrderBy(x => x.DefaultGruppe).ThenBy(x => x.IsHelfer).ThenBy(x => x.Name).ToList();
                for (int i = 0; i < mitarbeiters.Count; i++)
                {
                    var row = 4 + i;
                    var ma = sortedMa[i];
                    ws.Cells[row,1].Value = ma.Name;

                    var color = ma.DefaultGruppe.GetFarbeFromResources();
                    ws.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(color.Color.A, color.Color.R, color.Color.G, color.Color.B));

                    for (int j = 0; j < woche.Arbeitstage.Count; j++)
                    {
                        var tag = woche.Arbeitstage[j];
                       
                        foreach (var zeiten in tag.Planzeiten.Where(x=>x.ErledigtDurch.Name==ma.Name))
                        {
                            if(zeiten.Dienst ==DienstTyp.Frei)
                                continue;

                            var endzeit = zeiten.Zeitraum.End;
                            ws.Cells[row, j + 2].Value =$"{zeiten.Zeitraum.Start.ToString("HH:mm")}-{endzeit.ToString("HH:mm")}";

                            var c = zeiten.Gruppe.GetFarbeFromResources();
                            ws.Cells[row, j + 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[row, j + 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(c.Color.A, c.Color.R, c.Color.G, c.Color.B));

                            if ((zeiten.Dienst & DienstTyp.Frühdienst)== DienstTyp.Frühdienst
                                || (zeiten.Dienst & DienstTyp.SpätdienstEnde) == DienstTyp.SpätdienstEnde)
                                ws.Cells[row, j + 2].Style.Font.Bold = true;
                        }
                    }

                    ws.Cells[row, colAngeordneteStunden].Value = ma.WochenStunden;
                    ws.Cells[row, colAngeordneteStunden].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[row, colKfz].Value = ma.KindFreieZeit;
                    ws.Cells[row, colKfz].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                
                ws.Cells.AutoFitColumns();
                ws.View.ShowGridLines = false;
                //ws.View.FreezePanes(4, 1);
                var modelTable = ws.Cells[3,1,mitarbeiters.Count+3,8];
                
                // Assign borders
                modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                modelTable.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                var ms = new MemoryStream();
                xls.SaveAs(ms);
                
                return ms;
            }
        }
    }
}
