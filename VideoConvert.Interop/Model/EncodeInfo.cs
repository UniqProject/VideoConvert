// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncodeInfo.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Encode job properties class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VideoConvert.Interop.Model.MediaInfo;
    using VideoConvert.Interop.Model.Profiles;
    using VideoConvert.Interop.Utilities;

    /// <summary>
    /// Encode job properties class
    /// </summary>
    public class EncodeInfo : IFormattable
    {
        /// <summary>
        /// Job Name
        /// </summary>
        public string                   JobName { get; set; }

        /// <summary>
        /// File base name
        /// </summary>
        public string                   BaseName { get; set; }

        /// <summary>
        /// Source file
        /// </summary>
        public string                   InputFile { get; set; }

        /// <summary>
        /// Input type
        /// </summary>
        public InputType                Input { get; set; }

        /// <summary>
        /// Formatted input type for displaying only
        /// </summary>
        public string                   FormattedInput => GenHelper.StringValueOf(Input);

        /// <summary>
        /// Output file name
        /// </summary>
        public string                   OutputFile { get; set; }

        /// <summary>
        /// List of audio streams
        /// </summary>
        public List<AudioInfo>          AudioStreams { get; set; }

        /// <summary>
        /// List of subtitle streams
        /// </summary>
        public List<SubtitleInfo>       SubtitleStreams { get; set; }

        /// <summary>
        /// Chapters list
        /// </summary>
        public List<TimeSpan>           Chapters { get; set; }

        /// <summary>
        /// Next encoding step
        /// </summary>
        public EncodingStep             NextStep { get; set; }

        /// <summary>
        /// Completed encoding step
        /// </summary>
        public EncodingStep             CompletedStep { get; set; }

        /// <summary>
        /// Media properties
        /// </summary>
        public MediaInfoContainer       MediaInfo { get; set; }

        /// <summary>
        /// Video stream
        /// </summary>
        public VideoInfo                VideoStream { get; set; }

        /// <summary>
        /// Right-eye video stream
        /// </summary>
        public StereoVideoInfo          StereoVideoStream { get; set; }

        /// <summary>
        /// Encoding profile
        /// </summary>
        public QuickSelectProfile       EncodingProfile { get; set; }

        /// <summary>
        /// Video encoding profile
        /// </summary>
        public EncoderProfile           VideoProfile { get; set; }

        /// <summary>
        /// Audio encoding profile
        /// </summary>
        public EncoderProfile           AudioProfile { get; set; }

        /// <summary>
        /// Avisynth script file name
        /// </summary>
        public string                   AviSynthScript { get; set; }

        /// <summary>
        /// FFindex file name
        /// </summary>
        public string                   FfIndexFile { get; set; }

        /// <summary>
        /// Avisynth config file for stereo encoding
        /// </summary>
        public string                   AviSynthStereoConfig   { get; set; }

        /// <summary>
        /// Stream ID
        /// </summary>
        public int                      StreamId { get; set; }

        /// <summary>
        /// Track ID
        /// </summary>
        public int                      TrackId { get; set; }

        /// <summary>
        /// Temp file for input, in case of non-ansi file name
        /// </summary>
        public string                   TempInput { get; set; }

        /// <summary>
        /// Temp file for output, in case of non-ansi file name
        /// </summary>
        public string                   TempOutput { get; set; }

        /// <summary>
        /// File for dumping a dvd title
        /// </summary>
        public string                   DumpOutput { get; set; }

        /// <summary>
        /// Selected DVD chapters
        /// </summary>
        public string                   SelectedDvdChapters { get; set; }

        /// <summary>
        /// List of temp files
        /// </summary>
        public List<string>             TempFiles { get; set; }

        /// <summary>
        /// Encoder Exit code
        /// </summary>
        public int                      ExitCode { get; set; }

        /// <summary>
        /// Movie info
        /// </summary>
        public MovieEntry               MovieInfo { get; set; }

        /// <summary>
        /// TV-Show episode info
        /// </summary>
        public EpisodeEntry             EpisodeInfo { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public EncodeInfo()
        {
            JobName = string.Empty;
            BaseName = string.Empty;
            InputFile = string.Empty;
            Input = InputType.InputUndefined;
            OutputFile = string.Empty;
            AudioStreams = new List<AudioInfo>();
            SubtitleStreams = new List<SubtitleInfo>();
            Chapters = new List<TimeSpan>();
            NextStep = EncodingStep.NotSet;
            CompletedStep = EncodingStep.NotSet;
            MediaInfo = null;
            VideoStream = new VideoInfo();
            StereoVideoStream = new StereoVideoInfo();
            StreamId = int.MinValue;
            TrackId = int.MinValue;
            TempInput = string.Empty;
            TempOutput = string.Empty;
            DumpOutput = string.Empty;
            SelectedDvdChapters = string.Empty;
            TempFiles = new List<string>();
            ExitCode = int.MinValue;

            FfIndexFile = string.Empty;
            AviSynthScript = string.Empty;
            AviSynthStereoConfig = string.Empty;
        }

        /// <summary>
        /// Gibt einen <see cref="T:System.String"/> zurück, der das aktuelle <see cref="T:System.Object"/> darstellt.
        /// </summary>
        /// <returns>
        /// Ein <see cref="T:System.String"/>, der das aktuelle <see cref="T:System.Object"/> darstellt.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary>
        /// Formatiert den Wert der aktuellen Instanz unter Verwendung des angegebenen Formats.
        /// </summary>
        /// <returns>
        /// Der Wert der aktuellen Instanz im angegebenen Format.
        /// </returns>
        /// <param name="format">Das zu verwendende Format.– oder – Ein NULL-Verweis (Nothing in Visual Basic),
        ///  wenn das für den Typ der <see cref="T:System.IFormattable"/> -Implementierung definierte Standardformat verwendet werden soll. </param>
        /// <param name="formatProvider">Der zum Formatieren des Werts zu verwendende Anbieter.– oder – Ein NULL-Verweis (Nothing in Visual Basic),
        ///  wenn die Informationen über numerische Formate dem aktuellen Gebietsschema des Betriebssystems entnommen werden sollen. </param>
        /// <filterpriority>2</filterpriority>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            var result = string.Empty;

            result += $"JobName:            {JobName} {Environment.NewLine}";
            result += $"BaseName:           {BaseName} {Environment.NewLine}";
            result += $"InputFile:          {InputFile} {Environment.NewLine}";
            result += $"InputType:          {Input} {Environment.NewLine}";
            result += $"OutputFile:         {OutputFile} {Environment.NewLine}";
            result += Environment.NewLine;

            result += $"AudioStreams:       {Environment.NewLine}";
            result = AudioStreams.Aggregate(result, (current, item) => current + $"{item} {Environment.NewLine}");
            result += Environment.NewLine;

            result += $"SubtitleStreams:    {Environment.NewLine}";
            result = SubtitleStreams.Aggregate(result, (current, item) => current + $"{item} {Environment.NewLine}");
            result += Environment.NewLine;

            var list = new List<string>();
            foreach (var item in Chapters)
            {
                var dt = DateTime.MinValue.Add(item);
                list.Add(dt.ToString("H:mm:ss.fff"));
            }
            result += $"Chapters:           {string.Join(",", list.ToArray())} {Environment.NewLine}";

            result += $"NextStep:           {NextStep} {Environment.NewLine}";
            result += $"CompletedStep:      {CompletedStep} {Environment.NewLine}";
            result += Environment.NewLine;

            result += $"VideoStream:        {Environment.NewLine}";
            result += $"{VideoStream} {Environment.NewLine}";
            result += Environment.NewLine;

            result += $"StreamID:           {StreamId:0} {Environment.NewLine}";
            result += $"TrackID:            {TrackId:0} {Environment.NewLine}";

            result += $"TempInput:          {TempInput} {Environment.NewLine}";
            result += $"TempOutput:         {TempOutput} {Environment.NewLine}";
            result += $"DumpOutput:         {DumpOutput} {Environment.NewLine}";
            result += $"SelectedDVDChapters:{SelectedDvdChapters} {Environment.NewLine}";
            result += $"TempFiles:          {string.Join(",", TempFiles.ToArray())} {Environment.NewLine}";
            result += $"ReturnValue:        {ExitCode:0} {Environment.NewLine}";

            return result;
        }
    }
}