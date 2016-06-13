using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            using (var ms = new MemoryStream())
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(ms, obj);

                ms.CreateFile(false, filename);

                //using (TextWriter streamWriter = new StreamWriter(memoryStream))
                //{
                //    var xmlSerializer = new XmlSerializer(typeof(T));
                //    xmlSerializer.Serialize(streamWriter, obj);
                //    return XElement.Parse(Encoding.ASCII.GetString(memoryStream.ToArray()));
                //}
            }
        }

        public static T FromFile<T>(string filename)
        {
            if (string.IsNullOrEmpty(filename)) { return default(T); }

            T objectOut = default(T);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(filename);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            }
            catch (Exception ex)
            {
                //Log exception here
            }

            return objectOut;
        }
    }
}
