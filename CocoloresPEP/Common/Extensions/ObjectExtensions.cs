using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CocoloresPEP.Common.Extensions
{
    public static class ObjectExtensions
    {
        public static void ToFile<T>(this object obj, string filename)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    var serializer = new DataContractSerializer(typeof(T));
                    serializer.WriteObject(ms, obj);

                    ms.CreateFile(false, filename);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Serialisieren von {filename}", ex);
            }
            
        }

        public static T FromFile<T>(string filename)
        {
            if (string.IsNullOrEmpty(filename)) { return default(T); }

            T objectOut = default(T);

            try
            {
                var serializer = new DataContractSerializer(typeof(T));
                using (var stream = File.OpenRead(filename))
                {
                    objectOut = (T)serializer.ReadObject(stream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Derialisieren von {filename}", ex);
            }

            return objectOut;
        }
    }
}
