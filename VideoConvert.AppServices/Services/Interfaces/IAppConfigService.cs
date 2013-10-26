// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAppConfigService.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The Application Config Service Interface
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Services.Interfaces
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using Interop.Model;

    /// <summary>
    /// The Application Config Service Interface
    /// </summary>
    public interface IAppConfigService
    {
        /// <summary>
        /// The Property Changed Event Handler
        /// </summary>
        event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Internal decoding Named Pipe
        /// </summary>
        string DecodeNamedPipeName { get; }

        /// <summary>
        /// External decoding Named Pipe
        /// </summary>
        string DecodeNamedPipeFullName { get; }
        
        /// <summary>
        /// Internal encoding Named Pipe
        /// </summary>
        string EncodeNamedPipeName { get; }

        /// <summary>
        /// External encoding Named Pipe
        /// </summary>
        string EncodeNamedPipeFullName { get; }

        /// <summary>
        /// Use MT-Enabled AviSynth
        /// </summary>
        bool UseAviSynthMT { get; set; }

        /// <summary>
        /// Use High Quality Deinterlacing
        /// </summary>
        bool UseHQDeinterlace { get; set; }

        /// <summary>
        /// Enable SSIF Scanning with BDInfoLib
        /// </summary>
        bool EnableSSIF { get; set; }

        /// <summary>
        /// Enable filtering of looping Playlists with BDInfoLib
        /// </summary>
        bool FilterLoopingPlaylists { get; set; }

        /// <summary>
        /// Enable short playlist filtering with BDInfoLib
        /// </summary>
        bool FilterShortPlaylists { get; set; }

        /// <summary>
        /// Minimal playlist length
        /// </summary>
        int FilterShortPlaylistsValue { get; set; }

        /// <summary>
        /// Keep original Stream order
        /// </summary>
        bool KeepStreamOrder { get; set; }

        /// <summary>
        /// Location of encoder executables
        /// </summary>
        string ToolsPath { get; set; }

        /// <summary>
        /// Path to java.exe
        /// </summary>
        string JavaInstallPath { get; set; }

        /// <summary>
        /// Is Java installed?
        /// </summary>
        bool JavaInstalled { get; }

        /// <summary>
        /// Path to output files
        /// </summary>
        string OutputLocation { get; set; }

        /// <summary>
        /// Temp files location
        /// </summary>
        string DemuxLocation { get; set; }

        /// <summary>
        /// Last detected x264 version
        /// </summary>
        string Lastx264Ver { get; set; }

        /// <summary>
        /// Is x264 installed?
        /// </summary>
        bool X264Installed { get; }

        /// <summary>
        /// Last detected x264 version - 64bit build
        /// </summary>
        string Lastx26464Ver { get; set; }

        /// <summary>
        /// Is 64bit build of x264 installed?
        /// </summary>
        bool X26464Installed { get; }

        /// <summary>
        /// Last detected ffmpeg version
        /// </summary>
        string LastffmpegVer { get; set; }

        /// <summary>
        /// Is ffmpeg installed?
        /// </summary>
        bool FfmpegInstalled { get; }

        /// <summary>
        /// Last detected 64bit ffmpeg version
        /// </summary>
        string Lastffmpeg64Ver { get; set; }

        /// <summary>
        /// Is 64bit ffmpeg installed?
        /// </summary>
        bool Ffmpeg64Installed { get; }

        /// <summary>
        /// Last detected eac3to version
        /// </summary>
        string Lasteac3ToVer { get; set; }

        /// <summary>
        /// Is eac3to installed
        /// </summary>
        bool Eac3ToInstalled { get; }

        /// <summary>
        /// Last detected hcEnc version
        /// </summary>
        string LastHcEncVer { get; set; }

        /// <summary>
        /// Is hcEnc installed?
        /// </summary>
        bool HcEncInstalled { get; }

        /// <summary>
        /// Last detected lsdvd version
        /// </summary>
        string LastlsdvdVer { get; set; }

        /// <summary>
        /// Is lsdvd installed?
        /// </summary>
        bool LsDvdInstalled { get; }

        /// <summary>
        /// Last detected mkvmerge version
        /// </summary>
        string LastMKVMergeVer { get; set; }

        /// <summary>
        /// Is mkvmerge installed?
        /// </summary>
        bool MKVMergeInstalled { get; }

        /// <summary>
        /// Last detected mplayer version
        /// </summary>
        string LastMplayerVer { get; set; }

        /// <summary>
        /// Is mplayer installed?
        /// </summary>
        bool MplayerInstalled { get; }

        /// <summary>
        /// Last detected tsMuxeR version
        /// </summary>
        string LastTSMuxerVer { get; set; }

        /// <summary>
        /// Is tsMuxeR installed?
        /// </summary>
        bool TsMuxerInstalled { get; }

        /// <summary>
        /// Last detected AviSynth version
        /// </summary>
        string LastAviSynthVer { get; set; }

        /// <summary>
        /// Is AviSynth installed?
        /// </summary>
        bool AviSynthInstalled { get; }

        /// <summary>
        /// Last detected AviSynth plugins version
        /// </summary>
        string LastAviSynthPluginsVer { get; set; }

        /// <summary>
        /// Last detected BDSup2Sub version
        /// </summary>
        string LastBDSup2SubVer { get; set; }

        /// <summary>
        /// Is BDSup2Sub installed?
        /// </summary>
        bool BDSup2SubInstalled { get; }

        /// <summary>
        /// Last detected mp4box version
        /// </summary>
        string LastMp4BoxVer { get; set; }

        /// <summary>
        /// Is mp4box installed?
        /// </summary>
        bool MP4BoxInstalled { get; }

        /// <summary>
        /// Last detected mjpeg tools version
        /// </summary>
        string LastMJPEGToolsVer { get; set; }

        /// <summary>
        /// Is mjpeg tools installed?
        /// </summary>
        bool MjpegToolsInstalled { get; }

        /// <summary>
        /// Last detected DVDAuthor version
        /// </summary>
        string LastDVDAuthorVer { get; set; }

        /// <summary>
        /// Is DVDAuthor installed?
        /// </summary>
        bool DVDAuthorInstalled { get; }

        /// <summary>
        /// Last detected oggenc version
        /// </summary>
        string LastOggEncVer { get; set; }

        /// <summary>
        /// Is oggenc installed?
        /// </summary>
        bool OggEncInstalled { get; }

        /// <summary>
        /// Last detected Lancer build of oggenc
        /// </summary>
        string LastOggEncLancerVer { get; set; }

        /// <summary>
        /// Is Lancer build of oggenc installed?
        /// </summary>
        bool OggEncLancerInstalled { get; }

        /// <summary>
        /// Last detected NeroAacEnc version
        /// </summary>
        string LastNeroAacEncVer { get; set; }

        /// <summary>
        /// Is NeroAacEnc installed?
        /// </summary>
        bool NeroAacEncInstalled { get; }

        /// <summary>
        /// Last detected Lame version
        /// </summary>
        string LastLameVer { get; set; }

        /// <summary>
        /// Is Lame installed?
        /// </summary>
        bool LameInstalled { get; }

        /// <summary>
        /// Last detected 64bit Lame version
        /// </summary>
        string LastLame64Ver { get; set; }

        /// <summary>
        /// Is 64bit Lame installed?
        /// </summary>
        bool Lame64Installed { get; }

        /// <summary>
        /// Last detected VpxEnc version
        /// </summary>
        string LastVpxEncVer { get; set; }

        /// <summary>
        /// Is VpxEnc installed?
        /// </summary>
        bool VpxEncInstalled { get; }

        /// <summary>
        /// Is this the first time application launch?
        /// </summary>
        bool FirstStart { get; set; }

        /// <summary>
        /// Reload encoder versions
        /// </summary>
        bool ReloadToolVersions { get; set; }

        /// <summary>
        /// Use Async I/0 with tsMuxeR
        /// </summary>
        bool TSMuxeRUseAsyncIO { get; set; }

        /// <summary>
        /// Set Blu-Ray audio PES with tsMuxeR
        /// </summary>
        bool TSMuxeRBlurayAudioPES { get; set; }

        /// <summary>
        /// Subtitle additional Border
        /// </summary>
        int TSMuxerSubtitleAdditionalBorder { get; set; }

        /// <summary>
        /// Subtitle bottom Offset
        /// </summary>
        int TSMuxeRBottomOffset { get; set; }

        /// <summary>
        /// Subtitle font
        /// </summary>
        string TSMuxeRSubtitleFont { get; set; }

        /// <summary>
        /// Subtitle font color
        /// </summary>
        string TSMuxeRSubtitleColor { get; set; }

        /// <summary>
        /// Subtitle font size
        /// </summary>
        int TSMuxeRSubtitleFontSize { get; set; }

        /// <summary>
        /// Add picture timing info for video tracks
        /// </summary>
        bool TSMuxeRVideoTimingInfo { get; set; }

        /// <summary>
        /// Continually insert SPS/PPS for video tracks
        /// </summary>
        bool TSMuxeRAddVideoPPS { get; set; }

        /// <summary>
        /// Remove completed jobs from list
        /// </summary>
        bool DeleteCompletedJobs { get; set; }

        /// <summary>
        /// Process priority of encoding processes
        /// </summary>
        int ProcessPriority { get; set; }

        /// <summary>
        /// Delete temp files
        /// </summary>
        bool DeleteTemporaryFiles { get; set; }

        /// <summary>
        /// Enable debugging
        /// </summary>
        bool UseDebug { get; set; }

        /// <summary>
        /// Make use of 64bit encoders
        /// </summary>
        bool Use64BitEncoders { get; set; }

        /// <summary>
        /// Make use of optimized encoders
        /// </summary>
        bool UseOptimizedEncoders { get; set; }

        /// <summary>
        /// Enable WPF hardware rendering
        /// </summary>
        bool UseHardwareRendering { get; set; }

        /// <summary>
        /// Set application language
        /// </summary>
        string UseLanguage { get; set; }

        /// <summary>
        /// Last selected encoding profile
        /// </summary>
        string LastSelectedProfile { get; set; }

        /// <summary>
        /// Last detected profile list version
        /// </summary>
        string LastProfilesVer { get; set; }

        /// <summary>
        /// Update versions
        /// </summary>
        bool UpdateVersions { get; set; }

        /// <summary>
        /// Update checking frequency
        /// </summary>
        int UpdateFrequency { get; set; }

        /// <summary>
        /// Date of last update
        /// </summary>
        DateTime LastUpdateRun { get; set; }

        /// <summary>
        /// Show changelog after update
        /// </summary>
        bool ShowChangeLog { get; set; }

        /// <summary>
        /// Create XBMC info file (single DB entry)
        /// </summary>
        bool CreateXbmcInfoFile { get; set; }

        /// <summary>
        /// Last selected language for MovieDB
        /// </summary>
        string MovieDBLastLanguage { get; set; }

        /// <summary>
        /// Last selected Rating country for MovieDB
        /// </summary>
        string MovieDBLastRatingCountry { get; set; }

        /// <summary>
        /// Last selected fallback language for MovieDB
        /// </summary>
        string MovieDBLastFallbackLanguage { get; set; }

        /// <summary>
        /// Last selected fallback rating country for MovieDB
        /// </summary>
        string MovieDBLastFallbackRatingCountry { get; set; }

        /// <summary>
        /// Preferred certification prefix for MovieDB
        /// </summary>
        string MovieDBPreferredCertPrefix { get; set; }

        /// <summary>
        /// Fallback certification prefix for MovieDB
        /// </summary>
        string MovieDBFallbackCertPrefix { get; set; }

        /// <summary>
        /// Rating source for MovieDB
        /// </summary>
        int MovieDBRatingSrc { get; set; }

        /// <summary>
        /// Cache path for TvDBLib
        /// </summary>
        string TvDBCachePath { get; set; }

        /// <summary>
        /// Parse string for TVDBLib
        /// </summary>
        string TvDBParseString { get; set; }

        /// <summary>
        /// Preferred language for TVDBLib
        /// </summary>
        string TvDBPreferredLanguage { get; set; }

        /// <summary>
        /// Fallback language for TVDBLib
        /// </summary>
        string TvDBFallbackLanguage { get; set; }

        /// <summary>
        /// Last selected scraping source
        /// </summary>
        int LastSelectedSource { get; set; }

        /// <summary>
        /// Enable ffmpeg scaling/cropping
        /// </summary>
        bool UseFfmpegScaling { get; set; }

        /// <summary>
        /// Limit decoding threads for ffms
        /// </summary>
        bool LimitDecoderThreads { get; set; }

        /// <summary>
        /// List of supported CPU extensions
        /// </summary>
        Extensions SupportedCpuExtensions { get; set; }

        /// <summary>
        /// Path to Application executable
        /// </summary>
        string AppPath { get; }

        /// <summary>
        /// Location where all the settings are stored
        /// </summary>
        string AppSettingsPath { get; }

        /// <summary>
        /// Path to Common Application Data
        /// </summary>
        string CommonAppSettingsPath { get; }

        /// <summary>
        /// System temporary files folder
        /// </summary>
        string TempPath { get; }

        /// <summary>
        /// Global culture info
        /// </summary>
        CultureInfo CInfo { get; }

        /// <summary>
        /// Version of our Updater
        /// </summary>
        Version UpdaterVersion { get; set; }

        /// <summary>
        /// Path to our Updater
        /// </summary>
        string UpdaterPath { get; }

        /// <summary>
        /// Path to AviSynth plugins
        /// </summary>
        string AvsPluginsPath { get; }

        /// <summary>
        /// Get Process Priority from stored setting
        /// </summary>
        /// <returns></returns>
        ProcessPriorityClass GetProcessPriority();

        /// <summary>
        /// Get Thread Priority from stored setting
        /// </summary>
        /// <returns></returns>
        ThreadPriority GetThreadPriority();
    }
}