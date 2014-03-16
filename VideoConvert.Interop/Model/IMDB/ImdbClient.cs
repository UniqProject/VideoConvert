// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImdbClient.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   IMDB client
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.IMDB
{
    using log4net;
    using System;
    using System.IO;
    using System.Net;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// IMDB client
    /// </summary>
    public class ImdbClient
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ImdbClient));
        private const string ApiUrl = "http://imdbapi.org";
        private const string VersionPath = "api/version?type=xml";
        private const string ApiVersion = "1.2.3";

        private readonly WebClient _client;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ImdbClient()
        {
            _client = new WebClient {BaseAddress = ApiUrl};
            CheckApiVersion();
        }

        private void CheckApiVersion()
        {
            ImdbApiVersion version;
            var data = _client.DownloadData(VersionPath);

            using (var dataStream = new MemoryStream(data))
            {
                var serializer = new XmlSerializer(typeof (ImdbApiVersion));
                using (var reader = XmlReader.Create(dataStream))
                {
                    version = (ImdbApiVersion) serializer.Deserialize(reader);
                }
            }

            Log.Info("Server API version: " + version.Version);
            Log.Info("Local API version: " + ApiVersion);

            if (string.CompareOrdinal(version.Version, ApiVersion) != 0)
                Log.Warn("Warning! Local API version differs from Server API version!!!");
        }

        /// <summary>
        /// Get Movie entry by IMDB ID
        /// </summary>
        /// <param name="movieID">IMDB ID</param>
        /// <param name="fullPlot">Get full plot</param>
        /// <param name="fullAka">Get Full AKA</param>
        /// <param name="fullReleaseDates">Get full release dates</param>
        /// <returns></returns>
        public ImdbMovieData GetMovieById(string movieID, bool fullPlot = false, bool fullAka = false,
                                          bool fullReleaseDates = false)
        {
            ImdbMovieData result;

            var request = new Uri(string.Format("/?id={0}&type=xml&plot={1}&aka={2}&release={3}", movieID,
                                                fullPlot ? "full" : "simple", fullAka ? "full" : "simple",
                                                fullReleaseDates ? "full" : "simple"), UriKind.Relative);

            var data = _client.DownloadData(request);
            using (var dataStream = new MemoryStream(data))
            {
                var serializer = new XmlSerializer(typeof(ImdbMovieData));
                using (var reader = XmlReader.Create(dataStream))
                {
                    result = (ImdbMovieData)serializer.Deserialize(reader);
                }
            }

            return result;
        }
    }
}
