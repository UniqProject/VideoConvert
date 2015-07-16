// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessingService.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The Main Processing Service
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Services
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using DirectShowLib;
    using log4net;
    using VideoConvert.AppServices.Demuxer;
    using VideoConvert.AppServices.Encoder;
    using VideoConvert.AppServices.Muxer;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.AppServices.Utilities;
    using VideoConvert.Interop.Model;
    using VideoConvert.Interop.Model.MediaInfo;
    using VideoConvert.Interop.Utilities;

    /// <summary>
    /// The Main Processing Service
    /// </summary>
    public class ProcessingService : IProcessingService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessingService));
        private readonly IAppConfigService _configService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configService"></param>
        public ProcessingService(IAppConfigService configService)
        {
            _configService = configService;
        }

        /// <summary>
        /// Check folder structure
        /// </summary>
        /// <param name="pathToFile">Path to Folder</param>
        /// <returns><see cref="InputType"/></returns>
        public InputType CheckFolderStructure(string pathToFile)
        {
            var dvdCheck = Path.Combine(pathToFile, "VIDEO_TS\\VIDEO_TS.IFO");
            var hddvdCheck = Path.Combine(pathToFile, "ADV_OBJ\\DISCINFO.XML");
            var bluRayCheck = Path.Combine(pathToFile, "BDMV\\index.bdmv");
            var bluRayAltCheck = Path.Combine(pathToFile, "index.bdmv");

            if (File.Exists(dvdCheck))
            {
                Log.Info($"{dvdCheck} found, select input format {InputType.InputDvd}");
                return InputType.InputDvd;
            }

            if (File.Exists(hddvdCheck))
            {
                Log.Info($"{hddvdCheck} found, select input format {InputType.InputHddvd}");
                return InputType.InputHddvd;
            }

            var blurayExists = File.Exists(bluRayCheck);
            var blurayAltExists = File.Exists(bluRayAltCheck);

            if (blurayExists || blurayAltExists)
            {
                using (var fRead = blurayExists ? File.OpenRead(bluRayCheck) : File.OpenRead(bluRayAltCheck))
                {
                    var buffer = new byte[4];

                    fRead.Seek(4, SeekOrigin.Begin);
                    fRead.Read(buffer, 0, 4);
                    var verString = Encoding.Default.GetString(buffer);
                    var version = Convert.ToInt32(verString);
                    var tempLog = $"{bluRayCheck} found, playlist version {version:0}, select input format ";
                    switch (version)
                    {
                        case 100:
                            Log.Info($"{tempLog}{InputType.InputAvchd}");
                            return InputType.InputAvchd;
                        case 200:
                            Log.Info($"{tempLog}{InputType.InputBluRay}");
                            return InputType.InputBluRay;
                    }
                }
            }

            Log.Info($"{pathToFile} is unknown folder type");
            return InputType.InputUndefined;
        }

        /// <summary>
        /// Determines Media Type of the input file
        /// </summary>
        /// <param name="pathToFile">Path to input file</param>
        /// <returns><see cref="InputType"/></returns>
        public InputType CheckFileType(string pathToFile)
        {

            MediaInfoContainer mi;
            try
            {
                mi = MediaInfoContainer.GetMediaInfo(pathToFile);
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex);
                mi = new MediaInfoContainer();
            }
                
            var containerFormat = mi.General.Format;

            Log.Info($"General.FileName:                {mi.General.CompleteName}");
            Log.Info($"General.FileExtension:           {mi.General.FileExtension}");
            Log.Info($"General.Format:                  {mi.General.Format}");
            Log.Info($"General.FormatExtensions:        {mi.General.FormatExtensions}");
            Log.Info($"General.InternetMediaType:       {mi.General.InternetMediaType}");
            Log.Info($"General.EncodedApplication:      {mi.General.EncodedApplication}");
            Log.Info($"General.EncodedApplicationUrl:   {mi.General.EncodedApplicationUrl}");
            Log.Info($"General.EncodedLibrary:          {mi.General.EncodedLibrary}");
            Log.Info($"General.EncodedLibraryDate:      {mi.General.EncodedLibraryDate}");
            Log.Info($"General.EncodedLibraryName:      {mi.General.EncodedLibraryName}");
            Log.Info($"General.EncodedLibrarySettings:  {mi.General.EncodedLibrarySettings}");
            Log.Info($"General.EncodedLibraryVersion:   {mi.General.EncodedLibraryVersion}");
            Log.Info(string.Empty);

            foreach (var item in mi.Video)
            {
                Log.Info($"Video.ID:                        {item.ID:0}");
                Log.Info($"Video.StreamKindID:              {item.StreamKindID:0}");
                Log.Info($"Video.StreamKindPos:             {item.StreamKindPos:0}");
                Log.Info($"Video.CodecID:                   {item.CodecID}");
                Log.Info($"Video.CodecIDInfo:               {item.CodecIDInfo}");
                Log.Info($"Video.CodecIDURL:                {item.CodecIDUrl}");
                Log.Info($"Video.CodecIDDescription:        {item.CodecIDDescription}");
                Log.Info($"Video.InternetMediaType:         {item.InternetMediaType}");
                Log.Info($"Video.Format:                    {item.Format}");
                Log.Info($"Video.FormatProfile:             {item.FormatProfile}");
                Log.Info($"Video.FormatInfo:                {item.FormatInfo}");
                Log.Info($"Video.FormatVersion:             {item.FormatVersion}");
                Log.Info($"Video.MultiViewBaseProfile:      {item.MultiViewBaseProfile}");
                Log.Info($"Video.MultiViewCount:            {item.MultiViewCount}");
                Log.Info($"Video.DisplayAspectRatio:        {item.DisplayAspectRatio}");
                Log.Info($"Video.PixelAspectRatio:          {item.PixelAspectRatio}");
                Log.Info($"Video.BitrateMode:               {item.BitRateMode}");
                Log.Info($"Video.Bitrate:                   {item.BitRate:0}");
                Log.Info($"Video.BitrateNom:                {item.BitRateNom:0}");
                Log.Info($"Video.BitrateMin:                {item.BitRateMin:0}");
                Log.Info($"Video.BitrateMax:                {item.BitRateMax:0}");
                Log.Info($"Video.BitDepth:                  {item.BitDepth:0}");
                Log.Info($"Video.FrameRate:                 {item.FrameRate:0.###}".ToString(CultureInfo.InvariantCulture));
                Log.Info($"Video.FrameRateMax:              {item.FrameRateMax:0.###}".ToString(CultureInfo.InvariantCulture));
                Log.Info($"Video.FrameRateMin:              {item.FrameRateMin:0.###}".ToString(CultureInfo.InvariantCulture));
                Log.Info($"Video.FrameRateNom:              {item.FrameRateNom:0.###}".ToString(CultureInfo.InvariantCulture));
                Log.Info($"Video.FrameRateMode:             {item.FrameRateMode}");
                Log.Info($"Video.Height:                    {item.Height:0}");
                Log.Info($"Video.Width:                     {item.Width:0}");
                Log.Info($"Video.VideoSize:                 {item.VideoSize}");
                Log.Info($"Video.ScanType:                  {item.ScanType}");
                Log.Info($"Video.ScanOrder:                 {item.ScanOrder}");
                Log.Info($"Video.EncodedApplication:        {item.EncodedApplication}");
                Log.Info($"Video.EncodedApplicationUrl:     {item.EncodedApplicationUrl}");
                Log.Info($"Video.EncodedLibrary:            {item.EncodedLibrary}");
                Log.Info($"Video.EncodedLibraryDate:        {item.EncodedLibraryDate}");
                Log.Info($"Video.EncodedLibraryName:        {item.EncodedLibraryName}");
                Log.Info($"Video.EncodedLibrarySettings:    {item.EncodedLibrarySettings}");
                Log.Info($"Video.EncodedLibraryVersion:     {item.EncodedLibraryVersion}");
            }
            Log.Info(string.Empty);

            foreach (var item in mi.Audio)
            {
                Log.Info($"Audio.ID:                        {item.ID:0}");
                Log.Info($"Audio.StreamKindID:              {item.StreamKindID:0}");
                Log.Info($"Audio.StreamKindPos:             {item.StreamKindPos:0}");
                Log.Info($"Audio.CodecID:                   {item.CodecID}");
                Log.Info($"Audio.CodecIDInfo:               {item.CodecIDInfo}");
                Log.Info($"Audio.CodecIDURL:                {item.CodecIDUrl}");
                Log.Info($"Audio.CodecIDDescription:        {item.CodecIDDescription}");
                Log.Info($"Audio.Format:                    {item.Format}");
                Log.Info($"Audio.FormatProfile:             {item.FormatProfile}");
                Log.Info($"Audio.FormatInfo:                {item.FormatInfo}");
                Log.Info($"Audio.FormatVersion:             {item.FormatVersion}");
                Log.Info($"Audio.Channels:                  {item.Channels:0}");
                Log.Info($"Audio.ChannelsString:            {item.ChannelsString}");
                Log.Info($"Audio.BitrateMode:               {item.BitRateMode}");
                Log.Info($"Audio.Bitrate:                   {item.BitRate:0}");
                Log.Info($"Audio.BitrateNom:                {item.BitRateNom:0}");
                Log.Info($"Audio.BitrateMin:                {item.BitRateMin:0}");
                Log.Info($"Audio.BitrateMax:                {item.BitRateMax:0}");
                Log.Info($"Audio.BitDepth:                  {item.BitDepth:0}");
                Log.Info($"Audio.SamplingRate:              {item.SamplingRate:0}");
                Log.Info($"Audio.EncodedLibrary:            {item.EncodedLibrary}");
                Log.Info($"Audio.EncodedLibraryDate:        {item.EncodedLibraryDate}");
                Log.Info($"Audio.EncodedLibraryName:        {item.EncodedLibraryName}");
                Log.Info($"Audio.EncodedLibrarySettings:    {item.EncodedLibrarySettings}");
                Log.Info($"Audio.EncodedLibraryVersion:     {item.EncodedLibraryVersion}");
            }
            Log.Info(string.Empty);

            switch (containerFormat)
            {
                case "Matroska":
                    return InputType.InputMatroska;
                case "AVI":
                    return InputType.InputAvi;
                case "MPEG-4":
                    return InputType.InputMp4;
                case "BDAV":
                case "MPEG-TS":
                    return InputType.InputTs;
                case "Windows Media":
                    return InputType.InputWm;
                case "Flash Video":
                    return InputType.InputFlash;
                case "MPEG-PS":
                    return InputType.InputMpegps;
                case "WebM":
                    return InputType.InputWebM;
                case "OGG":
                    return InputType.InputOgg;
            }

            return Path.GetExtension(pathToFile) == ".avs" ? InputType.InputAviSynth : InputType.InputUndefined;
        }

        /// <summary>
        /// Detect the type of input
        /// </summary>
        /// <param name="pathToFile">Path to input file/directory</param>
        /// <returns><see cref="InputType"/></returns>
        public InputType DetectInputType(string pathToFile)
        {
            var dir = new DirectoryInfo(pathToFile);
            return (dir.Attributes & FileAttributes.Directory) == FileAttributes.Directory
                       ? CheckFolderStructure(pathToFile)
                       : CheckFileType(pathToFile);
        }

        /// <summary>
        /// reserved for future development
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public bool CheckDvdCompatible(EncodeInfo job)
        {
            return true;
        }

        /// <summary>
        /// reserved for future development
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public bool CheckBluRayCompatible(EncodeInfo job)
        {
            return true;
        }

        /// <summary>
        /// Check if audio stream is DVD compatible
        /// </summary>
        /// <param name="aud"><see cref="AudioInfo"/></param>
        /// <returns>true if stream is DVD compatible, false otherwise</returns>
        public bool CheckAudioDvdCompatible(AudioInfo aud)
        {
            var ext = StreamFormat.GetFormatExtension(aud.Format, aud.FormatProfile, false);

            var compat = true;

            Log.Info("Check if audio is compatible with DVD Spec");
            Log.Info($"Format: {aud.Format}, Profile: {aud.FormatProfile}");
            Log.Info($"Bitrate: {aud.Bitrate:0}, Samplerate: {aud.SampleRate:0}, Channel Count: {aud.ChannelCount:0}");

            if (ext != "ac3")
            {
                Log.Info("Format is not AC3");
                compat = false;
            }

            if (compat)
            {
                if (ext == "ac3")
                {
                    if (aud.Bitrate > 448000)
                    {
                        Log.Info("Bitrate is higher than 448kbit/s");
                        compat = false;
                    }
                }
            }

            if (compat)
            {
                if (aud.ChannelCount > 6)
                {
                    Log.Info("This channel configuration is not supported");
                    compat = false;
                }
            }

            if (!compat) return false;
            if (aud.SampleRate == 48000) return true;

            Log.Info("Samplerate != 48000Hz");

            return false;
        }

        /// <summary>
        /// Check if audio stream is Blu-Ray compatible
        /// </summary>
        /// <param name="aud"><see cref="AudioInfo"/></param>
        /// <returns>true if stream is Blu-Ray compatible, false otherwise</returns>
        public bool CheckAudioBluRayCompatible(AudioInfo aud)
        {
            var ext = StreamFormat.GetFormatExtension(aud.Format, aud.FormatProfile, false);

            var compat = !(ext != "ac3"    &&
                            ext != "eac3"   &&
                            ext != "dts"    &&
                            ext != "dtshd"  &&
                            ext != "mp2"    &&
                            ext != "truehd");

            return compat;
        }

        /// <summary>
        /// Gets the Description for enum Types
        /// </summary>
        /// <param name="value"><see cref="Enum"/></param>
        /// <returns>string containing the description</returns>
        public string StringValueOf(Enum value)
        {
            var fi = value.GetType().GetField(value.ToString("F"));
            var attributes =
                (DescriptionAttribute[]) fi.GetCustomAttributes(typeof (DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString("F");
        }


        // TODO: Get App Versions
        /// <summary>
        /// Read encoder versions
        /// </summary>
        /// <param name="encPath">Location of encoder executables</param>
        /// <param name="javaPath">Path to java.exe</param>
        public void GetAppVersions(string encPath = "", string javaPath = "")
        {
            if (string.IsNullOrEmpty(encPath))
                encPath = _configService.ToolsPath;
            if (string.IsNullOrEmpty(javaPath))
                javaPath = _configService.JavaInstallPath;

            _configService.Lasteac3ToVer = DemuxerEac3To.GetVersionInfo(encPath);
            _configService.LastMplayerVer = DemuxerMplayer.GetVersionInfo(encPath);

            _configService.LastffmpegVer = DemuxerFfmpeg.GetVersionInfo(encPath, false);
            _configService.LastLameVer = EncoderLame.GetVersionInfo(encPath, false);
            _configService.Lastx264Ver = EncoderX264.GetVersionInfo(encPath, false);

            _configService.LastMKVMergeVer = MuxerMkvMerge.GetVersionInfo(encPath);
            _configService.LastDVDAuthorVer = MuxerDvdAuthor.GetVersionInfo(encPath);
            _configService.LastMp4BoxVer = MuxerMp4Box.GetVersionInfo(encPath);
            _configService.LastMJPEGToolsVer = MuxerMplex.GetVersionInfo(encPath);

            if (!string.IsNullOrEmpty(javaPath))
                _configService.LastBDSup2SubVer = EncoderBdSup2Sub.GetVersionInfo(encPath, javaPath);

            if (Environment.Is64BitOperatingSystem && _configService.Use64BitEncoders)
            {
                _configService.Lastx26464Ver = EncoderX264.GetVersionInfo(encPath, true);
                _configService.Lastffmpeg64Ver = DemuxerFfmpeg.GetVersionInfo(encPath, true);
                _configService.LastLame64Ver = EncoderLame.GetVersionInfo(encPath, true);
            }

            _configService.LastOggEncVer = EncoderOggEnc.GetVersionInfo(encPath, false, _configService);

            if (_configService.UseOptimizedEncoders)
                _configService.LastOggEncLancerVer = EncoderOggEnc.GetVersionInfo(encPath, true, _configService);

            _configService.LastNeroAacEncVer = EncoderNeroAac.GetVersionInfo(encPath);

            _configService.LastTSMuxerVer = MuxerTsMuxeR.GetVersionInfo(encPath);

            //LsDvd lsdvd = new LsDvd();
            //ConfigService.LastlsdvdVer = lsdvd.GetVersionInfo(encPath);


            //HcEnc hcenc = new HcEnc();
            //ConfigService.LastHcEncVer = hcenc.GetVersionInfo(encPath);

            //VpxEnc vpxEnc = new VpxEnc();
            //ConfigService.LastVpxEncVer = vpxEnc.GetVersionInfo(encPath);

            //XvidEnc xvidEnc = new XvidEnc();
            //string myVer = xvidEnc.GetVersionInfo(encPath);

            #region Get AviSynth Version

            var graphBuilder = (IGraphBuilder) new FilterGraph();

            var avsFile = new AviSynthGenerator(_configService).GenerateTestFile();

            // workaround for crashes while in debug mode
            var result = 0;
            if (!Debugger.IsAttached)
            {
                try
                {
                    result = graphBuilder.RenderFile(avsFile, null);

                    graphBuilder.Abort();
                    graphBuilder = null;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
            
            Log.Debug($"RenderFile Result: {result}");

            if (result < 0 && !Debugger.IsAttached)
                Log.Debug("AviSynth is not installed");
            else
            {
                var ver = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.SystemDirectory, "avisynth.dll"));
                var appVer = $"{ver.FileMajorPart:0}.{ver.FileMinorPart:0}.{ver.FileBuildPart:0}.{ver.FilePrivatePart:0}";
                Log.Debug($"Avisynth version {appVer} installed");
                _configService.LastAviSynthVer = appVer;
            }

            try
            {
                File.Delete(avsFile);
            }
            catch (Exception ex)
            {
                Log.Error(ex.InnerException);
            }
            
            #endregion

            GetAviSynthPluginsVer();
            GetUpdaterVersion();

            _configService.UpdateVersions = false;
        }

        /// <summary>
        /// Read Updater version
        /// </summary>
        public void GetUpdaterVersion()
        {
            try
            {
                var updaterVer =
                    FileVersionInfo.GetVersionInfo(Path.Combine(_configService.UpdaterPath, @"AppUpdater.exe"));
                _configService.UpdaterVersion = new Version(updaterVer.ProductVersion);
            }
            catch (Exception e)
            {
                Log.Error("unable to get updater version", e);
                _configService.UpdaterVersion = new Version(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Get version of avisynth plugins archive
        /// </summary>
        public void GetAviSynthPluginsVer()
        {
            var verFile = Path.Combine(_configService.AvsPluginsPath, "version");

            if (!File.Exists(verFile)) return;

            using (var str = new StreamReader(verFile))
            {
                _configService.LastAviSynthPluginsVer = str.ReadLine();
            }
        }

        /// <summary>
        /// Checks if the Application process has elevated rights
        /// </summary>
        /// <returns>true if the process is elevated, false otherwise</returns>
        /// <exception cref="Win32Exception"><see cref="Win32Exception"/></exception>
        public bool IsProcessElevated()
        {
            bool fIsElevated;
            SafeTokenHandle hToken = null;
            var pTokenElevation = IntPtr.Zero;

            try
            {
                // Open the access token of the current process with TOKEN_QUERY. 
                if (!NativeMethods.OpenProcessToken(Process.GetCurrentProcess().Handle,
                    NativeMethods.TOKEN_QUERY, out hToken))
                {
                    throw new Win32Exception();
                }

                // Allocate a buffer for the elevation information. 
                var cbTokenElevation = Marshal.SizeOf(typeof(TOKEN_ELEVATION));
                pTokenElevation = Marshal.AllocHGlobal(cbTokenElevation);
                if (pTokenElevation == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                // Retrieve token elevation information. 
                if (!NativeMethods.GetTokenInformation(hToken,
                    TOKEN_INFORMATION_CLASS.TokenElevation,  pTokenElevation,
                    cbTokenElevation, out cbTokenElevation))
                {
                    // When the process is run on operating systems prior to Windows  
                    // Vista, GetTokenInformation returns false with the error code  
                    // ERROR_INVALID_PARAMETER because TokenElevation is not supported  
                    // on those operating systems. 
                    throw new Win32Exception();
                }

                // Marshal the TOKEN_ELEVATION struct from native to .NET object. 
                var elevation = (TOKEN_ELEVATION)Marshal.PtrToStructure(
                    pTokenElevation, typeof(TOKEN_ELEVATION));

                // TOKEN_ELEVATION.TokenIsElevated is a non-zero value if the token  
                // has elevated privileges; otherwise, a zero value. 
                fIsElevated = (elevation.TokenIsElevated != 0);
            }
            finally
            {
                // Centralized cleanup for all allocated resources.  
                hToken?.Close();

                if (pTokenElevation != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pTokenElevation);
                }
            }

            return fIsElevated;
        }

        /// <summary>
        /// Check if subtitle needs to be converted for given output type
        /// </summary>
        /// <param name="outputType">Target <see cref="OutputType"/></param>
        /// <param name="format">subtitle format</param>
        /// <returns>true if conversion is needed</returns>
        public bool SubtitleNeedConversion(OutputType outputType, string format)
        {
            switch (outputType)
            {
                case OutputType.OutputAvchd:
                case OutputType.OutputBluRay:
                case OutputType.OutputM2Ts:
                case OutputType.OutputTs:
                    return format.ToLowerInvariant() != "pgs";
                case OutputType.OutputMatroska:
                case OutputType.OutputWebM:
                    return false;
                case OutputType.OutputMp4:
                    return format.ToLowerInvariant() == "ssa" || format.ToLowerInvariant() == "ass";
                case OutputType.OutputDvd:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if subtitle conversion is supported for given output type
        /// </summary>
        /// <param name="outputType">Target <see cref="OutputType"/></param>
        /// <param name="format">subtitle format</param>
        /// <returns>true if conversion is supported</returns>
        public bool SubtitleConversionSupported(OutputType outputType, string format)
        {
            switch (outputType)
            {
                case OutputType.OutputMp4:
                    return format.ToLowerInvariant() != "pgs" && format.ToLowerInvariant() != "vobsub";
                case OutputType.OutputMatroska:
                case OutputType.OutputAvchd:
                case OutputType.OutputBluRay:
                case OutputType.OutputDvd:
                case OutputType.OutputM2Ts:
                case OutputType.OutputTs:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check if subtitles conversion is needed / supported for given Job
        /// </summary>
        /// <param name="encodingJob"><see cref="EncodeInfo"/> to check</param>
        public void CheckSubtitles(EncodeInfo encodingJob)
        {
            if (encodingJob.EncodingProfile == null) return;

            // WebM Format has no support for subtitles
            if (encodingJob.EncodingProfile.OutFormat == OutputType.OutputWebM)
                encodingJob.SubtitleStreams.Clear();

            foreach (var info in encodingJob.SubtitleStreams)
            {
                info.NeedConversion = SubtitleNeedConversion(encodingJob.EncodingProfile.OutFormat, info.Format) ||
                                      (info.KeepOnlyForcedCaptions && !info.HardSubIntoVideo);
                info.FormatSupported = SubtitleConversionSupported(encodingJob.EncodingProfile.OutFormat, info.Format);
            }

            encodingJob.SubtitleStreams.RemoveAll(info => !info.FormatSupported);
        }

        /// <summary>
        /// Check given Job for <see cref="OutputType"/> stream limits
        /// </summary>
        /// <param name="encodingJob"><see cref="EncodeInfo"/> to check</param>
        public void CheckStreamLimit(EncodeInfo encodingJob)
        {
            if (encodingJob.EncodingProfile == null) return;

            // rearrange default audio stream
            var defaultAudioItem = encodingJob.AudioStreams.Find(info => info.MkvDefault);
            if (defaultAudioItem != null)
            {
                encodingJob.AudioStreams.Remove(defaultAudioItem);
                encodingJob.AudioStreams.Insert(0, defaultAudioItem);
            }

            // rearrange default subtitle stream
            var defaultSubtitleItem = encodingJob.SubtitleStreams.Find(info => info.MkvDefault);
            if (defaultSubtitleItem != null)
            {
                encodingJob.SubtitleStreams.Remove(defaultSubtitleItem);
                encodingJob.SubtitleStreams.Insert(0, defaultSubtitleItem);
            }

            switch (encodingJob.EncodingProfile.OutFormat)
            {
                case OutputType.OutputWebM:
                    // WebM has no support for subtitles
                    encodingJob.SubtitleStreams.Clear();
                    // WebM supports max one audio stream per file
                    var firstIndex = encodingJob.AudioStreams.First();
                    if (firstIndex != null)
                        encodingJob.AudioStreams.RemoveAll(info => info != firstIndex);
                    break;
                case OutputType.OutputDvd:
                    var audioCount = encodingJob.AudioStreams.Count;
                    var subtitleCount = encodingJob.SubtitleStreams.Count;
                    var chapterCount = encodingJob.Chapters.Count;

                    // DVD supports max 8 audio streams
                    if (audioCount > 8)
                        encodingJob.AudioStreams.RemoveRange(8, audioCount - 8);

                    // DVD supports max 32 subtitle streams
                    if (subtitleCount > 32)
                        encodingJob.SubtitleStreams.RemoveRange(32, subtitleCount - 32);

                    // DVD supports max 99 chapter markers
                    if (chapterCount > 99)
                        encodingJob.Chapters.RemoveRange(99, chapterCount - 99);
                    break;
            }
        }
    }
}