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
using System.Globalization;
using System.Xml;

namespace VideoConvert.Core.Helpers
{
    class XMLValueParser
    {
        internal static Int32 ParseInt32(XmlNode value)
        {
            Int32 outValue = 0;

            if (value != null)
                Int32.TryParse(value.InnerText, NumberStyles.Number, AppSettings.CInfo, out outValue);

            return outValue;
        }

        internal static Int64 ParseInt64(XmlNode value)
        {
            Int64 outValue = 0;

            if (value != null)
                Int64.TryParse(value.InnerText, NumberStyles.Number, AppSettings.CInfo, out outValue);

            return outValue;
        }

        internal static UInt32 ParseUInt32(XmlNode value)
        {
            UInt32 outValue = 0;

            if (value != null)
                UInt32.TryParse(value.InnerText, NumberStyles.Number, AppSettings.CInfo, out outValue);

            return outValue;
        }

        internal static UInt64 ParseUInt64(XmlNode value)
        {
            UInt64 outValue = 0;

            if (value != null)
                UInt64.TryParse(value.InnerText, NumberStyles.Number, AppSettings.CInfo, out outValue);

            return outValue;
        }

        internal static Single ParseSingle(XmlNode value)
        {
            Single outValue = 0.0f;

            if (value != null)
                Single.TryParse(value.InnerText, NumberStyles.Number, AppSettings.CInfo, out outValue);

            return outValue;
        }

        internal static Double ParseDouble(XmlNode value)
        {
            Double outValue = 0.0d;

            if (value != null)
                Double.TryParse(value.InnerText, NumberStyles.Number, AppSettings.CInfo, out outValue);

            return outValue;
        }

        internal static Boolean ParseBoolean(XmlNode value)
        {
            Boolean outValue = false;

            if (value != null)
                Boolean.TryParse(value.InnerText, out outValue);

            return outValue;
        }

        internal static String ParseString(XmlNode value)
        {
            String outValue = String.Empty;

            if (value != null)
                outValue = value.InnerText;

            return outValue;
        }
    }
}
