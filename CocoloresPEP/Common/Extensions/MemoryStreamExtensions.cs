using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocoloresPEP.Common.Extensions
{
    public static class MemoryStreamExtensions
    {
        public static string CreatePdfFileOnTempPath(this MemoryStream ms, bool obShowWithProcessStart)
        {
            return ms.CreateFileOnTempPath(obShowWithProcessStart, "pdf");
        }

        public static string CreateXlsxFileOnTempPath(this MemoryStream ms, bool obShowWithProcessStart)
        {
            return ms.CreateFileOnTempPath(obShowWithProcessStart, "xlsx");
        }

        public static string CreateFileOnTempPath(this MemoryStream ms, bool obShowWithProcessStart, string fileExtension)
        {
            return ms.CreateFile(obShowWithProcessStart, System.IO.Path.GetTempFileName() + "." + fileExtension);
        }

        public static string CreateFile(this MemoryStream ms, bool obShowWithProcessStart, string file)
        {
            if (ms == null)
                throw new ArgumentNullException("ms", "Daten Stream ist leer");

            ms.Position = 0;

            var path = file;

            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Create);
                ms.WriteTo(fs);
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
                ms.Dispose();
            }

            if (obShowWithProcessStart)
                Process.Start(path);

            return path;
        }
    }
}
