// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BDNExport.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   BDN-Xml file exporter
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Utilities.Subtitles
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Model.Subtitles;

    /// <summary>
    /// BDN-Xml file exporter
    /// </summary>
    public class BdnExport
    {
        private static readonly CultureInfo CInfo = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");

        // TODO: look into xml generation

        /// <summary>
        /// Writes BDN File
        /// </summary>
        /// <param name="subtitle">Text subtitle to write</param>
        /// <param name="fileName">Output file name</param>
        /// <param name="videoWidth">Target video width</param>
        /// <param name="videoHeight">Target video height</param>
        /// <param name="fps">Target video fps</param>
        /// <returns></returns>
        public static bool WriteBdnXmlFile(TextSubtitle subtitle, string fileName, int videoWidth, int videoHeight, float fps)
        {
            if (File.Exists(fileName)) return false;

            string partFileName = Path.GetFileNameWithoutExtension(fileName);

            var outputDocument = new XmlDocument();
            outputDocument.AppendChild(outputDocument.CreateXmlDeclaration("1.0", "UTF-8", null));

            XmlNode docNode = outputDocument.CreateElement("BDN");
            outputDocument.AppendChild(docNode);

            AppendAttribute(docNode, "Version", "0.93", outputDocument);
            AppendAttribute(docNode, "xsi", "noNamespaceSchemaLocation", "http://www.w3.org/2001/XMLSchema-instance",
                            "BD-03-006-0093b BDN File Format.xsd", outputDocument);
            

            XmlNode descNode = outputDocument.CreateElement("Description");
            docNode.AppendChild(descNode);

            XmlNode workNode = outputDocument.CreateElement("Name");
            AppendAttribute(workNode, "Title", partFileName, outputDocument);
            AppendAttribute(workNode, "Content", string.Empty, outputDocument);
            descNode.AppendChild(workNode);

            workNode = outputDocument.CreateElement("Language");
            AppendAttribute(workNode, "Code", string.Empty, outputDocument);
            descNode.AppendChild(workNode);

            workNode = outputDocument.CreateElement("Format");
            AppendAttribute(workNode, "VideoFormat", string.Format("{0:0}p", videoHeight), outputDocument);
            AppendAttribute(workNode, "FrameRate", string.Format(CInfo, "{0:0.000}", fps), outputDocument);
            AppendAttribute(workNode, "DropFrame", "False", outputDocument);
            descNode.AppendChild(workNode);

            workNode = outputDocument.CreateElement("Events");
            AppendAttribute(workNode, "Type", "Graphic", outputDocument);
            AppendAttribute(workNode, "FirstEventInTC", CreateBdnTimeStamp(subtitle.Captions.First().StartTime, fps), outputDocument);
            AppendAttribute(workNode, "LastEventOutTC", CreateBdnTimeStamp(subtitle.Captions.Last().EndTime, fps), outputDocument);
            AppendAttribute(workNode, "NumberofEvents", subtitle.Captions.Count.ToString(CInfo), outputDocument);
            descNode.AppendChild(workNode);

            XmlNode eventNode = outputDocument.CreateElement("Events");
            docNode.AppendChild(eventNode);

            int i = 0;
            foreach (SubCaption caption in subtitle.Captions)
            {
                ImageHolder image = PNGImage.CreateImage(caption, subtitle.Style, i, videoWidth, videoHeight, fileName);
                if (string.IsNullOrEmpty(image.FileName) || image.Width == 0 || image.Height == 0) continue;

                workNode = outputDocument.CreateElement("Event");
                AppendAttribute(workNode, "InTC", CreateBdnTimeStamp(caption.StartTime, fps), outputDocument);
                AppendAttribute(workNode, "OutTC", CreateBdnTimeStamp(caption.EndTime, fps), outputDocument);
                AppendAttribute(workNode, "Forced", "False", outputDocument);
                eventNode.AppendChild(workNode);

                XmlNode gNode = outputDocument.CreateElement("Graphic");
                AppendAttribute(gNode, "Width", image.Width.ToString(CInfo), outputDocument);
                AppendAttribute(gNode, "Height", image.Height.ToString(CInfo), outputDocument);
                var posX = (int)Math.Ceiling((float) videoWidth/2 - (float) image.Width/2);
                var posY = videoHeight - image.Height - 120;
                AppendAttribute(gNode, "X", posX.ToString(CInfo), outputDocument);
                AppendAttribute(gNode, "Y", posY.ToString(CInfo), outputDocument);
                gNode.InnerText = image.FileName;

                workNode.AppendChild(gNode);

                i++;
            }

            outputDocument.Save(fileName);
            return true;
        }

        /// <summary>
        /// Appends an xml attribute to a node
        /// </summary>
        /// <param name="workNode">Append attribute to this node</param>
        /// <param name="attribName">Attribute name</param>
        /// <param name="value">Attribute value</param>
        /// <param name="xmlDoc">Root XML document</param>
        private static void AppendAttribute(XmlNode workNode, string attribName, string value, XmlDocument xmlDoc)
        {
            AppendAttribute(workNode, null, attribName, null, value, xmlDoc);
        }

        /// <summary>
        /// Appends an xml attribute to a node
        /// </summary>
        /// <param name="workNode">Append attribute to this node</param>
        /// <param name="prefix">Attribute prefix</param>
        /// <param name="localName">Attribute local name</param>
        /// <param name="nameSpaceUri">Attribute namespace URI</param>
        /// <param name="value">Attribute value</param>
        /// <param name="xmlDoc">Root XML document</param>
        private static void AppendAttribute(XmlNode workNode, string prefix, string localName, string nameSpaceUri, string value, XmlDocument xmlDoc)
        {
            if (workNode.Attributes == null) return;

            XmlAttribute attribute = xmlDoc.CreateAttribute(prefix, localName, nameSpaceUri);
            attribute.Value = value;
            workNode.Attributes.Append(attribute);
        }

        /// <summary>
        /// Calculate BDN Timestamp
        /// </summary>
        /// <param name="inTime">Source timestamp</param>
        /// <param name="fps">Video fps</param>
        /// <returns>BDN Timestamp</returns>
        public static string CreateBdnTimeStamp(TimeSpan inTime, float fps)
        {
            int roundedFps = (int) Math.Ceiling(fps);
            int num, denom;
            VideoHelper.GetFPSNumDenom(fps, out num, out denom);

            double calculatedTime = inTime.TotalMilliseconds/((double)denom/1000);

            TimeSpan outTime = TimeSpan.FromMilliseconds(calculatedTime);
            float mSec = outTime.Milliseconds - (outTime.Milliseconds / 1000f * roundedFps) * 10;

            TimeSpan corrected = TimeSpan.FromMilliseconds(mSec);
            outTime = outTime.Subtract(corrected);
            
            DateTime date = new DateTime().Add(outTime);

            return date.ToString("HH:mm:ss:ff", CInfo);
        }
    }
}
