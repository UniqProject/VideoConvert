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
        /// <summary>
        /// Generates commandline for the eac3to executable
        /// </summary>
        /// <param name="jobInfo">Job entry to process</param>
        /// <returns>commandline arguments</returns>
        public static string GenerateDemuxLine(ref EncodeInfo jobInfo)
        {
            StringBuilder sb = new StringBuilder();

            string inputFile;
            int startstream = 0;
            string ext;
            string formattedExt;

            // generate output filename depending input file given
            if (jobInfo.Input == InputType.InputDvd)
            {
                inputFile = jobInfo.DumpOutput;
                jobInfo.VideoStream.TempFile = Path.ChangeExtension(jobInfo.DumpOutput, "demuxed.mkv");
            }
            else
            {
                inputFile = string.IsNullOrEmpty(jobInfo.TempInput) ? jobInfo.InputFile : jobInfo.TempInput;

                jobInfo.VideoStream.TempFile = string.IsNullOrEmpty(jobInfo.TempInput)
                                                   ? Processing.CreateTempFile(
                                                       string.IsNullOrEmpty(jobInfo.TempOutput)
                                                           ? jobInfo.BaseName
                                                           : jobInfo.TempOutput, "demuxed.mkv")
                                                   : Processing.CreateTempFile(jobInfo.TempInput, "demuxed.mkv");
            }

            sb.AppendFormat("\"{0}\" {1:g}:\"{2}\" ", inputFile, jobInfo.VideoStream.StreamId + startstream,
                            jobInfo.VideoStream.TempFile);

            // on stereo sources, decide if stream for right eye should be extracted
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

            // if input source is dvd, increment stream id to match eac2to stream counting
            if (jobInfo.Input == InputType.InputDvd)
                startstream++;

            // process all audio streams
            for (int i = 0; i < jobInfo.AudioStreams.Count; i++)
            {
                AudioInfo item = jobInfo.AudioStreams[i];

                // get file extension for selected stream based on format and format profile
                ext = StreamFormat.GetFormatExtension(item.Format, item.FormatProfile, false);
                string core = string.Empty;

                // extract only core audio data for dvd output
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

                switch (jobInfo.Input)
                {
                    case InputType.InputDvd:
                        item.TempFile = Processing.CreateTempFile(jobInfo.DumpOutput, formattedExt);
                        break;
                    default:
                        item.TempFile = string.IsNullOrEmpty(jobInfo.TempInput)
                                            ? Processing.CreateTempFile(
                                                string.IsNullOrEmpty(jobInfo.TempOutput)
                                                    ? jobInfo.BaseName
                                                    : jobInfo.TempOutput, formattedExt)
                                            : Processing.CreateTempFile(jobInfo.TempInput, formattedExt);
                        break;
                }

                sb.AppendFormat("{0:g}:\"{1}\" {2} ", item.Id + startstream, item.TempFile, core);
                jobInfo.AudioStreams[i] = item;
            }

            // process all subtitle streams
            for (int i = 0; i < jobInfo.SubtitleStreams.Count; i++)
            {
                SubtitleInfo item = jobInfo.SubtitleStreams[i];

                ext = StreamFormat.GetFormatExtension(item.Format, String.Empty, false);
                formattedExt = string.Format("demuxed.{0:g}.{1}.{2}", item.StreamId, item.LangCode, ext);

                switch (jobInfo.Input)
                {
                    case InputType.InputDvd:
                        item.TempFile = Processing.CreateTempFile(jobInfo.DumpOutput, formattedExt);
                        break;
                    default:
                        item.TempFile = string.IsNullOrEmpty(jobInfo.TempInput)
                                            ? Processing.CreateTempFile(
                                                string.IsNullOrEmpty(jobInfo.TempOutput)
                                                    ? jobInfo.BaseName
                                                    : jobInfo.TempOutput, formattedExt)
                                            : Processing.CreateTempFile(jobInfo.TempInput, formattedExt);
                        break;
                }

                sb.AppendFormat("{0:g}:\"{1}\" ", item.Id + startstream, item.TempFile);
                jobInfo.SubtitleStreams[i] = item;
            }

            // add logfile to tempfiles list for deletion
            jobInfo.TempFiles.Add(
                jobInfo.VideoStream.TempFile.Substring(0, jobInfo.VideoStream.TempFile.LastIndexOf('.')) + " - Log.txt");

            if (jobInfo.Input == InputType.InputDvd)
                jobInfo.TempFiles.Add(jobInfo.DumpOutput);

            sb.Append("-progressNumbers -no2ndpass ");

            return sb.ToString();
        }
    }
}