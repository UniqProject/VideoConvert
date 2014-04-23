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
    using Demuxer;
    using DirectShowLib;
    using Encoder;
    using Interfaces;
    using Interop.Model;
    using Interop.Model.MediaInfo;
    using Interop.Utilities;
    using log4net;
    using Muxer;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using Utilities;

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
            this._configService = configService;
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
                Log.InfoFormat("{0} found, select input format {1}", dvdCheck, InputType.InputDvd);
                return InputType.InputDvd;
            }

            if (File.Exists(hddvdCheck))
            {
                Log.InfoFormat("{0} found, select input format {1:s}", hddvdCheck, InputType.InputHddvd);
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
                    switch (version)
                    {
                        case 100:
                            Log.InfoFormat("{0} found, playlist version {1:g}, select input format {2}", bluRayCheck,
                                           version, InputType.InputAvchd.ToString());
                            return InputType.InputAvchd;
                        case 200:
                            Log.InfoFormat("{0} found, playlist version {1:g}, select input format {2}", bluRayCheck,
                                           version, InputType.InputBluRay.ToString());
                            return InputType.InputBluRay;
                    }
                }
            }

            Log.InfoFormat("{0} is unknown folder type", pathToFile);
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

            Log.InfoFormat(CultureInfo.InvariantCulture, "General.FileName:            {0:s}", mi.General.CompleteName);
            Log.InfoFormat(CultureInfo.InvariantCulture, "General.FileExtension:       {0:s}", mi.General.FileExtension);
            Log.InfoFormat(CultureInfo.InvariantCulture, "General.Format:              {0:s}", mi.General.Format);
            Log.InfoFormat(CultureInfo.InvariantCulture, "General.FormatExtensions:    {0:s}", mi.General.FormatExtensions);
            Log.InfoFormat(CultureInfo.InvariantCulture, "General.InternetMediaType:   {0:s}", mi.General.InternetMediaType);
            Log.InfoFormat(CultureInfo.InvariantCulture, "General.EncodedApplication:  {0:s}", mi.General.EncodedApplication);
            Log.InfoFormat(CultureInfo.InvariantCulture, "General.EncodedApplicationUrl:{0:s}", mi.General.EncodedApplicationUrl);
            Log.InfoFormat(CultureInfo.InvariantCulture, "General.EncodedLibrary:      {0:s}", mi.General.EncodedLibrary);
            Log.InfoFormat(CultureInfo.InvariantCulture, "General.EncodedLibraryDate:  {0:s}", mi.General.EncodedLibraryDate);
            Log.InfoFormat(CultureInfo.InvariantCulture, "General.EncodedLibraryName:  {0:s}", mi.General.EncodedLibraryName);
            Log.InfoFormat(CultureInfo.InvariantCulture, "General.EncodedLibrarySettings: {0:s}", mi.General.EncodedLibrarySettings);
            Log.InfoFormat(CultureInfo.InvariantCulture, "General.EncodedLibraryVersion: {0:s}", mi.General.EncodedLibraryVersion);
            Log.Info(String.Empty);

            foreach (var item in mi.Video)
            {
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.ID:                 {0:g}", item.ID);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.StreamKindID:       {0:g}", item.StreamKindID);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.StreamKindPos:      {0:g}", item.StreamKindPos);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.CodecID:            {0:s}", item.CodecID);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.CodecIDInfo:        {0:s}", item.CodecIDInfo);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.CodecIDURL:         {0:s}", item.CodecIDUrl);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.CodecIDDescription: {0:s}", item.CodecIDDescription);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.InternetMediaType:  {0:s}", item.InternetMediaType);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.Format:             {0:s}", item.Format);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.FormatProfile:      {0:s}", item.FormatProfile);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.FormatInfo:         {0:s}", item.FormatInfo);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.FormatVersion:      {0:s}", item.FormatVersion);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.MultiViewBaseProfile: {0:s}", item.MultiViewBaseProfile);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.MultiViewCount:     {0:s}", item.MultiViewCount);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.DisplayAspectRatio: {0:s}", item.DisplayAspectRatio);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.PixelAspectRatio:   {0:g}", item.PixelAspectRatio);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.BitrateMode:        {0:s}", item.BitRateMode);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.Bitrate:            {0:g}", item.BitRate);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.BitrateNom:         {0:g}", item.BitRateNom);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.BitrateMin:         {0:g}", item.BitRateMin);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.BitrateMax:         {0:g}", item.BitRateMax);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.BitDepth:           {0:g}", item.BitDepth);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.FrameRate:          {0:g}", item.FrameRate);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.FrameRateMax:       {0:g}", item.FrameRateMax);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.FrameRateMin:       {0:g}", item.FrameRateMin);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.FrameRateNom:       {0:g}", item.FrameRateNom);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.FrameRateMode:      {0:s}", item.FrameRateMode);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.Height:             {0:g}", item.Height);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.Width:              {0:g}", item.Width);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.VideoSize:          {0:s}", item.VideoSize.ToString());
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.ScanType:           {0:s}", item.ScanType);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.ScanOrder:          {0:g}", item.ScanOrder);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.EncodedApplication: {0:s}", item.EncodedApplication);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.EncodedApplicationUrl: {0:s}", item.EncodedApplicationUrl);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.EncodedLibrary:     {0:s}", item.EncodedLibrary);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.EncodedLibraryDate: {0:s}", item.EncodedLibraryDate);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.EncodedLibraryName: {0:s}", item.EncodedLibraryName);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.EncodedLibrarySettings: {0:s}", item.EncodedLibrarySettings);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Video.EncodedLibraryVersion: {0:s}", item.EncodedLibraryVersion);
            }
            Log.Info(String.Empty);

            foreach (var item in mi.Audio)
            {
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.ID:                 {0:g}", item.ID);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.StreamKindID:       {0:g}", item.StreamKindID);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.StreamKindPos:      {0:g}", item.StreamKindPos);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.CodecID:            {0:s}", item.CodecID);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.CodecIDInfo:        {0:s}", item.CodecIDInfo);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.CodecIDURL:         {0:s}", item.CodecIDUrl);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.CodecIDDescription: {0:s}", item.CodecIDDescription);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.Format:             {0:s}", item.Format);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.FormatProfile:      {0:s}", item.FormatProfile);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.FormatInfo:         {0:s}", item.FormatInfo);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.FormatVersion:      {0:s}", item.FormatVersion);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.Channels:           {0:g}", item.Channels);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.ChannelsString:     {0:s}", item.ChannelsString);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.BitrateMode:        {0:s}", item.BitRateMode);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.Bitrate:            {0:g}", item.BitRate);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.BitrateNom:         {0:g}", item.BitRateNom);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.BitrateMin:         {0:g}", item.BitRateMin);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.BitrateMax:         {0:g}", item.BitRateMax);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.BitDepth:           {0:g}", item.BitDepth);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.SamplingRate:       {0:g}", item.SamplingRate);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.EncodedLibrary:     {0:s}", item.EncodedLibrary);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.EncodedLibraryDate: {0:s}", item.EncodedLibraryDate);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.EncodedLibraryName: {0:s}", item.EncodedLibraryName);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.EncodedLibrarySettings: {0:s}", item.EncodedLibrarySettings);
                Log.InfoFormat(CultureInfo.InvariantCulture, "Audio.EncodedLibraryVersion: {0:s}", item.EncodedLibraryVersion);
            }
            Log.Info(String.Empty);

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
            Log.InfoFormat("Format: {0}, Profile: {1}", aud.Format, aud.FormatProfile);
            Log.InfoFormat("Bitrate: {0:g}, Samplerate: {1:g}, Channel Count: {2:g}", aud.Bitrate, aud.SampleRate,
                           aud.ChannelCount);

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
                        Log.InfoFormat("Bitrate is higher than 448kbit/s");
                        compat = false;
                    }
                }
            }

            if (compat)
            {
                if (aud.ChannelCount > 6)
                {
                    Log.InfoFormat("This channel configuration is not supported");
                    compat = false;
                }
            }

            if (compat)
            {
                if (aud.SampleRate != 48000)
                {
                    Log.InfoFormat("Samplerate != 48000Hz");
                    compat = false;
                }
            }

            return compat;
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
            if (String.IsNullOrEmpty(encPath))
                encPath = _configService.ToolsPath;
            if (String.IsNullOrEmpty(javaPath))
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

            if (!String.IsNullOrEmpty(javaPath))
                _configService.LastBDSup2SubVer = EncoderBdSup2Sub.GetVersionInfo(encPath, javaPath);

            if (Environment.Is64BitOperatingSystem && _configService.Use64BitEncoders)
            {
                _configService.Lastx26464Ver = EncoderX264.GetVersionInfo(encPath, true);
                _configService.Lastffmpeg64Ver = DemuxerFfmpeg.GetVersionInfo(encPath, true);
                _configService.LastLame64Ver = EncoderLame.GetVersionInfo(encPath, true);
            }

            _configService.LastOggEncVer = EncoderOggEnc.GetVersionInfo(encPath, false, this._configService);

            if (_configService.UseOptimizedEncoders)
                _configService.LastOggEncLancerVer = EncoderOggEnc.GetVersionInfo(encPath, true, this._configService);

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

            var graphBuilder = (IGraphBuilder)new FilterGraph();

            var avsFile = new AviSynthGenerator(_configService).GenerateTestFile();

            var result = graphBuilder.RenderFile(avsFile, null);

            Log.DebugFormat("RenderFile Result: {0}", result);

            if (result < 0)
                Log.Debug("AviSynth is not installed");
            else
            {
                var ver = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.SystemDirectory, "avisynth.dll"));
                var appVer = String.Format("{0:g}.{1:g}.{2:g}.{3:g}", ver.FileMajorPart, ver.FileMinorPart,
                                              ver.FileBuildPart, ver.FilePrivatePart);
                Log.DebugFormat("Avisynth version {0} installed", appVer);
                _configService.LastAviSynthVer = appVer;
            }

            File.Delete(avsFile);
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
            if (File.Exists(verFile))
            {
                using (var str = new StreamReader(verFile))
                {
                    _configService.LastAviSynthPluginsVer = str.ReadLine();
                }
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
                if (hToken != null)
                {
                    hToken.Close();
                }
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
                    if (format.ToLowerInvariant() == "pgs")
                        return false;
                    return true;
                case OutputType.OutputMatroska:
                case OutputType.OutputWebM:
                    return false;
                case OutputType.OutputMp4:
                    if (format.ToLowerInvariant() == "ssa" || format.ToLowerInvariant() == "ass")
                        return true;
                    return false;
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
                    if (format.ToLowerInvariant() == "pgs" || format.ToLowerInvariant() == "vobsub")
                        return false;
                    return true;
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