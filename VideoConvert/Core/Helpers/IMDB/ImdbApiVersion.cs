using System.Xml.Serialization;

namespace VideoConvert.Core.Helpers.IMDB
{
    [XmlRoot("version")]
    public class ImdbApiVersion
    {
        [XmlElement("api")]
        public string Version { get; set; }

        [XmlElement("database")]
        public string DatabaseDate { get; set; }

        public ImdbApiVersion()
        {
            Version = string.Empty;
            DatabaseDate = string.Empty;
        }
    }
}