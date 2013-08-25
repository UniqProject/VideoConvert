//============================================================================
// VideoConvert - Fast Video & Audio Conversion Tool
// Copyright © 2012 JT-Soft
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

using System.Collections.Generic;
using System.Xml.Serialization;

namespace VideoConvert.Core.Helpers.IMDB
{
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