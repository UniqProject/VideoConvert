// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProcessingService.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Services.Interfaces
{
    using System;
    using System.Drawing;
    using System.IO;
    using Interop.Model;
    using Interop.Model.MediaInfo;
    using Interop.Model.Profiles;

    public interface IProcessingService
    {
        InputType CheckFolderStructure(string pathToFile);
        InputType CheckFileType(string pathToFile);
        InputType DetectInputType(string pathToFile);

        /// <summary>
        /// reserved for future development
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        bool CheckDvdCompatible(EncodeInfo job);

        /// <summary>
        /// reserved for future development
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        bool CheckBluRayCompatible(EncodeInfo job);

        /// <summary>
        /// check if audio stream is dvd compatible
        /// </summary>
        /// <param name="aud"></param>
        /// <returns>true if stream is dvd compatible, false otherwise</returns>
        bool CheckAudioDvdCompatible(AudioInfo aud);

        /// <summary>
        /// reserved for future development
        /// </summary>
        /// <param name="aud"></param>
        /// <returns></returns>
        bool CheckAudioBluRayCompatible(AudioInfo aud);


        /// <summary>
        /// Gets the Description for enum Types
        /// </summary>
        /// <param name="value"></param>
        /// <returns>string containing the description</returns>
        string StringValueOf(Enum value);

        void GetAppVersions(string encPath = "", string javaPath = "");
        void GetUpdaterVersion();
        void GetAviSynthPluginsVer();

        void CopyStreamToStream(Stream source, Stream destination, int buffersize,
            Action<Stream, Stream, Exception> completed);

        bool IsProcessElevated();
        bool SubtitleNeedConversion(OutputType outputType, string format);
        bool SubtitleConversionSupported(OutputType outputType, string format);
        void CheckSubtitles(EncodeInfo encodingJob);
        void CheckStreamLimit(EncodeInfo encodingJob);
    }
}