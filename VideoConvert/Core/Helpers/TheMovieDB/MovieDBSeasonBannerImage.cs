using System.Xml.Serialization;

namespace VideoConvert.Core.Helpers.TheMovieDB
{
    public class MovieDBSeasonBannerImage : MovieDBBannerImage
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("season")]
        public int Season { get; set; }

        public MovieDBSeasonBannerImage()
        {
            Type = "season";
            Season = -1;
        }
    }
}