using System.Xml.Serialization;

namespace VideoConvert.Core.Helpers.TheMovieDB
{
    public class MovieDBPosterImage : MovieDBImageInfo
    {
        [XmlAttribute("aspect")]
        public string Aspect { get; set; }
        
        public MovieDBPosterImage()
        {
            Aspect = "poster";
        }
    }
}