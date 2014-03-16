// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProcessingService.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The Main Processing Service Interface
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Services.Interfaces
{
    using Interop.Model;
    using System;
    using System.ComponentModel;

    /// <summary>
    /// The Main Processing Service Interface
    /// </summary>
    public interface IProcessingService
    {
        /// <summary>
        /// Check folder structure
        /// </summary>
        /// <param name="pathToFile">Path to Folder</param>
        /// <returns><see cref="InputType"/></returns>
        InputType CheckFolderStructure(string pathToFile);

        /// <summary>
        /// Determines Media Type of the input file
        /// </summary>
        /// <param name="pathToFile">Path to input file</param>
        /// <returns><see cref="InputType"/></returns>
        InputType CheckFileType(string pathToFile);

        /// <summary>
        /// Detect the type of input
        /// </summary>
        /// <param name="pathToFile">Path to input file/directory</param>
        /// <returns><see cref="InputType"/></returns>
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
        /// Check if audio stream is DVD compatible
        /// </summary>
        /// <param name="aud"><see cref="AudioInfo"/></param>
        /// <returns>true if stream is DVD compatible, false otherwise</returns>
        bool CheckAudioDvdCompatible(AudioInfo aud);

        /// <summary>
        /// Check if audio stream is Blu-Ray compatible
        /// </summary>
        /// <param name="aud"><see cref="AudioInfo"/></param>
        /// <returns>true if stream is Blu-Ray compatible, false otherwise</returns>
        bool CheckAudioBluRayCompatible(AudioInfo aud);

        /// <summary>
        /// Gets the Description for enum Types
        /// </summary>
        /// <param name="value"><see cref="Enum"/></param>
        /// <returns>string containing the description</returns>
        string StringValueOf(Enum value);

        /// <summary>
        /// Read encoder versions
        /// </summary>
        /// <param name="encPath">Location of encoder executables</param>
        /// <param name="javaPath">Path to java.exe</param>
        void GetAppVersions(string encPath = "", string javaPath = "");

        /// <summary>
        /// Read Updater version
        /// </summary>
        void GetUpdaterVersion();

        /// <summary>
        /// Get version of avisynth plugins archive
        /// </summary>
        void GetAviSynthPluginsVer();

        /// <summary>
        /// Checks if the Application process has elevated rights
        /// </summary>
        /// <returns>true if the process is elevated, false otherwise</returns>
        /// <exception cref="Win32Exception"><see cref="Win32Exception"/></exception>
        bool IsProcessElevated();

        /// <summary>
        /// Check if subtitle needs to be converted for given output type
        /// </summary>
        /// <param name="outputType">Target <see cref="OutputType"/></param>
        /// <param name="format">subtitle format</param>
        /// <returns>true if conversion is needed</returns>
        bool SubtitleNeedConversion(OutputType outputType, string format);

        /// <summary>
        /// Checks if subtitle conversion is supported for given output type
        /// </summary>
        /// <param name="outputType">Target <see cref="OutputType"/></param>
        /// <param name="format">subtitle format</param>
        /// <returns>true if conversion is supported</returns>
        bool SubtitleConversionSupported(OutputType outputType, string format);

        /// <summary>
        /// Check if subtitles conversion is needed / supported for given Job
        /// </summary>
        /// <param name="encodingJob"><see cref="EncodeInfo"/> to check</param>
        void CheckSubtitles(EncodeInfo encodingJob);

        /// <summary>
        /// Check given Job for <see cref="OutputType"/> stream limits
        /// </summary>
        /// <param name="encodingJob"><see cref="EncodeInfo"/> to check</param>
        void CheckStreamLimit(EncodeInfo encodingJob);
    }
}