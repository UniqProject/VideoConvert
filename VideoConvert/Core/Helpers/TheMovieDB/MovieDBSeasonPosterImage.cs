using System.Xml.Serialization;

namespace VideoConvert.Core.Helpers.TheMovieDB
{
    public class MovieDBSeasonPosterImage : MovieDBPosterImage
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("season")]
        public int Season { get; set; }

        public MovieDBSeasonPosterImage()
        {
            Type = "season";
            Season = -1;
        }
    }
}