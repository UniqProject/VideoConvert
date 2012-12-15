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

using System.Collections.Generic;

namespace VideoConvert.Core.Helpers
{
    public class SupportedLanguage
    {
        public SupportedLanguage(string langName, string langCode)
        {
            LangName = langName;
            LangCode = langCode;
        }

        public string LangName { get; private set; }

        public string LangCode { get; private set; }

        public static List<SupportedLanguage> GetSupportedLanguages()
        {
            List<SupportedLanguage> langList = new List<SupportedLanguage>
                                                   {
                                                       new SupportedLanguage("System", "system"),
                                                       new SupportedLanguage("English", "en-US"),
                                                       new SupportedLanguage("Deutsch", "de-DE")
                                                   };

            return langList;
        }
    }
}