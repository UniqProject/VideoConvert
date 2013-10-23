// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImdbMovieData.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.IMDB
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot("imdbdocument")]
    public class ImdbMovieData
    {
        [XmlElement("rating")]
        public float Rating { get; set; }

        [XmlElement("rating_count")]
        public long RatingCount { get; set; }

        [XmlElement("year")]
        public int Year { get; set; }

        [XmlElement("plot")]
        public string Plot { get; set; }

        [XmlArray("genres")]
        [XmlArrayItem("item")]
        public List<string> Genres { get; set; }

        [XmlElement("rated")]
        public string Certification { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("imdb_url")]
        public string ImdbUrl { get; set; }

        [XmlArray("directors")]
        [XmlArrayItem("item")]
        public List<string> Directors { get; set; }

        [XmlArray("writers")]
        [XmlArrayItem("item")]
        public List<string> Writers { get; set; }

        [XmlArray("actors")]
        [XmlArrayItem("item")]
        public List<string> Cast { get; set; }

        [XmlElement("plot_simple")]
        public string PlotOutline { get; set; }

        [XmlElement("type")]
        public string MediaType { get; set; }

        [XmlElement("poster")]
        public string PosterUrl { get; set; }

        [XmlElement("imdb_id")]
        public string ImdbID { get; set; }

        [XmlArray("also_known_as")]
        [XmlArrayItem("item")]
        public List<ImdbAKAEntry> AlsoKnownAs { get; set; }

        [XmlArray("language")]
        [XmlArrayItem("item")]
        public List<string> Languages { get; set; }

        [XmlArray("country")]
        [XmlArrayItem("item")]
        public List<string> Countries { get; set; }

        [XmlArray("release_date")]
        [XmlArrayItem("item")]
        public List<ImdbReleaseDateEntry> ReleaseDates { get; set; }

        [XmlElement("filming_locations")]
        public string FilmingLocations { get; set; }

        [XmlElement("runtime")]
        public string Runtime { get; set; }
    }
}