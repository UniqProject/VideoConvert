// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EpisodeEntry.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   TV-Show episode
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using TheMovieDB;

    /// <summary>
    /// TV-Show episode
    /// </summary>
    [XmlRoot("episodedetails")]
    public class EpisodeEntry
    {
        /// <summary>
        /// Episode Title
        /// </summary>
        [XmlElement("title")]
        public string Title { get; set; }

        /// <summary>
        /// Show Title
        /// </summary>
        [XmlElement("showtitle")]
        public string ShowTitle { get; set; }

        /// <summary>
        /// Rating
        /// </summary>
        [XmlElement("rating", typeof(float))]
        public float Rating { get; set; }

        /// <summary>
        /// Episode bookmark
        /// </summary>
        [XmlElement("epbookmark", typeof(float))]
        public float EpBookmark { get; set; }

        /// <summary>
        /// Year
        /// </summary>
        [XmlElement("year", typeof(int))]
        public int Year { get; set; }

        /// <summary>
        /// Top 250 placement
        /// </summary>
        [XmlElement("top250", typeof(int))]
        public int Top250 { get; set; }

        /// <summary>
        /// Season number
        /// </summary>
        [XmlElement("season")]
        public int Season { get; set; }

        /// <summary>
        /// Episode number
        /// </summary>
        [XmlElement("episode")]
        public int Episode { get; set; }

        /// <summary>
        /// Episode unique ID
        /// </summary>
        [XmlElement("uniqueid")]
        public int UniqueID { get; set; }

        /// <summary>
        /// Displayed Season number
        /// </summary>
        [XmlElement("displayseason")]
        public int DisplaySeason { get; set; }

        /// <summary>
        /// Displayed Episode number
        /// </summary>
        [XmlElement("displayepisode")]
        public int DisplayEpisode { get; set; }

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
        /// Episode Plot
        /// </summary>
        [XmlElement("plot")]
        public string Plot { get; set; }

        /// <summary>
        /// Tagline
        /// </summary>
        [XmlElement("tagline")]
        public string Tagline { get; set; }

        /// <summary>
        /// Episode Runtime
        /// </summary>
        [XmlElement("runtime", typeof(int))]
        public int Runtime { get; set; }

        /// <summary>
        /// Poster Image
        /// </summary>
        [XmlElement("thumb")]
        public MovieDbImageInfo PosterImage { get; set; }

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
        /// Last played date
        /// </summary>
        [XmlElement("lastplayed")]
        public string LastPlayed { get; set; }

        /// <summary>
        /// IMDB ID
        /// </summary>
        [XmlElement("id")]
        public string ImdbID { get; set; }

        /// <summary>
        /// Names of Movie/Show sets
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
        /// Premiere date
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
        /// Date first aired
        /// </summary>
        [XmlElement("aired")]
        public string Aired { get; set; }

        /// <summary>
        /// Studios
        /// </summary>
        [XmlElement("studio")]
        public List<string> Studios { get; set; }

        /// <summary>
        /// Trailer link
        /// </summary>
        [XmlElement("trailer")]
        public string Trailer { get; set; }

        /// <summary>
        /// Cast
        /// </summary>
        [XmlElement("actor")]
        public List<MovieDbCast> Casts { get; set; }

        /// <summary>
        /// Date added
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