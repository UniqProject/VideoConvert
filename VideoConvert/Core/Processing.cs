//============================================================================
// VideoConvert - Fast Video & Audio Conversion Tool
// Copyright © 2012 JT-Soft
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using VideoConvert.Core.Encoder;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Media;
using VideoConvert.Core.Profiles;
using log4net;
using System.Diagnostics;
using DirectShowLib;
using Size = System.Drawing.Size;

namespace VideoConvert.Core
{
    class Processing
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Processing));

        public static MediaInfo MyMediaInfo;

        private static InputType CheckFolderStructure(string pathToFile)
        {
            string dvdCheck = Path.Combine(pathToFile, "VIDEO_TS\\VIDEO_TS.IFO");
            string hddvdCheck = Path.Combine(pathToFile, "ADV_OBJ\\DISCINFO.XML");
            string bluRayCheck = Path.Combine(pathToFile, "BDMV\\index.bdmv");
            string bluRayAltCheck = Path.Combine(pathToFile, "index.bdmv");

            if (File.Exists(dvdCheck))
            {
                Log.InfoFormat("{0:s} found, select input format {1:s}", dvdCheck, InputType.InputDvd.ToString());
                return InputType.InputDvd;
            }

            if (File.Exists(hddvdCheck))
            {
                Log.InfoFormat("{0:s} found, select input format {1:s}", hddvdCheck, InputType.InputHddvd.ToString());
                return InputType.InputHddvd;
            }

            bool blurayExists = File.Exists(bluRayCheck);
            bool blurayAltExists = File.Exists(bluRayAltCheck);

            if (blurayExists || blurayAltExists)
            {
                using (FileStream fRead = blurayExists ? File.OpenRead(bluRayCheck) : File.OpenRead(bluRayAltCheck))
                {
                    byte[] buffer = new byte[4];

                    fRead.Seek(4, SeekOrigin.Begin);
                    fRead.Read(buffer, 0, 4);
                    string verString = Encoding.Default.GetString(buffer);
                    int version = Convert.ToInt32(verString);
                    switch (version)
                    {
                        case 100:
                            Log.InfoFormat("{0:s} found, playlist version {1:g}, select input format {2:s}", bluRayCheck,
                                           version, InputType.InputAvchd.ToString());
                            return InputType.InputAvchd;
                        case 200:
                            Log.InfoFormat("{0:s} found, playlist version {1:g}, select input format {2:s}", bluRayCheck,
                                           version, InputType.InputBluRay.ToString());
                            return InputType.InputBluRay;
                    }
                }
            }

            Log.InfoFormat("{0:s} is unknown folder type", pathToFile);
            return InputType.InputUndefined;
        }

        private static InputType CheckFileType(string pathToFile)
        {

            MediaInfoContainer mi;
            try
            {
                mi = GetMediaInfo(pathToFile);
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex);
                mi = new MediaInfoContainer();
            }
                

            string containerFormat = mi.General.Format;

            Log.InfoFormat(AppSettings.CInfo, "General.FileName:            {0:s}", mi.General.CompleteName);
            Log.InfoFormat(AppSettings.CInfo, "General.FileExtension:       {0:s}", mi.General.FileExtension);
            Log.InfoFormat(AppSettings.CInfo, "General.Format:              {0:s}", mi.General.Format);
            Log.InfoFormat(AppSettings.CInfo, "General.FormatExtensions:    {0:s}", mi.General.FormatExtensions);
            Log.InfoFormat(AppSettings.CInfo, "General.InternetMediaType:   {0:s}", mi.General.InternetMediaType);
            Log.InfoFormat(AppSettings.CInfo, "General.EncodedApplication:  {0:s}", mi.General.EncodedApplication);
            Log.InfoFormat(AppSettings.CInfo, "General.EncodedApplicationUrl:{0:s}", mi.General.EncodedApplicationUrl);
            Log.InfoFormat(AppSettings.CInfo, "General.EncodedLibrary:      {0:s}", mi.General.EncodedLibrary);
            Log.InfoFormat(AppSettings.CInfo, "General.EncodedLibraryDate:  {0:s}", mi.General.EncodedLibraryDate);
            Log.InfoFormat(AppSettings.CInfo, "General.EncodedLibraryName:  {0:s}", mi.General.EncodedLibraryName);
            Log.InfoFormat(AppSettings.CInfo, "General.EncodedLibrarySettings: {0:s}", mi.General.EncodedLibrarySettings);
            Log.InfoFormat(AppSettings.CInfo, "General.EncodedLibraryVersion: {0:s}", mi.General.EncodedLibraryVersion);
            Log.Info(String.Empty);

            foreach (MediaInfoContainer.VideoStreamInfo item in mi.Video)
            {
                Log.InfoFormat(AppSettings.CInfo, "Video.ID:                 {0:g}", item.ID);
                Log.InfoFormat(AppSettings.CInfo, "Video.StreamKindID:       {0:g}", item.StreamKindID);
                Log.InfoFormat(AppSettings.CInfo, "Video.StreamKindPos:      {0:g}", item.StreamKindPos);
                Log.InfoFormat(AppSettings.CInfo, "Video.CodecID:            {0:s}", item.CodecID);
                Log.InfoFormat(AppSettings.CInfo, "Video.CodecIDInfo:        {0:s}", item.CodecIDInfo);
                Log.InfoFormat(AppSettings.CInfo, "Video.CodecIDURL:         {0:s}", item.CodecIDUrl);
                Log.InfoFormat(AppSettings.CInfo, "Video.CodecIDDescription: {0:s}", item.CodecIDDescription);
                Log.InfoFormat(AppSettings.CInfo, "Video.InternetMediaType:  {0:s}", item.InternetMediaType);
                Log.InfoFormat(AppSettings.CInfo, "Video.Format:             {0:s}", item.Format);
                Log.InfoFormat(AppSettings.CInfo, "Video.FormatProfile:      {0:s}", item.FormatProfile);
                Log.InfoFormat(AppSettings.CInfo, "Video.FormatInfo:         {0:s}", item.FormatInfo);
                Log.InfoFormat(AppSettings.CInfo, "Video.FormatVersion:      {0:s}", item.FormatVersion);
                Log.InfoFormat(AppSettings.CInfo, "Video.MultiViewBaseProfile: {0:s}", item.MultiViewBaseProfile);
                Log.InfoFormat(AppSettings.CInfo, "Video.MultiViewCount:     {0:s}", item.MultiViewCount);
                Log.InfoFormat(AppSettings.CInfo, "Video.DisplayAspectRatio: {0:s}", item.DisplayAspectRatio);
                Log.InfoFormat(AppSettings.CInfo, "Video.PixelAspectRatio:   {0:g}", item.PixelAspectRatio);
                Log.InfoFormat(AppSettings.CInfo, "Video.BitrateMode:        {0:s}", item.BitRateMode);
                Log.InfoFormat(AppSettings.CInfo, "Video.Bitrate:            {0:g}", item.BitRate);
                Log.InfoFormat(AppSettings.CInfo, "Video.BitrateNom:         {0:g}", item.BitRateNom);
                Log.InfoFormat(AppSettings.CInfo, "Video.BitrateMin:         {0:g}", item.BitRateMin);
                Log.InfoFormat(AppSettings.CInfo, "Video.BitrateMax:         {0:g}", item.BitRateMax);
                Log.InfoFormat(AppSettings.CInfo, "Video.BitDepth:           {0:g}", item.BitDepth);
                Log.InfoFormat(AppSettings.CInfo, "Video.FrameRate:          {0:g}", item.FrameRate);
                Log.InfoFormat(AppSettings.CInfo, "Video.FrameRateMax:       {0:g}", item.FrameRateMax);
                Log.InfoFormat(AppSettings.CInfo, "Video.FrameRateMin:       {0:g}", item.FrameRateMin);
                Log.InfoFormat(AppSettings.CInfo, "Video.FrameRateNom:       {0:g}", item.FrameRateNom);
                Log.InfoFormat(AppSettings.CInfo, "Video.FrameRateMode:      {0:s}", item.FrameRateMode);
                Log.InfoFormat(AppSettings.CInfo, "Video.Height:             {0:g}", item.Height);
                Log.InfoFormat(AppSettings.CInfo, "Video.Width:              {0:g}", item.Width);
                Log.InfoFormat(AppSettings.CInfo, "Video.VideoSize:          {0:s}", item.VideoSize.ToString());
                Log.InfoFormat(AppSettings.CInfo, "Video.ScanType:           {0:s}", item.ScanType);
                Log.InfoFormat(AppSettings.CInfo, "Video.ScanOrder:          {0:g}", item.ScanOrder);
                Log.InfoFormat(AppSettings.CInfo, "Video.EncodedApplication: {0:s}", item.EncodedApplication);
                Log.InfoFormat(AppSettings.CInfo, "Video.EncodedApplicationUrl: {0:s}", item.EncodedApplicationUrl);
                Log.InfoFormat(AppSettings.CInfo, "Video.EncodedLibrary:     {0:s}", item.EncodedLibrary);
                Log.InfoFormat(AppSettings.CInfo, "Video.EncodedLibraryDate: {0:s}", item.EncodedLibraryDate);
                Log.InfoFormat(AppSettings.CInfo, "Video.EncodedLibraryName: {0:s}", item.EncodedLibraryName);
                Log.InfoFormat(AppSettings.CInfo, "Video.EncodedLibrarySettings: {0:s}", item.EncodedLibrarySettings);
                Log.InfoFormat(AppSettings.CInfo, "Video.EncodedLibraryVersion: {0:s}", item.EncodedLibraryVersion);
            }
            Log.Info(String.Empty);

            foreach (MediaInfoContainer.AudioStreamInfo item in mi.Audio)
            {
                Log.InfoFormat(AppSettings.CInfo, "Audio.ID:                 {0:g}", item.ID);
                Log.InfoFormat(AppSettings.CInfo, "Audio.StreamKindID:       {0:g}", item.StreamKindID);
                Log.InfoFormat(AppSettings.CInfo, "Audio.StreamKindPos:      {0:g}", item.StreamKindPos);
                Log.InfoFormat(AppSettings.CInfo, "Audio.CodecID:            {0:s}", item.CodecID);
                Log.InfoFormat(AppSettings.CInfo, "Audio.CodecIDInfo:        {0:s}", item.CodecIDInfo);
                Log.InfoFormat(AppSettings.CInfo, "Audio.CodecIDURL:         {0:s}", item.CodecIDUrl);
                Log.InfoFormat(AppSettings.CInfo, "Audio.CodecIDDescription: {0:s}", item.CodecIDDescription);
                Log.InfoFormat(AppSettings.CInfo, "Audio.Format:             {0:s}", item.Format);
                Log.InfoFormat(AppSettings.CInfo, "Audio.FormatProfile:      {0:s}", item.FormatProfile);
                Log.InfoFormat(AppSettings.CInfo, "Audio.FormatInfo:         {0:s}", item.FormatInfo);
                Log.InfoFormat(AppSettings.CInfo, "Audio.FormatVersion:      {0:s}", item.FormatVersion);
                Log.InfoFormat(AppSettings.CInfo, "Audio.Channels:           {0:g}", item.Channels);
                Log.InfoFormat(AppSettings.CInfo, "Audio.ChannelsString:     {0:s}", item.ChannelsString);
                Log.InfoFormat(AppSettings.CInfo, "Audio.BitrateMode:        {0:s}", item.BitRateMode);
                Log.InfoFormat(AppSettings.CInfo, "Audio.Bitrate:            {0:g}", item.BitRate);
                Log.InfoFormat(AppSettings.CInfo, "Audio.BitrateNom:         {0:g}", item.BitRateNom);
                Log.InfoFormat(AppSettings.CInfo, "Audio.BitrateMin:         {0:g}", item.BitRateMin);
                Log.InfoFormat(AppSettings.CInfo, "Audio.BitrateMax:         {0:g}", item.BitRateMax);
                Log.InfoFormat(AppSettings.CInfo, "Audio.BitDepth:           {0:g}", item.BitDepth);
                Log.InfoFormat(AppSettings.CInfo, "Audio.SamplingRate:       {0:g}", item.SamplingRate);
                Log.InfoFormat(AppSettings.CInfo, "Audio.EncodedLibrary:     {0:s}", item.EncodedLibrary);
                Log.InfoFormat(AppSettings.CInfo, "Audio.EncodedLibraryDate: {0:s}", item.EncodedLibraryDate);
                Log.InfoFormat(AppSettings.CInfo, "Audio.EncodedLibraryName: {0:s}", item.EncodedLibraryName);
                Log.InfoFormat(AppSettings.CInfo, "Audio.EncodedLibrarySettings: {0:s}", item.EncodedLibrarySettings);
                Log.InfoFormat(AppSettings.CInfo, "Audio.EncodedLibraryVersion: {0:s}", item.EncodedLibraryVersion);
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

        public static InputType DetectInputType(string pathToFile)
        {
            DirectoryInfo dir = new DirectoryInfo(pathToFile);
            return (dir.Attributes & FileAttributes.Directory) == FileAttributes.Directory
                       ? CheckFolderStructure(pathToFile)
                       : CheckFileType(pathToFile);
        }

        /// <summary>
        /// reserved for future development
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public static bool CheckDvdCompatible(EncodeInfo job)
        {
            return true;
        }

        /// <summary>
        /// reserved for future development
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public static bool CheckBluRayCompatible(EncodeInfo job)
        {
            return true;
        }

        /// <summary>
        /// check if audio stream is dvd compatible
        /// </summary>
        /// <param name="aud"></param>
        /// <returns>true if stream is dvd compatible, false otherwise</returns>
        public static bool CheckAudioDvdCompatible(AudioInfo aud)
        {
            string ext = StreamFormat.GetFormatExtension(aud.Format, aud.FormatProfile, false);

            bool compat = true;

            Log.Info("Check if audio is compatible with DVD Spec");
            Log.InfoFormat("Format: {0:s}, Profile: {1:s}", aud.Format, aud.FormatProfile);
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
        /// reserved for future development
        /// </summary>
        /// <param name="aud"></param>
        /// <returns></returns>
        public static bool CheckAudioBluRayCompatible(AudioInfo aud)
        {
            string ext = StreamFormat.GetFormatExtension(aud.Format, aud.FormatProfile, false);

            bool compat = !(ext != "ac3"    &&
                            ext != "eac3"   &&
                            ext != "dts"    &&
                            ext != "dtshd"  &&
                            ext != "mp2"    &&
                            ext != "truehd");

            return compat;
        }

        public static string CreateTempFile(string extension)
        {
            return CreateTempFile(null, extension);
        }

        public static string CreateTempFile(string baseName, string extension)
        {
            string output;
            if (String.IsNullOrEmpty(baseName))
                output = Path.ChangeExtension(Path.Combine(AppSettings.DemuxLocation, Guid.NewGuid().ToString()),
                                              extension);
            else
            {
                if (String.IsNullOrEmpty(Path.GetDirectoryName(baseName)))
                    output = Path.Combine(AppSettings.DemuxLocation, String.Format("{0}.{1}", baseName, extension));
                else
                {
                    string inFile = Path.GetFileNameWithoutExtension(baseName);
                    output = Path.Combine(AppSettings.DemuxLocation, String.Format("{0}.{1}", inFile, extension));
                }
            }

            if (output.LastIndexOf('.') == output.Length - 1)
                output = output.Remove(output.Length - 1);

            return output;
        }

        public static void GetFPSNumDenom(float fps, out int fpsEnumerator, out int fpsDenominator)
        {
            int tempFrameRate = Convert.ToInt32(Math.Round(fps, 3) * 1000);

            fpsEnumerator = 0;
            fpsDenominator = 0;

            switch (tempFrameRate)
            {
                case 23976: // 23.976
                    fpsEnumerator = 24000;
                    fpsDenominator = 1001;
                    break;
                case 24000: // 24
                    fpsEnumerator = 24000;
                    fpsDenominator = 1000;
                    break;
                case 25000: // 25
                    fpsEnumerator = 25000;
                    fpsDenominator = 1000;
                    break;
                case 29970: // 29.97
                    fpsEnumerator = 30000;
                    fpsDenominator = 1001;
                    break;
                case 30000: // 30
                    fpsEnumerator = 30000;
                    fpsDenominator = 1000;
                    break;
                case 50000: // 50
                    fpsEnumerator = 50000;
                    fpsDenominator = 1000;
                    break;
                case 59940: // 59.94
                    fpsEnumerator = 60000;
                    fpsDenominator = 1001;
                    break;
                case 60000: // 60
                    fpsEnumerator = 60000;
                    fpsDenominator = 1000;
                    break;
                case 0: // Forbidden
                    break;
                default: // Reserved
                    fpsEnumerator = tempFrameRate;
                    fpsDenominator = 1000;
                    break;
            }
        }

        // TODO: check overhead calculation
        public static int CalculateVideoBitrate(EncodeInfo jobInfo)
        {
            const double tsOverhead = 0.1D; // 10%
            const double mkvOverhead = 0.005D; // 0.5%
            const double dvdOverhead = 0.04D; // 4%
            
            ulong targetSize = jobInfo.EncodingProfile.TargetFileSize;
            double overhead = 0D;

            switch (jobInfo.EncodingProfile.OutFormat)
            {
                case OutputType.OutputBluRay:
                    overhead = tsOverhead;
                    break;
                case OutputType.OutputAvchd:
                    overhead = tsOverhead;
                    break;
                case OutputType.OutputMatroska:
                case OutputType.OutputWebM:
                    overhead = mkvOverhead;
                    break;
                case OutputType.OutputMp4:
                    break;
                case OutputType.OutputTs:
                case OutputType.OutputM2Ts:
                    overhead = tsOverhead;
                    break;
                case OutputType.OutputDvd:
                    overhead = dvdOverhead;
                    break;
            }
                
            ulong streamSizes = 0;

            int maxRate = -1;

            if (jobInfo.VideoProfile.Type == ProfileType.X264)
                maxRate = CalculateMaxRatex264((X264Profile)jobInfo.VideoProfile, jobInfo.EncodingProfile.OutFormat);

            foreach (AudioInfo item in jobInfo.AudioStreams)
            {
                streamSizes += item.StreamSize + (ulong)Math.Floor(item.StreamSize * overhead);
                if ((item.IsHdStream) && (Math.Abs(overhead - tsOverhead) <= 0))
                    streamSizes += (ulong)Math.Floor(item.StreamSize * 0.03D);
            }
            streamSizes = jobInfo.SubtitleStreams.Aggregate(streamSizes,
                                                            (current, item) =>
                                                            current +
                                                            (item.StreamSize +
                                                             (ulong) Math.Floor(item.StreamSize*overhead)));

            ulong sizeRemains = targetSize - streamSizes;

            sizeRemains -= (ulong)Math.Floor(sizeRemains * overhead);
            int bitrateCalc = (int)Math.Floor(sizeRemains / jobInfo.VideoStream.Length / 1000 * 8);

            if (jobInfo.EncodingProfile.OutFormat == OutputType.OutputDvd)
                bitrateCalc = Math.Min(bitrateCalc, 8000);

            if (maxRate > -1)
                bitrateCalc = Math.Min(bitrateCalc, maxRate);

            return bitrateCalc;
        }

        private static int CalculateMaxRatex264(X264Profile x264Prof, OutputType outType)
        {
            int[] baseLineBitrates =
            {
                64, 192, 384, 786, 2000, 4000, 4000, 10000, 14000, 20000, 20000, 50000, 50000,
                135000, 240000, -1
            };

            int[] highBitrates =
            {
                80, 240, 480, 960, 2500, 5000, 5000, 12500, 17500, 25000, 25000, 62500, 62500,
                168750, 300000, -1
            };

            const int maxBlurayBitrate = 40000;

            if (outType == OutputType.OutputBluRay)
                return maxBlurayBitrate;

            return x264Prof.AVCProfile < 2 ? baseLineBitrates[x264Prof.AVCLevel] : highBitrates[x264Prof.AVCLevel];
        }

        public static Size GetVideoDimensions(VideoFormat videoFormat, float aspect, OutputType outType)
        {
            Size outPut = new Size();
            double aspect16To9 = Math.Round(16 / (double)9, 3);
            double aspect16To10 = Math.Round(16 / (double)10, 3);

            switch (videoFormat)
            {
                case VideoFormat.Videoformat480I:
                case VideoFormat.Videoformat480P:
                    if ((Math.Abs(Math.Round(aspect, 3) - aspect16To9) < 0) && (outType == OutputType.OutputDvd))
                    {
                        outPut.Width = 1024;
                    }
                    else
                    {
                        outPut.Width = 720;
                    }
                    outPut.Height = 480;
                    break;
                case VideoFormat.Videoformat576I:
                case VideoFormat.Videoformat576P:
                    if ((Math.Abs(Math.Round(aspect, 3) - aspect16To9) < 0) && (outType == OutputType.OutputDvd))
                    {
                        outPut.Width = 1024;
                    }
                    else
                    {
                        outPut.Width = 720;
                    }
                    outPut.Height = 576;
                    break;
                case VideoFormat.Videoformat720P:
                    outPut.Width = 1280;
                    outPut.Height = 720;
                    break;
                case VideoFormat.Videoformat1080I:
                case VideoFormat.Videoformat1080P:
                    if (Math.Round(aspect, 3) >= aspect16To9 || Math.Round(aspect, 3) >= aspect16To10)
                    {
                        outPut.Width = 1920;
                    }
                    else
                    {
                        outPut.Width = 1440;
                    }
                    outPut.Height = 1080;
                    break;
            }

            return outPut;
        }

        // Get media information with an 10 sec timeout
        delegate MediaInfoContainer MiWorkDelegate(string fileName);
        internal static MediaInfoContainer GetMediaInfo(string fileName)
        {
            MiWorkDelegate d = DoWorkHandler;
            IAsyncResult res = d.BeginInvoke(fileName, null, null);
            if (res.IsCompleted == false)
            {
                res.AsyncWaitHandle.WaitOne(10000, false);
                if (res.IsCompleted == false)
                    throw new TimeoutException("Could not open media file!");
            }
            return d.EndInvoke(res);
        }

        private static MediaInfoContainer DoWorkHandler(string fileName)
        {
            MediaInfoContainer mi = new MediaInfoContainer();
            mi.GetMediaInfo(fileName);
            return mi;
        }

        /// <summary>
        /// Gets the Description for enum Types
        /// </summary>
        /// <param name="value"></param>
        /// <returns>string containing the description</returns>
        internal static string StringValueOf(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString("F"));
            DescriptionAttribute[] attributes =
                (DescriptionAttribute[]) fi.GetCustomAttributes(typeof (DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString("F");
        }

        internal static string GetAsciiFileName(string fileName)
        {
            return
                Encoding.ASCII.GetString(Encoding.Convert(Encoding.Unicode, Encoding.ASCII,
                                                          Encoding.Unicode.GetBytes(fileName))).Replace('?', '_');
        }

        public static ulong GetFileSize(string fName)
        {
            return (ulong)new FileInfo(fName).Length;
        }

        public static void GetAppVersions(string encPath = "", string javaPath = "")
        {
            if (String.IsNullOrEmpty(encPath))
                encPath = AppSettings.ToolsPath;
            if (String.IsNullOrEmpty(javaPath))
                javaPath = AppSettings.JavaInstallPath;

            X264 x264Enc = new X264();
            AppSettings.Lastx264Ver = x264Enc.GetVersionInfo(encPath,false);

            if (Environment.Is64BitOperatingSystem)
                AppSettings.Lastx26464Ver = x264Enc.GetVersionInfo(encPath, true);

            FfMpeg ffmpeg = new FfMpeg();
            AppSettings.LastffmpegVer = ffmpeg.GetVersionInfo(encPath, false);

            if (Environment.Is64BitOperatingSystem)
                AppSettings.Lastffmpeg64Ver = ffmpeg.GetVersionInfo(encPath, true);

            Eac3To eac3To = new Eac3To();
            AppSettings.Lasteac3ToVer = eac3To.GetVersionInfo(encPath);

            LsDvd lsdvd = new LsDvd();
            AppSettings.LastlsdvdVer = lsdvd.GetVersionInfo(encPath);

            MkvMerge mkvMerge = new MkvMerge();
            AppSettings.LastMKVMergeVer = mkvMerge.GetVersionInfo(encPath);

            MPlayer mplayer = new MPlayer();
            AppSettings.LastMplayerVer = mplayer.GetVersionInfo(encPath);

            TsMuxeR tsmuxer = new TsMuxeR();
            AppSettings.LastTSMuxerVer = tsmuxer.GetVersionInfo(encPath);

            MJpeg mjpeg = new MJpeg();
            AppSettings.LastMJPEGToolsVer = mjpeg.GetVersionInfo(encPath);

            DvdAuthor dvdauthor = new DvdAuthor();
            AppSettings.LastDVDAuthorVer = dvdauthor.GetVersionInfo(encPath);

            MP4Box mp4Box = new MP4Box();
            AppSettings.LastMp4BoxVer = mp4Box.GetVersionInfo(encPath);

            HcEnc hcenc = new HcEnc();
            AppSettings.LastHcEncVer = hcenc.GetVersionInfo(encPath);

            OggEnc ogg = new OggEnc();
            AppSettings.LastOggEncVer = ogg.GetVersionInfo(encPath,false);

            if (AppSettings.UseOptimizedEncoders)
                AppSettings.LastOggEncLancerVer = ogg.GetVersionInfo(encPath, true);

            NeroAACEnc aac = new NeroAACEnc();
            AppSettings.LastNeroAacEncVer = aac.GetVersionInfo(encPath);

            Lame lame = new Lame();
            AppSettings.LastLameVer = lame.GetVersionInfo(encPath);

            VpxEnc vpxEnc = new VpxEnc();
            AppSettings.LastVpxEncVer = vpxEnc.GetVersionInfo(encPath);

            if (!String.IsNullOrEmpty(javaPath))
            {
                BdSup2SubTool bdSup2Sub = new BdSup2SubTool();
                AppSettings.LastBDSup2SubVer = bdSup2Sub.GetVersionInfo(encPath, javaPath);
            }

            #region Get AviSynth Version

            IGraphBuilder graphBuilder = (IGraphBuilder)new FilterGraph();

            string avsFile = AviSynthGenerator.GenerateTestFile();

            int result = graphBuilder.RenderFile(avsFile, null);
            
            Log.DebugFormat("RenderFile Result: {0}", result);

            if (result < 0)
                Log.Debug("AviSynth is not installed");
            else
            {
                FileVersionInfo ver = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.SystemDirectory, "avisynth.dll"));
                string appVer = String.Format("{0:g}.{1:g}.{2:g}.{3:g}", ver.FileMajorPart, ver.FileMinorPart,
                                              ver.FileBuildPart, ver.FilePrivatePart);
                Log.DebugFormat("Avisynth version {0:s} installed", appVer);
                AppSettings.LastAviSynthVer = appVer;
            }

            File.Delete(avsFile);
            #endregion

            GetAviSynthPluginsVer();
            GetUpdaterVersion();

            AppSettings.UpdateVersions = false;
            AppSettings.SaveSettings();
        }

        public static void GetUpdaterVersion()
        {
            try
            {
                FileVersionInfo updaterVer =
                    FileVersionInfo.GetVersionInfo(Path.Combine(AppSettings.UpdaterPath, @"AppUpdater.exe"));
                AppSettings.UpdaterVersion = new Version(updaterVer.ProductVersion);
            }
            catch (Exception e)
            {
                Log.Error("unable to get updater version", e);
                AppSettings.UpdaterVersion = new Version(0, 0, 0, 0);
            }
        }

        public static void GetAviSynthPluginsVer()
        {
            string verFile = Path.Combine(AppSettings.AvsPluginsPath, "version");
            if (File.Exists(verFile))
            {
                using (StreamReader str = new StreamReader(verFile))
                {
                    AppSettings.LastAviSynthPluginsVer = str.ReadLine();
                }
            }
        }

        public static string GetResourceString(string key)
        {
            string resString = String.Empty;
            if (Application.Current != null)
            {
                ResourceDictionary dict = Application.Current.TryFindResource("StringRes") as ResourceDictionary;
                if (dict != null && dict.MergedDictionaries.Count > 0)
                {
                    resString = dict.MergedDictionaries[0][key] as string;
                }
            }

            return resString;
        }

        public static void CopyStreamToStream(Stream source, Stream destination, int buffersize,
                                              Action<Stream, Stream, Exception> completed)
        {
            //byte[] buffer = new byte[0x2500];
            byte[] buffer = new byte[buffersize];
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(null);

            Action<Exception> done = e =>
                {
                    if (completed != null)
                        asyncOp.Post(delegate { completed(source, destination, e); }, null);
                };

            AsyncCallback[] rc = { null };
            rc[0] = readResult =>
                        {
                            try
                            {
                                int read = source.EndRead(readResult);
                                if (read > 0)
                                {
                                    destination.BeginWrite(buffer, 0, read, writeResult =>
                                                                                {
                                                                                    try
                                                                                    {
                                                                                        destination.EndWrite(writeResult);
                                                                                        source.BeginRead(
                                                                                            buffer, 0, buffer.Length,
                                                                                            rc[0], null);
                                                                                    }
                                                                                    catch (Exception exc)
                                                                                    {
                                                                                        done(exc);
                                                                                    }
                                                                                }, null);
                                }
                                else done(null);
                            }
                            catch (Exception exc) { done(exc); }
                        };

            source.BeginRead(buffer, 0, buffer.Length, rc[0], null);
        }

        public static bool IsProcessElevated()
        {
            bool fIsElevated;
            SafeTokenHandle hToken = null;
            IntPtr pTokenElevation = IntPtr.Zero;

            try
            {
                // Open the access token of the current process with TOKEN_QUERY. 
                if (!NativeMethods.OpenProcessToken(Process.GetCurrentProcess().Handle,
                    NativeMethods.TOKEN_QUERY, out hToken))
                {
                    throw new Win32Exception();
                }

                // Allocate a buffer for the elevation information. 
                int cbTokenElevation = Marshal.SizeOf(typeof(TOKEN_ELEVATION));
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
                TOKEN_ELEVATION elevation = (TOKEN_ELEVATION)Marshal.PtrToStructure(
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

        public static bool SubtitleNeedConversion(OutputType outputType, string format)
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
            }

            return false;
        }

        private static bool SubtitleConversionSupported(OutputType outputType, string format)
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

        public static void CheckSubtitles(EncodeInfo encodingJob)
        {
            // WebM Format has no support for subtitles
            if (encodingJob.EncodingProfile.OutFormat == OutputType.OutputWebM)
                encodingJob.SubtitleStreams.Clear();

            foreach (SubtitleInfo info in encodingJob.SubtitleStreams)
            {
                info.NeedConversion = SubtitleNeedConversion(encodingJob.EncodingProfile.OutFormat, info.Format) ||
                                      (info.KeepOnlyForcedCaptions && !info.HardSubIntoVideo);
                info.FormatSupported = SubtitleConversionSupported(encodingJob.EncodingProfile.OutFormat, info.Format);
            }

            encodingJob.SubtitleStreams.RemoveAll(info => !info.FormatSupported);
        }

        public static void CheckStreamLimit(EncodeInfo encodingJob)
        {
            // rearrange default audio stream
            AudioInfo defaultAudioItem = encodingJob.AudioStreams.Find(info => info.MkvDefault);
            if (defaultAudioItem != null)
            {
                encodingJob.AudioStreams.Remove(defaultAudioItem);
                encodingJob.AudioStreams.Insert(0, defaultAudioItem);
            }

            // rearrange default subtitle stream
            SubtitleInfo defaultSubtitleItem = encodingJob.SubtitleStreams.Find(info => info.MkvDefault);
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
                    AudioInfo firstIndex = encodingJob.AudioStreams.First();
                    if (firstIndex != null)
                        encodingJob.AudioStreams.RemoveAll(info => info != firstIndex);
                    break;
                case OutputType.OutputDvd:
                    int audioCount = encodingJob.AudioStreams.Count;
                    int subtitleCount = encodingJob.SubtitleStreams.Count;
                    int chapterCount = encodingJob.Chapters.Count;

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