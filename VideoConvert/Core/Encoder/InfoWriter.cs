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
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using VideoConvert.Core.Helpers;
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

            bool isMovie = _jobInfo.MovieInfo != null;
            bool isEpisode = _jobInfo.EpisodeInfo != null;

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

            Uri backdropUri = null;
            Uri posterUri;
            string posterExt;
            string backdropFile = string.Empty;
            string posterFile = string.Empty;

            if (isMovie)
            {
                backdropUri = new Uri(_jobInfo.MovieInfo.SelectedBackdropImage);
                posterUri = new Uri(_jobInfo.MovieInfo.SelectedPosterImage);
                string backdropExt = Path.GetExtension(backdropUri.LocalPath);
                posterExt = Path.GetExtension(posterUri.LocalPath);
                backdropFile = Path.Combine(baseImagePath, baseImageName + "-fanart" + backdropExt);
                posterFile = Path.Combine(baseImagePath, baseImageName + "-poster" + posterExt);
            }
            else if (isEpisode)
            {
                posterUri = new Uri(_jobInfo.EpisodeInfo.SelectedPosterImage);
                posterExt = Path.GetExtension(posterUri.LocalPath);
            }
            else
                return;

            string thumbFile = Path.Combine(baseImagePath, baseImageName + "-thumb" + posterExt);
            string infoFile = Path.Combine(baseImagePath, baseImageName + ".nfo");

            using (WebClient client = new WebClient())
            {
                if (isMovie)
                {
                    client.DownloadFile(backdropUri, backdropFile);
                    _bw.ReportProgress(25, imagesStatus);

                    client.DownloadFile(posterUri, posterFile);
                    _bw.ReportProgress(50, imagesStatus);

                    client.DownloadFile(posterUri, thumbFile);
                    _bw.ReportProgress(75, imagesStatus);
                }
                else
                {
                    client.DownloadFile(posterUri, thumbFile);
                    _bw.ReportProgress(50, imagesStatus);
                }
            }

            _bw.ReportProgress(-10, infoStatus);
            _bw.ReportProgress(isMovie ? 75 : 50, infoStatus);

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            XmlSerializer serializer = isMovie ? new XmlSerializer(typeof (MovieEntry)) : new XmlSerializer(typeof (EpisodeEntry));
            using (XmlWriter writer = XmlWriter.Create(infoFile, new XmlWriterSettings{Encoding = Encoding.UTF8, Indent = true}))
            {
                if (isMovie)
                    serializer.Serialize(writer, _jobInfo.MovieInfo, ns);
                else
                    serializer.Serialize(writer, _jobInfo.EpisodeInfo, ns);
            }

            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;

            e.Result = _jobInfo;

        }
    }
}