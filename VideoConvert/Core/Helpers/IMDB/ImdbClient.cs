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
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using log4net;

namespace VideoConvert.Core.Helpers.IMDB
{
    public class ImdbClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ImdbClient));
        private const string ApiUrl = "http://imdbapi.org";
        private const string VersionPath = "api/version?type=xml";
        private const string ApiVersion = "1.2.3";

        private readonly WebClient _client;

        public ImdbClient()
        {
            _client = new WebClient {BaseAddress = ApiUrl};
            CheckApiVersion();
        }

        private void CheckApiVersion()
        {
            ImdbApiVersion version;
            byte[] data = _client.DownloadData(VersionPath);

            using (MemoryStream dataStream = new MemoryStream(data))
            {
                XmlSerializer serializer = new XmlSerializer(typeof (ImdbApiVersion));
                using (XmlReader reader = XmlReader.Create(dataStream))
                {
                    version = (ImdbApiVersion) serializer.Deserialize(reader);
                }
            }

            Log.Info("Server API version: " + version.Version);
            Log.Info("Local API version: " + ApiVersion);

            if (string.CompareOrdinal(version.Version, ApiVersion) != 0)
                Log.Warn("Warning! Local API version differs from Server API version!!!");
        }

        public ImdbMovieData GetMovieById(string movieID, bool fullPlot = false, bool fullAKA = false,
                                          bool fullReleaseDates = false)
        {
            ImdbMovieData result;

            Uri request =
                new Uri(
                    string.Format("/?id={0}&type=xml&plot={1}&aka={2}&release={3}", movieID,
                                  fullPlot ? "full" : "simple", fullAKA ? "full" : "simple",
                                  fullReleaseDates ? "full" : "simple"), UriKind.Relative);

            byte[] data = _client.DownloadData(request);
            using (MemoryStream dataStream = new MemoryStream(data))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ImdbMovieData));
                using (XmlReader reader = XmlReader.Create(dataStream))
                {
                    result = (ImdbMovieData)serializer.Deserialize(reader);
                }
            }

            return result;
        }
    }
}
