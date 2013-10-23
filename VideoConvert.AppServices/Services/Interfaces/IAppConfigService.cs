// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAppConfigService.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
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

    public interface IAppConfigService
    {
        event PropertyChangedEventHandler PropertyChanged;
        string DecodeNamedPipeName { get; }
        string DecodeNamedPipeFullName { get; }
        string EncodeNamedPipeName { get; }
        string EncodeNamedPipeFullName { get; }
        bool UseAviSynthMT { get; set; }
        bool UseHQDeinterlace { get; set; }
        bool EnableSSIF { get; set; }
        bool FilterLoopingPlaylists { get; set; }
        bool FilterShortPlaylists { get; set; }
        int FilterShortPlaylistsValue { get; set; }
        bool KeepStreamOrder { get; set; }
        string ToolsPath { get; set; }
        string JavaInstallPath { get; set; }
        bool JavaInstalled { get; }
        string OutputLocation { get; set; }
        string DemuxLocation { get; set; }
        string Lastx264Ver { get; set; }
        bool X264Installed { get; }
        string Lastx26464Ver { get; set; }
        bool X26464Installed { get; }
        string LastffmpegVer { get; set; }
        bool FfmpegInstalled { get; }
        string Lastffmpeg64Ver { get; set; }
        bool Ffmpeg64Installed { get; }
        string Lasteac3ToVer { get; set; }
        bool Eac3ToInstalled { get; }
        string LastHcEncVer { get; set; }
        bool HcEncInstalled { get; }
        string LastlsdvdVer { get; set; }
        bool LsDvdInstalled { get; }
        string LastMKVMergeVer { get; set; }
        bool MKVMergeInstalled { get; }
        string LastMplayerVer { get; set; }
        bool MplayerInstalled { get; }
        string LastTSMuxerVer { get; set; }
        bool TsMuxerInstalled { get; }
        string LastAviSynthVer { get; set; }
        bool AviSynthInstalled { get; }
        string LastAviSynthPluginsVer { get; set; }
        string LastBDSup2SubVer { get; set; }
        bool BDSup2SubInstalled { get; }
        string LastMp4BoxVer { get; set; }
        bool MP4BoxInstalled { get; }
        string LastMJPEGToolsVer { get; set; }
        bool MjpegToolsInstalled { get; }
        string LastDVDAuthorVer { get; set; }
        bool DVDAuthorInstalled { get; }
        string LastOggEncVer { get; set; }
        bool OggEncInstalled { get; }
        string LastOggEncLancerVer { get; set; }
        bool OggEncLancerInstalled { get; }
        string LastNeroAacEncVer { get; set; }
        bool NeroAacEncInstalled { get; }
        string LastLameVer { get; set; }
        bool LameInstalled { get; }
        string LastLame64Ver { get; set; }
        bool Lame64Installed { get; }
        string LastVpxEncVer { get; set; }
        bool VpxEncInstalled { get; }
        bool FirstStart { get; set; }
        bool ReloadToolVersions { get; set; }
        bool TSMuxeRUseAsyncIO { get; set; }
        bool TSMuxeRBlurayAudioPES { get; set; }
        int TSMuxerSubtitleAdditionalBorder { get; set; }
        int TSMuxeRBottomOffset { get; set; }
        string TSMuxeRSubtitleFont { get; set; }
        string TSMuxeRSubtitleColor { get; set; }
        int TSMuxeRSubtitleFontSize { get; set; }
        bool TSMuxeRVideoTimingInfo { get; set; }
        bool TSMuxeRAddVideoPPS { get; set; }
        bool DeleteCompletedJobs { get; set; }
        int ProcessPriority { get; set; }
        bool DeleteTemporaryFiles { get; set; }
        bool UseDebug { get; set; }
        bool Use64BitEncoders { get; set; }
        bool UseOptimizedEncoders { get; set; }
        bool UseHardwareRendering { get; set; }
        string UseLanguage { get; set; }
        string LastSelectedProfile { get; set; }
        string LastProfilesVer { get; set; }
        bool UpdateVersions { get; set; }
        int UpdateFrequency { get; set; }
        DateTime LastUpdateRun { get; set; }
        bool ShowChangeLog { get; set; }
        bool CreateXbmcInfoFile { get; set; }
        string MovieDBLastLanguage { get; set; }
        string MovieDBLastRatingCountry { get; set; }
        string MovieDBLastFallbackLanguage { get; set; }
        string MovieDBLastFallbackRatingCountry { get; set; }
        string MovieDBPreferredCertPrefix { get; set; }
        string MovieDBFallbackCertPrefix { get; set; }
        int MovieDBRatingSrc { get; set; }
        string TvDBCachePath { get; set; }
        string TvDBParseString { get; set; }
        string TvDBPreferredLanguage { get; set; }
        string TvDBFallbackLanguage { get; set; }
        int LastSelectedSource { get; set; }
        bool UseFfmpegScaling { get; set; }
        bool LimitDecoderThreads { get; set; }
        Extensions SupportedCpuExtensions { get; set; }
        string AppPath { get; }
        string AppSettingsPath { get; }
        string CommonAppSettingsPath { get; }
        string TempPath { get; }
        CultureInfo CInfo { get; }
        Version UpdaterVersion { get; set; }
        string UpdaterPath { get; }
        string AvsPluginsPath { get; }
        ProcessPriorityClass GetProcessPriority();
        ThreadPriority GetThreadPriority();
    }
}