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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using log4net;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Subtitles;

namespace VideoConvert.Core.Encoder
{
    public class TextSubtitleConverter
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(TextSubtitleConverter));

        private EncodeInfo _jobInfo;
        private BackgroundWorker _bw;

        /// <summary>
        /// Sets job for processing
        /// </summary>
        /// <param name="job">Job to process</param>
        public void SetJob(EncodeInfo job)
        {
            _jobInfo = job;
        }

        private readonly string _readingstatus = Processing.GetResourceString("subtitleconverter_read");
        private readonly string _writingstatus = Processing.GetResourceString("subtitleconverter_write");
        private readonly string _status = Processing.GetResourceString("bdsup2sub_convert_subtitles_status");

        /// <summary>
        /// Main processing function, called by BackgroundWorker thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DoProcess(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            _bw.ReportProgress(-10, _status);
            _bw.ReportProgress(0, _status);

            SubtitleInfo sub = _jobInfo.SubtitleStreams[_jobInfo.StreamId];
            string inFile = sub.TempFile;
            string outFile = Path.ChangeExtension(inFile, "converted.srt");

            _bw.ReportProgress(0, _readingstatus);
            TextSubtitle textSub = null;
            switch (sub.Format)
            {
                case "SSA":
                case "ASS":
                    textSub = SSAReader.ReadFile(inFile);
                    break;
                case "UTF-8":
                    textSub = SRTReader.ReadFile(inFile);
                    break;
            }

            if (textSub == null) return;

            _bw.ReportProgress(50, _writingstatus);

            if (SRTWriter.WriteFile(outFile, textSub))
            {
                sub.Format = "UTF-8";
                sub.NeedConversion = false;
                _jobInfo.TempFiles.Add(inFile);
                sub.TempFile = outFile;
                _jobInfo.ExitCode = 0;
            }

            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;
            e.Result = _jobInfo;
        }
    }
}