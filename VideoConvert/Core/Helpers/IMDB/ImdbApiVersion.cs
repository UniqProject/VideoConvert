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

using System.Xml.Serialization;

namespace VideoConvert.Core.Helpers.IMDB
{
    [XmlRoot("version")]
    public class ImdbApiVersion
    {
        [XmlElement("api")]
        public string Version { get; set; }

        [XmlElement("database")]
        public string DatabaseDate { get; set; }

        public ImdbApiVersion()
        {
            Version = string.Empty;
            DatabaseDate = string.Empty;
        }
    }
}