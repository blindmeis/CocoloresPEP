using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocoloresPEP.Common.Entities;
using OfficeOpenXml;

namespace CocoloresPEP.Services
{
    public class IstService
    {
        public MemoryStream SchreibeIstZeiten(Arbeitswoche woche, IList<Mitarbeiter> mitarbeiters)
        {
            using (var xls = new ExcelPackage())
            {
                var ws = xls.Workbook.Worksheets.Add("Einzelpläne");
                ws.Workbook.CalcMode = ExcelCalcMode.Automatic;

                ws.Cells[1, 1, 1, 13].Merge = true;
                ws.Cells[1, 1].Value = $"Arbeitswoche: {woche.KalenderWoche} vom {woche.Arbeitstage.First().Datum.ToString("dd.MM.yyyy")} bis {woche.Arbeitstage.Last().Datum.ToString("dd.MM.yyyy")}";
                ws.Cells[1, 1, 1, 13].Style.Font.Bold = true;

                for (int i = 0; i < woche.Arbeitstage.Count; i++)
                {
                    int col = i + 2;
                    ws.Cells[3,col].Value = woche.Arbeitstage[i].Datum;
                    ws.Cells[3,col].Style.Numberformat.Format = "dd.MM.yyyy";
                }

                for (int i = 0; i < mitarbeiters.Count; i++)
                {
                    var row = 4 + i;
                    ws.Cells[row,1].Value = mitarbeiters[i].Name;

                    for (int j = 0; j < woche.Arbeitstage.Count; j++)
                    {
                        var tag = woche.Arbeitstage[j];
                        var ma = mitarbeiters[i];
                        foreach (var zeiten in tag.Istzeiten.Where(x=>x.ErledigtDurch.Name==ma.Name))
                        {
                            var endzeit = zeiten.Startzeit.AddMinutes(15*zeiten.QuarterTicks);
                            ws.Cells[row, j + 2].Value =$"{zeiten.Startzeit.ToString("HH:mm")}-{endzeit.ToString("HH:mm")}";
                        }
                    }

                }
                
                ws.Cells.AutoFitColumns();
                ws.View.FreezePanes(4, 1);

                var ms = new MemoryStream();
                xls.SaveAs(ms);
                return ms;
            }
        }
    }
}
