using System.Collections.Generic;
using System.Xml.Serialization;

namespace VideoConvert.Core.Helpers.IMDB
{
    public class ImdbAKAEntry
    {
        [XmlArray("remarks")]
        [XmlArrayItem("item")]
        public List<string> Remarks { get; set; }

        [XmlElement("country")]
        public string Country { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        public ImdbAKAEntry()
        {
            Remarks = new List<string>();
            Country = string.Empty;
            Title = string.Empty;
        }
    }
}