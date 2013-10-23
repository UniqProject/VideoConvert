// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TvdbLang.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.DataProviders
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using TvdbLib;
    using TvdbLib.Cache;
    using TvdbLib.Data;
    using Model;
    using Utilities;

    public class TvdbLang
    {
        private static TvdbHandler _handler;

        public static List<TvdbLanguage> Languages
        {
            get
            {
                List<TvdbLanguage> result = new List<TvdbLanguage>();
                XmlSerializer serializer = new XmlSerializer(typeof(List<TvdbLanguage>));
                string commPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string cachePath = Path.Combine(commPath, "TvDBCache");

                if (!Directory.Exists(cachePath))
                    Directory.CreateDirectory(cachePath, DirSecurity.CreateDirSecurity(SecurityClass.Everybody));

                if (_handler == null)
                {
                    _handler = new TvdbHandler(new XmlCacheProvider(cachePath), ApiKeys.TheTVDBApiKey);
                }
                try
                {
                    result = _handler.Languages;
                }
                catch (Exception)
                {
                    XmlTextReader xmlTextReader =
                        new XmlTextReader(Path.Combine(cachePath, "tvdblangcache.xml"));
                    result = (List<TvdbLanguage>)serializer.Deserialize(xmlTextReader);
                }
                finally
                {
                    using (
                        FileStream writer = new FileStream(Path.Combine(cachePath, "tvdblangcache.xml"), FileMode.Create,
                            FileAccess.ReadWrite, FileShare.Read))
                    {
                        serializer.Serialize(writer, result);
                    }
                }
                return result;
            }
        }
    }
}