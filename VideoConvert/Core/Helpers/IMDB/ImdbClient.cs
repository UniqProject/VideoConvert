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
