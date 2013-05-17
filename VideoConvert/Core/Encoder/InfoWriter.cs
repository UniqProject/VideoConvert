using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Profiles;
using log4net;

namespace VideoConvert.Core.Encoder
{
    public class InfoWriter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(InfoWriter));

        private EncodeInfo _jobInfo;
        private BackgroundWorker _bw;

        public void SetJob(EncodeInfo job)
        {
            _jobInfo = job;
        }

        public void DoWrite(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string imagesStatus = Processing.GetResourceString("infowriter_images_status");
            string infoStatus = Processing.GetResourceString("infowriter_info_status");

            _bw.ReportProgress(-10, imagesStatus);
            _bw.ReportProgress(0, imagesStatus);

            string baseImageName;

            if (_jobInfo.EncodingProfile.OutFormat != OutputType.OutputAvchd &&
                _jobInfo.EncodingProfile.OutFormat != OutputType.OutputBluRay &&
                _jobInfo.EncodingProfile.OutFormat != OutputType.OutputDvd)
            {
                baseImageName = Path.GetFileNameWithoutExtension(_jobInfo.OutputFile);
                if (baseImageName != null) 
                    baseImageName = baseImageName.TrimEnd(new[] {'.'});
            }
            else
                baseImageName = Path.GetFileName(_jobInfo.OutputFile);

            string baseImagePath = Path.GetDirectoryName(_jobInfo.OutputFile);
            if (string.IsNullOrEmpty(baseImagePath)) baseImagePath = string.Empty;

            Uri backdropUri = new Uri(_jobInfo.BackDropImage);
            Uri posterUri = new Uri(_jobInfo.PosterImage);

            string backdropExt = Path.GetExtension(backdropUri.LocalPath);
            string posterExt = Path.GetExtension(posterUri.LocalPath);

            string backdropFile = Path.Combine(baseImagePath, baseImageName + "-fanart" + backdropExt);
            string posterFile = Path.Combine(baseImagePath, baseImageName + "-poster" + posterExt);
            string thumbFile = Path.Combine(baseImagePath, baseImageName + "-thumb" + posterExt);
            string infoFile = Path.Combine(baseImagePath, baseImageName + ".nfo");

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(backdropUri, backdropFile);
                _bw.ReportProgress(25, imagesStatus);

                client.DownloadFile(posterUri, posterFile);
                _bw.ReportProgress(50, imagesStatus);

                client.DownloadFile(posterUri, thumbFile);
                _bw.ReportProgress(75, imagesStatus);
            }

            _bw.ReportProgress(-10, infoStatus);
            _bw.ReportProgress(75, infoStatus);

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            XmlSerializer serializer = new XmlSerializer(typeof(MovieEntry));
            using (XmlWriter writer = XmlWriter.Create(infoFile, new XmlWriterSettings{Encoding = Encoding.UTF8, Indent = true}))
            {
                
                serializer.Serialize(writer, _jobInfo.MovieInfo,ns);
            }

            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;

            e.Result = _jobInfo;

        }
    }
}