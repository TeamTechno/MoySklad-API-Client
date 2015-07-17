using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MoySklad.Client
{
    public static class StringXmlSerializer
    {
        public static string ToXml(this object objectInstance)
        {
            var serializer = new XmlSerializer(objectInstance.GetType());
            var sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, objectInstance);
            }
            return sb.ToString();
        }

        public static string ToXmlWithoutXmlDeclaration(this object objectInstance)
        {
            //Create our own namespaces for the output
            var ns = new XmlSerializerNamespaces();
            //Add an empty namespace and empty value
            ns.Add("", "");
            var serializer = new XmlSerializer(objectInstance.GetType());
            var writerSettings = new XmlWriterSettings {OmitXmlDeclaration = true};
            var sw = new StringWriter();
            using (var xmlWriter = XmlWriter.Create(sw, writerSettings))
            {
                serializer.Serialize(xmlWriter, objectInstance, ns);
            }
            return sw.ToString();
        }

        public static T FromXml<T>(string objectData)
        {
            return (T) FromXml(objectData, typeof (T));
        }

        public static object FromXml(string objectData, Type type)
        {
            var serializer = new XmlSerializer(type);
            object result;
            using (TextReader reader = new StringReader(objectData))
            {
                result = serializer.Deserialize(reader);
            }
            return result;
        }
    }
}