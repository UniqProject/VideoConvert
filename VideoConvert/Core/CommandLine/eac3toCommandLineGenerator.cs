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
using System.IO;
using System.Text;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Profiles;

namespace VideoConvert.Core.CommandLine
{
    class Eac3ToCommandLineGenerator
    {
        public static string GenerateDemuxLine(ref EncodeInfo jobInfo)
        {
            StringBuilder sb = new StringBuilder();

            string inputFile;
            int startstream = 0;
            string ext;
            string formattedExt;

            if (jobInfo.Input == InputType.InputDvd)
            {
                inputFile = jobInfo.DumpOutput;
                jobInfo.VideoStream.TempFile = Path.ChangeExtension(jobInfo.DumpOutput, "demuxed.mkv");
            }
            else
            {
                inputFile = string.IsNullOrEmpty(jobInfo.TempInput) ? jobInfo.InputFile : jobInfo.TempInput;
                jobInfo.VideoStream.TempFile = string.IsNullOrEmpty(jobInfo.TempInput)
                                                   ? Processing.CreateTempFile(jobInfo.JobName, "demuxed.mkv")
                                                   : Processing.CreateTempFile(jobInfo.TempInput, "demuxed.mkv");
            }

            sb.AppendFormat("\"{0}\" {1:g}:\"{2}\" ", inputFile, jobInfo.VideoStream.StreamId + startstream,
                            jobInfo.VideoStream.TempFile);

            if (jobInfo.StereoVideoStream.RightStreamId > 0 && jobInfo.EncodingProfile.StereoType != StereoEncoding.None)
            {
                jobInfo.StereoVideoStream.RightTempFile = Processing.CreateTempFile(jobInfo.VideoStream.TempFile,
                                                                                    "right.h264");
                jobInfo.StereoVideoStream.LeftTempFile = Processing.CreateTempFile(jobInfo.VideoStream.TempFile,
                                                                                   "left.h264");
                sb.AppendFormat("{0:g}:\"{1}\" {2:g}:\"{3}\" ", jobInfo.StereoVideoStream.LeftStreamId,
                                jobInfo.StereoVideoStream.LeftTempFile, jobInfo.StereoVideoStream.RightStreamId,
                                jobInfo.StereoVideoStream.RightTempFile);
            }

            if (jobInfo.Input == InputType.InputDvd)
                startstream++;

            for (int i = 0; i < jobInfo.AudioStreams.Count; i++)
            {
                AudioInfo item = jobInfo.AudioStreams[i];

                ext = StreamFormat.GetFormatExtension(item.Format, item.FormatProfile, false);
                string core = string.Empty;

                if (jobInfo.EncodingProfile.OutFormat == OutputType.OutputDvd && jobInfo.AudioProfile.Type == ProfileType.Copy)
                {
                    if (string.CompareOrdinal(ext, "dtshd") == 0)
                    {
                        core = "-core";
                        ext = "dts";
                    }
                    else if (string.CompareOrdinal(ext, "truehd") == 0)
                    {
                        core = "-core";
                        ext = "ac3";
                    }
                }
                formattedExt = string.Format("demuxed.{0:g}.{1}.{2}", item.StreamId, item.LangCode, ext);

                item.TempFile = jobInfo.Input == InputType.InputDvd
                                    ? Processing.CreateTempFile(jobInfo.DumpOutput, formattedExt)
                                    : Processing.CreateTempFile(
                                        string.IsNullOrEmpty(jobInfo.TempInput) ? jobInfo.JobName : jobInfo.TempInput,
                                        formattedExt);

                sb.AppendFormat("{0:g}:\"{1}\" {2} ", item.Id + startstream, item.TempFile, core);
                jobInfo.AudioStreams[i] = item;
            }

            for (int i = 0; i < jobInfo.SubtitleStreams.Count; i++)
            {
                SubtitleInfo item = jobInfo.SubtitleStreams[i];

                ext = StreamFormat.GetFormatExtension(item.Format, String.Empty, false);
                formattedExt = string.Format("demuxed.{0:g}.{1}.{2}", item.StreamId, item.LangCode, ext);

                item.TempFile = jobInfo.Input == InputType.InputDvd
                                    ? Processing.CreateTempFile(jobInfo.DumpOutput, formattedExt)
                                    : Processing.CreateTempFile(
                                        string.IsNullOrEmpty(jobInfo.TempInput)
                                            ? jobInfo.JobName
                                            : jobInfo.TempInput,
                                        formattedExt);

                sb.AppendFormat("{0:g}:\"{1}\" ", item.Id + startstream, item.TempFile);
                jobInfo.SubtitleStreams[i] = item;
            }

            jobInfo.TempFiles.Add(
                jobInfo.VideoStream.TempFile.Substring(0, jobInfo.VideoStream.TempFile.LastIndexOf('.')) + " - Log.txt");

            if (jobInfo.Input == InputType.InputDvd)
                jobInfo.TempFiles.Add(jobInfo.DumpOutput);

            sb.Append("-progressNumbers -no2ndpass ");

            return sb.ToString();
        }
    }
}