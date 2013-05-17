using System.Xml.Serialization;

namespace VideoConvert.Core.Helpers.TheMovieDB
{
    public class MovieDBCast
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("role")]
        public string Role { get; set; }

        [XmlElement("thumb")]
        public string Thumbnail { get; set; }
        
        public MovieDBCast()
        {
            Thumbnail = string.Empty;
            Name = string.Empty;
            Role = string.Empty;
        }
    }
}