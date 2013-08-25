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

using System;
using System.Collections.Generic;

namespace VideoConvert.Core.Helpers.TheMovieDB
{
    public class DBTvShowEpisode
    {
        public string EpisodeTitle { get; set; }
        public int EpisodeNumber { get; set; }
        public int AbsoluteEpisodeNumber { get; set; }
        public double DvdEpisodeNumber { get; set; }
        public double CombinedEpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public bool IsSpecial { get; set; }
        public string ImdbId { get; set; }
        public DateTime FirstAired { get; set; }
        public double Rating { get; set; }
        public int Runtime { get; set; }
        public List<string> Writers { get; set; }
        public string WritersString
        {
            get { return Writers != null ? string.Join(" / ", Writers) : string.Empty; }
        }

        public List<string> Directors { get; set; }
        public string DirectorsString
        {
            get { return Directors != null ? string.Join(" / ", Directors) : string.Empty; }
        }

        public List<string> GuestStars { get; set; }
        public string GuestStarsString
        {
            get { return GuestStars != null ? string.Join(" / ", GuestStars) : string.Empty; }
        }

        public string Plot { get; set; }
        public string EpisodeImageUrl { get; set; }
    }
}