using System;
using System.Xml.Serialization;
using UpdateCore.VersionInfo;

namespace UpdateCore
{
    [XmlRoot("VideoConvertUpdateFile")]
    public class UpdateFileInfo : IDisposable
    {
        [XmlElement("Core")]
        public CoreInfo Core { get; set; }
        [XmlElement("Updater")]
        public CoreInfo Updater { get; set; }
        [XmlElement("AviSynthPlugins")]
        public ToolInfo AviSynthPlugins { get; set; }
        [XmlElement("Profiles")]
        public ToolInfo Profiles { get; set; }

        [XmlElement("x264")]
        public ToolInfo X264 { get; set; }
        [XmlElement("x264_64")]
        public ToolInfo X26464 { get; set; }
        [XmlElement("ffmpeg")]
        public ToolInfo FFMPEG { get; set; }
        [XmlElement("eac3to")]
        public ToolInfo Eac3To { get; set; }
        [XmlElement("lsdvd")]
        public ToolInfo LsDvd { get; set; }
        [XmlElement("mkvtoolnix")]
        public ToolInfo MKVToolnix { get; set; }
        [XmlElement("mplayer")]
        public ToolInfo Mplayer { get; set; }
        [XmlElement("tsmuxer")]
        public ToolInfo TSMuxeR { get; set; }
        [XmlElement("mjpegtools")]
        public ToolInfo MjpegTools { get; set; }
        [XmlElement("dvdauthor")]
        public ToolInfo DVDAuthor { get; set; }
        [XmlElement("mp4box")]
        public ToolInfo MP4Box { get; set; }
        [XmlElement("hcenc")]
        public ToolInfo HcEnc { get; set; }
        [XmlElement("oggenc")]
        public ToolInfo OggEnc { get; set; }
        [XmlElement("lame")]
        public ToolInfo Lame { get; set; }
        [XmlElement("vpxenc")]
        public ToolInfo VpxEnc { get; set; }
        [XmlElement("bdsup2sub")]
        public ToolInfo BDSup2Sub { get; set; }

        public UpdateFileInfo()
        {
            Core = new CoreInfo();
            Updater = new CoreInfo();
            AviSynthPlugins = new ToolInfo();
            Profiles = new ToolInfo();

            X264 = new ToolInfo();
            X26464 = new ToolInfo();
            FFMPEG = new ToolInfo();
            Eac3To = new ToolInfo();
            LsDvd = new ToolInfo();
            MKVToolnix = new ToolInfo();
            Mplayer = new ToolInfo();
            TSMuxeR = new ToolInfo();
            MjpegTools = new ToolInfo();
            DVDAuthor = new ToolInfo();
            MP4Box = new ToolInfo();
            HcEnc = new ToolInfo();
            OggEnc = new ToolInfo();
            Lame = new ToolInfo();
            VpxEnc = new ToolInfo();
            BDSup2Sub = new ToolInfo();
        }

        public void Dispose()
        {
        }
    }
}
