// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StreamFormat.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Stream format helper class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Stream format helper class
    /// </summary>
    public class StreamFormat
    {
        private readonly string _name;
        private readonly string _profile;
        private readonly string _demux;
        private readonly string _encode;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name">Format name</param>
        /// <param name="demux">Demux file extension</param>
        /// <param name="encode">Encode file extension</param>
        public StreamFormat(string name, string demux, string encode)
        {
            _name = name;
            _profile = string.Empty;
            _demux = demux;
            _encode = encode;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Format name</param>
        /// <param name="profile">Format profile</param>
        /// <param name="demux">Demux file extension</param>
        /// <param name="encode">Encode file extension</param>
        public StreamFormat(string name, string profile, string demux, string encode)
        {
            _name = name;
            _profile = profile;
            _demux = demux;
            _encode = encode;
        }

        /// <summary>
        /// Create default format list
        /// </summary>
        /// <returns></returns>
        private static List<StreamFormat> GenerateList()
        {
            var formatList = new List<StreamFormat>
            {
                new StreamFormat("ac-3", "ac3", "flac"),
                new StreamFormat("ac3", "ac3", "flac"),
                new StreamFormat("ac3-ex", "ac3", "flac"),
                new StreamFormat("eac-3", "eac3", "flac"),
                new StreamFormat("dts", "dts", "flac"),
                new StreamFormat("dts-hd hr", "dtshd", "flac"),
                new StreamFormat("dts-hd", "dtshd", "flac"),
                new StreamFormat("dts-hd ma", "dtshd", "flac"),
                new StreamFormat("dts-es", "dts", "flac"),
                new StreamFormat("mpeg1", "mp2", "flac"),
                new StreamFormat("mpeg1", "layer 2", "mp2", "flac"),
                new StreamFormat("mpeg1", "layer 3", "mp3", "flac"),
                new StreamFormat("mpeg audio", "mp2", "flac"),
                new StreamFormat("mpeg audio", "layer 2", "mp2", "flac"),
                new StreamFormat("mpeg audio", "layer 3", "mp3", "flac"),
                new StreamFormat("pcm", "flac", "flac"),
                new StreamFormat("lpcm", "flac", "flac"),
                new StreamFormat("truehd", "truehd", "flac"),
                new StreamFormat("aac", "aac", "flac"),
                new StreamFormat("flac", "flac", "flac"),
                new StreamFormat("vorbis", "ogg", "flac"),
                new StreamFormat("utf-8", "srt", "srt"),
                new StreamFormat("ssa", "ssa", "ssa"),
                new StreamFormat("ass", "ass", "ass"),
                new StreamFormat("pgs", "sup", "sup"),
                new StreamFormat("vobsub", "sup", "vobsub")
            };

            return formatList;
        }

        /// <summary>
        /// Get file extension for given format
        /// </summary>
        /// <param name="format">Format name</param>
        /// <param name="formatProfile">Format profile</param>
        /// <param name="encode">True for Encoder extension, false for demuxing</param>
        /// <returns></returns>
        public static string GetFormatExtension(string format, string formatProfile, bool encode)
        {
            var stream = GenerateList().Find(sf =>
                                                          {
                                                              if (!String.IsNullOrEmpty(sf._profile))
                                                                  return sf._name.Equals(format.ToLowerInvariant()) &&
                                                                         sf._profile.Equals(
                                                                             formatProfile.ToLowerInvariant());
                                                              return sf._name.Equals(format.ToLowerInvariant());
                                                          });

            if (stream != null)
            {
                return encode ? stream._encode : stream._demux;
            }

            return "flac";
        }
    }
}
