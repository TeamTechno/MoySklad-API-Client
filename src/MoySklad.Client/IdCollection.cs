using System.Collections.Generic;
using System.Xml.Serialization;

namespace MoySklad.Client
{
    [XmlRoot("collection")]
    public class IdCollection
    {
        public IdCollection()
        {
        }

        public IdCollection(IEnumerable<string> items)
        {
            Items = new List<string>(items);
        }

        [XmlElement("id")]
        public List<string> Items { get; } = new List<string>();
    }
}