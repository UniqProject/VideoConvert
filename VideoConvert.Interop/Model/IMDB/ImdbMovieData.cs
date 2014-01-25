// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImdbMovieData.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   IMDB movie data
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.IMDB
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// IMDB movie data
    /// </summary>
    [XmlRoot("imdbdocument")]
    public class ImdbMovieData
    {
        /// <summary>
        /// Rating
        /// </summary>
        [XmlElement("rating")]
        public float Rating { get; set; }

        /// <summary>
        /// Rating count
        /// </summary>
        [XmlElement("rating_count")]
        public long RatingCount { get; set; }

        /// <summary>
        /// Release year
        /// </summary>
        [XmlElement("year")]
        public int Year { get; set; }

        /// <summary>
        /// Plot
        /// </summary>
        [XmlElement("plot")]
        public string Plot { get; set; }

        /// <summary>
        /// Genre list
        /// </summary>
        [XmlArray("genres")]
        [XmlArrayItem("item")]
        public List<string> Genres { get; set; }

        /// <summary>
        /// Certification
        /// </summary>
        [XmlElement("rated")]
        public string Certification { get; set; }

        /// <summary>
        /// Movie title
        /// </summary>
        [XmlElement("title")]
        public string Title { get; set; }

        /// <summary>
        /// IMDB URL
        /// </summary>
        [XmlElement("imdb_url")]
        public string ImdbUrl { get; set; }

        /// <summary>
        /// Directors list
        /// </summary>
        [XmlArray("directors")]
        [XmlArrayItem("item")]
        public List<string> Directors { get; set; }

        /// <summary>
        /// Writers list
        /// </summary>
        [XmlArray("writers")]
        [XmlArrayItem("item")]
        public List<string> Writers { get; set; }

        /// <summary>
        /// Cast list
        /// </summary>
        [XmlArray("actors")]
        [XmlArrayItem("item")]
        public List<string> Cast { get; set; }

        /// <summary>
        /// Plot outline
        /// </summary>
        [XmlElement("plot_simple")]
        public string PlotOutline { get; set; }

        /// <summary>
        /// Media Type
        /// </summary>
        [XmlElement("type")]
        public string MediaType { get; set; }

        /// <summary>
        /// Poster URL
        /// </summary>
        [XmlElement("poster")]
        public string PosterUrl { get; set; }

        /// <summary>
        /// IMDB ID
        /// </summary>
        [XmlElement("imdb_id")]
        public string ImdbID { get; set; }

        /// <summary>
        /// Movie AKA
        /// </summary>
        [XmlArray("also_known_as")]
        [XmlArrayItem("item")]
        public List<ImdbAKAEntry> AlsoKnownAs { get; set; }

        /// <summary>
        /// Language list
        /// </summary>
        [XmlArray("language")]
        [XmlArrayItem("item")]
        public List<string> Languages { get; set; }

        /// <summary>
        /// Countries
        /// </summary>
        [XmlArray("country")]
        [XmlArrayItem("item")]
        public List<string> Countries { get; set; }

        /// <summary>
        /// Release dates
        /// </summary>
        [XmlArray("release_date")]
        [XmlArrayItem("item")]
        public List<ImdbReleaseDateEntry> ReleaseDates { get; set; }

        /// <summary>
        /// Filming locations
        /// </summary>
        [XmlElement("filming_locations")]
        public string FilmingLocations { get; set; }

        /// <summary>
        /// Movie runtime
        /// </summary>
        [XmlElement("runtime")]
        public string Runtime { get; set; }
    }
}