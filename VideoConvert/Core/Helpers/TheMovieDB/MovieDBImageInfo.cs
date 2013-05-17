using System.Xml.Serialization;

namespace VideoConvert.Core.Helpers.TheMovieDB
{
    public class MovieDBImageInfo
    {
        [XmlIgnore()]
        public string Title { get; set; }

        [XmlAttribute("preview")]
        public string UrlPreview { get; set; }

        [XmlText()]
        public string UrlOriginal { get; set; }

        public MovieDBImageInfo()
        {
            Title = string.Empty;
            UrlPreview = string.Empty;
            UrlOriginal = string.Empty;
        }
    }

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