// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SSAReader.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   SSA/ASS subtitle reader
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Utilities.Subtitles
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using log4net;
    using Model.Subtitles;

    /// <summary>
    /// SSA/ASS subtitle reader
    /// </summary>
    public class SSAReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SSAReader));

        private static readonly CultureInfo CInfo = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");

        /// <summary>
        /// Read subtitle file
        /// </summary>
        /// <param name="fileName">Textfile</param>
        /// <returns>parsed <see cref="TextSubtitle"/></returns>
        public static TextSubtitle ReadFile(string fileName)
        {
            TextSubtitle result = new TextSubtitle();
            if (!File.Exists(fileName))
            {
                Log.DebugFormat("File \"{0}\" doesn't exist. Aborting file import", fileName);
                return result;
            }

            string lines;
            using (TextReader reader = File.OpenText(fileName))
            {
                lines = reader.ReadToEnd();
            }
            if (string.IsNullOrEmpty(lines)) return result;

            List<string> textLines = lines.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> textCaps = new List<string>();

            StringBuilder sb = new StringBuilder();

            foreach (string line in textLines)
            {
                if (line.Trim().StartsWith("["))
                {
                    if (sb.Length > 0)
                    {
                        textCaps.Add(sb.ToString());
                        sb.Clear();
                    }
                    sb.AppendLine(line.Trim());
                }
                else
                {
                    sb.AppendLine(line.Trim());
                }
            }

            if (sb.Length > 0)
                textCaps.Add(sb.ToString());
            sb.Clear();

            //textCaps = lines.Split(new[] {"\r\n\r\n", "\n\n"}, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (!textCaps.Any()) return result;

            string sInfo = textCaps[0];
            if (!sInfo.Any()) return result;
            sInfo = Regex.Replace(sInfo, "^;.*$|^Title.*$", "", RegexOptions.Multiline);

            bool isAdvancedScript = false;
            try
            {
                Match matchAdvanced = Regex.Match(sInfo, @"^\[Script Info\].*ScriptType: v4\.00+.*$", RegexOptions.Singleline | RegexOptions.Multiline);
                Match matchResults = Regex.Match(sInfo, @"^\[Script Info\].*ScriptType: v4\.00.*$", RegexOptions.Singleline | RegexOptions.Multiline);
                if (!matchResults.Success && !matchAdvanced.Success)
                {
                    return result;
                }
                if (matchAdvanced.Success)
                    isAdvancedScript = true;
            }
            catch (ArgumentException ex)
            {
                Log.Error(ex);
            }

            bool setFormat = false;
            
            for (int i = 1; i <= textCaps.Count - 1; i++ )
            {
                string section = textCaps[i];
                try
                {
                    Match matchStyles = Regex.Match(section, @"^\[V4.*Styles\].*$", RegexOptions.Multiline);
                    Match matchEvents = Regex.Match(section, @"^\[Events\].*$", RegexOptions.Multiline);
                    if (matchStyles.Success && !setFormat)
                    {
                        string[] styles = section.Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);
                        if (styles.Length < 3) return result;
                        List<String> formatAttribs = styles[1].Split(new[] {","}, StringSplitOptions.None).ToList();
                        List<String> formatValues = styles[2].Split(new[] {","}, StringSplitOptions.None).ToList();
                        
                        for (int index = 0; index <= formatAttribs.Count - 1; index++)
                        {
                            string formatAttrib = formatAttribs[index].Trim();
                            string formatValue = formatValues[index].Trim();

                            switch (formatAttrib)
                            {
                                case "Fontname":
                                    result.Style.FontName = formatValue;
                                    break;
                                case "Fontsize":
                                    result.Style.FontSize = int.Parse(formatValue, NumberStyles.Integer);
                                    break;
                                case "PrimaryColour":
                                    result.Style.PrimaryColor = GetCorrectColor(formatValue);
                                    break;
                                case "SecondaryColour":
                                    result.Style.SecondaryColor = GetCorrectColor(formatValue);
                                    break;
                                case "TertiaryColour":
                                case "OutlineColour":
                                    result.Style.OutlineColor = GetCorrectColor(formatValue);
                                    break;
                                case "BackColour":
                                    result.Style.BackColor = GetCorrectColor(formatValue);
                                    break;
                                case "Bold":
                                    result.Style.Bold = formatValue == "-1";
                                    break;
                                case "Italic":
                                    result.Style.Italic = formatValue == "-1";
                                    break;
                                case "Underline":
                                    result.Style.Underline = formatValue == "-1";
                                    break;
                                case "StrikeOut":
                                    result.Style.StrikeThrough = formatValue == "-1";
                                    break;
                                case "BorderStyle":
                                    result.Style.BorderStyle = int.Parse(formatValue, NumberStyles.Integer);
                                    break;
                                case "Outline":
                                    result.Style.Outline = int.Parse(formatValue, NumberStyles.Integer);
                                    break;
                                case "Shadow":
                                    result.Style.Shadow = int.Parse(formatValue, NumberStyles.Integer);
                                    break;
                                case "Alignment":
                                    result.Style.Alignment = GetAlignment(formatValue, isAdvancedScript);
                                    break;
                                case "MarginL":
                                    result.Style.MarginL = int.Parse(formatValue, NumberStyles.Integer);
                                    break;
                                case "MarginR":
                                    result.Style.MarginR = int.Parse(formatValue, NumberStyles.Integer);
                                    break;
                                case "MarginV":
                                    result.Style.MarginV = int.Parse(formatValue, NumberStyles.Integer);
                                    break;
                                case "Alphalevel":
                                    result.Style.AlphaLevel = int.Parse(formatValue, NumberStyles.Integer);
                                    break;
                                case "Encoding":
                                    result.Style.Encoding = formatValue;
                                    break;
                            }
                        }
                        setFormat = true;
                    }
                    else if (matchEvents.Success)
                    {
                        string[] events = section.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        if (events.Length < 2) return result;
                        List<String> eventFormat = events[1].Split(new[] { "," }, StringSplitOptions.None).ToList();

                        int startTimeRow = -1, endTimeRow = -1, textRow = -1;

                        for (int j = 0; j <= eventFormat.Count - 1; j++)
                        {
                            string label = eventFormat[j].Trim();
                            if (label.Equals("Start"))
                                startTimeRow = j;
                            else if (label.Equals("End"))
                                endTimeRow = j;
                            else if (label.Equals("Text"))
                                textRow = j;
                        }

                        if (startTimeRow < 0 || endTimeRow < 0 || textRow < 0) return result;

                        for (int j = 2; j <= events.Length - 1; j++)
                        {
                            string eventLine = events[j];
                            string[] eventRows = eventLine.Split(new[] {","}, StringSplitOptions.None);
                            if (eventRows.Length < textRow + 1) continue;

                            TimeSpan startTime =
                                DateTime.ParseExact(eventRows[startTimeRow].Trim(), "h:mm:ss.ff", CInfo).
                                    TimeOfDay;
                            TimeSpan endTime =
                                DateTime.ParseExact(eventRows[endTimeRow].Trim(), "h:mm:ss.ff", CInfo).
                                    TimeOfDay;
                            string text = string.Join(", ", eventRows, textRow, eventRows.Length - textRow);

                            // line break
                            text = Regex.Replace(text, @"(?:\\N|\\n)", Environment.NewLine, RegexOptions.Multiline);
                            // remove line break strategy
                            text = Regex.Replace(text, @"\\q\d*?", string.Empty, RegexOptions.Multiline);
                            // bold text
                            text = Regex.Replace(text, @"\{\\b1\}(.*?)\{\\b0*?\}", "<b>$1</b>", RegexOptions.Singleline | RegexOptions.Multiline);
                            // italic text
                            text = Regex.Replace(text, @"\{\\i1\}(.*?)\{\\i0*?\}", "<i>$1</i>", RegexOptions.Singleline | RegexOptions.Multiline);
                            // underlined text
                            text = Regex.Replace(text, @"\{\\u1\}(.*?)\{\\u0*?\}", "<u>$1</u>", RegexOptions.Singleline | RegexOptions.Multiline);
                            // strike-through text
                            text = Regex.Replace(text, @"\{\\s1\}(.*?)\{\\s0*?\}", "<s>$1</s>", RegexOptions.Singleline | RegexOptions.Multiline);

                            // remove border and shadow override
                            text = Regex.Replace(text, @"\{\\(?:bord|shad)\d*?\}", string.Empty, RegexOptions.Multiline);
                            // remove blurry text border
                            text = Regex.Replace(text, @"\{\\be(?:1|0)\}", string.Empty, RegexOptions.Multiline);
                            // remove fontname
                            text = Regex.Replace(text, @"\{\\fn.*\}", string.Empty, RegexOptions.Multiline);
                            // remove fontsize
                            text = Regex.Replace(text, @"\{\\fs.*\}", string.Empty, RegexOptions.Multiline);
                            // remove color definition
                            text = Regex.Replace(text, @"\{\\\d?c&H.*&\}", string.Empty, RegexOptions.Multiline);
                            // remove alpha definition
                            text = Regex.Replace(text, @"\{\\\d?(?:a|alpha)&H.*&\}", string.Empty, RegexOptions.Multiline);
                            // remove x/y text scaling
                            text = Regex.Replace(text, @"\{\\(?:fscy|fscx)\d+\}", string.Empty, RegexOptions.Multiline);
                            // remove text spacing
                            text = Regex.Replace(text, @"\{\\fsp\d+\}", string.Empty, RegexOptions.Multiline);
                            // remove charset definition
                            text = Regex.Replace(text, @"\{\\fe.*?\}", string.Empty, RegexOptions.Multiline);
                            // parse and remove text alignment
                            Regex align = new Regex(@"\{\\an*?(\d*?)\}", RegexOptions.Multiline);
                            int alignment = GetAlignment(align.Match(text).Value, isAdvancedScript);
                            text = align.Replace(text, string.Empty);
                            // remove x/y/z text rotation
                            text = Regex.Replace(text, @"\{\\fr(?:x|y|z)??(-??\d*?)\}", string.Empty, RegexOptions.Multiline);
                            // remove karaoke formatting
                            text = Regex.Replace(text, @"\{\\(?:k|ko|kf|K)(\d*?)\}", string.Empty, RegexOptions.Multiline);
                            // remove format reset
                            text = Regex.Replace(text, @"\{\\r.*\}", string.Empty, RegexOptions.Multiline);
                            // remove text animation
                            text = Regex.Replace(text, @"\{\\(?:move|pos|t|org|fad|fade|clip)\(.*?\)\}", string.Empty, RegexOptions.Multiline);

                            // remove anything that was not catched above
                            text = Regex.Replace(text, @"\{(?:\\(?:fscy|fscx)\d+|\\fn.*|\\fs.*|\\\d?c&H.*&|\\\d?(?:a|alpha)&H.*&|\\(?:fscy|fscx)\d+|" + 
                                                       @"\\fsp\d+|\\fe.*?|\\an*?(\d*?)|\\fr(?:x|y|z)??(-??\d*?)|\\(?:k|ko|kf|K)(\d*?)|\\r.*|" +
                                                       @"\\(?:move|pos|t|org|fad|fade|clip)\(.*?\)\\N|\\n|\\q\d*?|\\(?:b|i|u|s|be)(?:1|0)*?|\\(?:bord|shad)\d*?)*?\}",
                                                       string.Empty, RegexOptions.Multiline);

                            SubCaption caption = new SubCaption
                                                {
                                                    StartTime = startTime,
                                                    EndTime = endTime,
                                                    Text = text,
                                                    Alignment = alignment
                                                };
                            result.Captions.Add(caption);

                        }
                    }
                }
                catch (ArgumentException ex)
                {
                    Log.Error(ex);
                }

            }

            if (!setFormat)
            {
                result.SetDefaultStyle();
            }

            return result;
        }

        /// <summary>
        /// Read color from input
        /// </summary>
        /// <param name="input">source color</param>
        /// <returns>parsed <see cref="Color"/></returns>
        public static Color GetCorrectColor(string input)
        {
            ColorConverter converter = new ColorConverter();
            object fromString = converter.ConvertFromString(input);
            if (fromString == null) return Color.Black;

            Color converted = (Color) fromString;
            if (converted.A == 0)
            {
                converted = Color.FromArgb(255, converted);
            }
            return Color.FromArgb(converted.A, converted.B, converted.G, converted.R);
        }

        /// <summary>
        /// Calculate Text alignment
        /// </summary>
        /// <param name="formatValue"></param>
        /// <param name="isAdvancedScript"></param>
        /// <returns>parsed alignment</returns>
        public static int GetAlignment(string formatValue, bool isAdvancedScript)
        {
            int result = 2;
            if (string.IsNullOrEmpty(formatValue)) return result;

            int tempAlignment = int.Parse(formatValue, NumberStyles.Integer);
            if (isAdvancedScript)
                result = tempAlignment;
            else
            {
                switch (tempAlignment)
                {
                    case 1:
                    case 2:
                    case 3:
                        result = tempAlignment;
                        break;
                    case 5:
                    case 6:
                    case 7:
                        result = tempAlignment + 2;
                        break;
                    case 9:
                    case 10:
                    case 11:
                        result = tempAlignment - 5;
                        break;
                }
            }
            return result;
        }
    }
}