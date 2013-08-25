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
using System.Linq;
using System.Xml;
using log4net;

namespace VideoConvert.Core.Subtitles
{
    public class BDNExport
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BDNExport));

        public static bool WriteBDNXmlFile(TextSubtitle subtitle, string fileName, int videoWidth, int videoHeight, float fps)
        {
            if (File.Exists(fileName)) return false;

            string partFileName = Path.GetFileNameWithoutExtension(fileName);

            XmlDocument outputDocument = new XmlDocument();
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
            AppendAttribute(workNode, "FrameRate", string.Format(AppSettings.CInfo, "{0:0.000}", fps), outputDocument);
            AppendAttribute(workNode, "DropFrame", "False", outputDocument);
            descNode.AppendChild(workNode);

            workNode = outputDocument.CreateElement("Events");
            AppendAttribute(workNode, "Type", "Graphic", outputDocument);
            AppendAttribute(workNode, "FirstEventInTC", CreateBDNTimeStamp(subtitle.Captions.First().StartTime, fps), outputDocument);
            AppendAttribute(workNode, "LastEventOutTC", CreateBDNTimeStamp(subtitle.Captions.Last().EndTime, fps), outputDocument);
            AppendAttribute(workNode, "NumberofEvents", subtitle.Captions.Count.ToString(AppSettings.CInfo), outputDocument);
            descNode.AppendChild(workNode);

            XmlNode eventNode = outputDocument.CreateElement("Events");
            docNode.AppendChild(eventNode);

            int i = 0;
            foreach (SubCaption caption in subtitle.Captions)
            {
                ImageHolder image = PNGImage.CreateImage(caption, subtitle.Style, i, videoWidth, videoHeight, fileName);
                if (string.IsNullOrEmpty(image.FileName) || image.Width == 0 || image.Height == 0) continue;

                workNode = outputDocument.CreateElement("Event");
                AppendAttribute(workNode, "InTC", CreateBDNTimeStamp(caption.StartTime, fps), outputDocument);
                AppendAttribute(workNode, "OutTC", CreateBDNTimeStamp(caption.EndTime, fps), outputDocument);
                AppendAttribute(workNode, "Forced", "False", outputDocument);
                eventNode.AppendChild(workNode);

                XmlNode gNode = outputDocument.CreateElement("Graphic");
                AppendAttribute(gNode, "Width", image.Width.ToString(AppSettings.CInfo), outputDocument);
                AppendAttribute(gNode, "Height", image.Height.ToString(AppSettings.CInfo), outputDocument);
                int posX = (int)Math.Ceiling((float) videoWidth/2 - (float) image.Width/2);
                int posY = videoHeight - image.Height - 120;
                AppendAttribute(gNode, "X", posX.ToString(AppSettings.CInfo), outputDocument);
                AppendAttribute(gNode, "Y", posY.ToString(AppSettings.CInfo), outputDocument);
                gNode.InnerText = image.FileName;

                workNode.AppendChild(gNode);

                i++;
            }

            outputDocument.Save(fileName);
            return true;
        }

        private static void AppendAttribute(XmlNode workNode, string attribName, string value, XmlDocument xmlDoc)
        {
            AppendAttribute(workNode, null, attribName, null, value, xmlDoc);
        }

        private static void AppendAttribute(XmlNode workNode, string prefix, string localName, string nameSpaceUri, string value, XmlDocument xmlDoc)
        {
            if (workNode.Attributes == null) return;

            XmlAttribute attribute = xmlDoc.CreateAttribute(prefix, localName, nameSpaceUri);
            attribute.Value = value;
            workNode.Attributes.Append(attribute);
        }

        public static string CreateBDNTimeStamp(TimeSpan inTime, float fps)
        {
            int roundedFps = (int) Math.Ceiling(fps);
            int num, denom;
            Processing.GetFPSNumDenom(fps, out num, out denom);

            double calculatedTime = inTime.TotalMilliseconds/((double)denom/1000);

            TimeSpan outTime = TimeSpan.FromMilliseconds(calculatedTime);
            float mSec = outTime.Milliseconds - (outTime.Milliseconds / 1000f * roundedFps) * 10;

            TimeSpan corrected = TimeSpan.FromMilliseconds(mSec);
            outTime = outTime.Subtract(corrected);
            
            DateTime date = new DateTime().Add(outTime);

            return date.ToString("HH:mm:ss:ff", AppSettings.CInfo);
        }
    }
}
