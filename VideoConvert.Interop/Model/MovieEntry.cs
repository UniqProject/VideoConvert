// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieEntry.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Movie Metadata
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VideoConvert.Interop.Model.TheMovieDB;

    /// <summary>
    /// Movie Metadata
    /// </summary>
    [XmlRoot("movie")]
    public class MovieEntry
    {
        /// <summary>
        /// Movie Title
        /// </summary>
        [XmlElement("title")]
        public string Title { get; set; }

        /// <summary>
        /// Original Title
        /// </summary>
        [XmlElement("originaltitle")]
        public string OriginalTitle { get; set; }

        /// <summary>
        /// Movie Title used for sorting
        /// </summary>
        [XmlElement("sorttitle")]
        public string SortTitle { get; set; }

        /// <summary>
        /// Movie Rating
        /// </summary>
        [XmlElement("rating", typeof(float))]
        public float Rating { get; set; }

        /// <summary>
        /// Bookmark
        /// </summary>
        [XmlElement("epbookmark", typeof(float))]
        public float EpBookmark { get; set; }

        /// <summary>
        /// Release year
        /// </summary>
        [XmlElement("year", typeof(int))]
        public int Year { get; set; }

        /// <summary>
        /// Top 250 placement
        /// </summary>
        [XmlElement("top250", typeof(int))]
        public int Top250 { get; set; }

        /// <summary>
        /// Votes count
        /// </summary>
        [XmlElement("votes", typeof(int))]
        public int Votes { get; set; }

        /// <summary>
        /// Plot outline
        /// </summary>
        [XmlElement("outline")]
        public string Outline { get; set; }

        /// <summary>
        /// Plot
        /// </summary>
        [XmlElement("plot")]
        public string Plot { get; set; }

        /// <summary>
        /// Tagline
        /// </summary>
        [XmlElement("tagline")]
        public string Tagline { get; set; }

        /// <summary>
        /// Runtime
        /// </summary>
        [XmlElement("runtime", typeof(int))]
        public int Runtime { get; set; }

        /// <summary>
        /// Poster/cover images
        /// </summary>
        [XmlElement("thumb")]
        public List<MovieDbPosterImage> PosterImages { get; set; }

        /// <summary>
        /// Fanart / backdrop images
        /// </summary>
        [XmlArray("fanart")]
        [XmlArrayItem("thumb")]
        public List<MovieDbImageInfo> FanartImages { get; set; }

        /// <summary>
        /// MPAA rating
        /// </summary>
        [XmlElement("mpaa")]
        public string MpaaRating { get; set; }

        /// <summary>
        /// Playcount
        /// </summary>
        [XmlElement("playcount", typeof(int))]
        public int PlayCount { get; set; }

        /// <summary>
        /// Date last played
        /// </summary>
        [XmlElement("lastplayed")]
        public string LastPlayed { get; set; }

        /// <summary>
        /// IMDB ID
        /// </summary>
        [XmlElement("id")]
        public string ImdbID { get; set; }

        /// <summary>
        /// Genres
        /// </summary>
        [XmlElement("genre")]
        public List<string> Genres { get; set; }

        /// <summary>
        /// Production countries
        /// </summary>
        [XmlElement("country")]
        public List<string> Countries { get; set; }

        /// <summary>
        /// Names of Movie sets this Movie belongs to
        /// </summary>
        [XmlElement("set")]
        public List<string> SetNames { get; set; }

        /// <summary>
        /// Writers
        /// </summary>
        [XmlElement("credits")]
        public List<string> Writers { get; set; }

        /// <summary>
        /// Directors
        /// </summary>
        [XmlElement("director")]
        public List<string> Directors { get; set; }

        /// <summary>
        /// Date of premiere
        /// </summary>
        [XmlElement("premiered")]
        public string Premiered { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [XmlElement("status")]
        public string Status { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        [XmlElement("code")]
        public string Code { get; set; }

        /// <summary>
        /// First aired date
        /// </summary>
        [XmlElement("aired")]
        public string Aired { get; set; }

        /// <summary>
        /// Studios
        /// </summary>
        [XmlElement("studio")]
        public List<string> Studios { get; set; }

        /// <summary>
        /// Link to trailer
        /// </summary>
        [XmlElement("trailer")]
        public string Trailer { get; set; }

        /// <summary>
        /// Casts
        /// </summary>
        [XmlElement("actor")]
        public List<MovieDbCast> Casts { get; set; }

        /// <summary>
        /// Date of DB Add
        /// </summary>
        [XmlElement("dateadded")]
        public string DateAdded { get; set; }

        /// <summary>
        /// Backdrop image selection
        /// </summary>
        [XmlIgnore]
        public string SelectedBackdropImage { get; set; }

        /// <summary>
        /// Poster image selection
        /// </summary>
        [XmlIgnore]
        public string SelectedPosterImage { get; set; }
    }
}
