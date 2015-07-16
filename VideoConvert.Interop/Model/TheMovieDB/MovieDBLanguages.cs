// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MovieDBLanguages.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   List of Languages supported by TheMovieDB Lib
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// List of Languages supported by TheMovieDB Lib
    /// </summary>
    public static class MovieDbLanguages
    {
        /// <summary>
        /// Raw language list
        /// </summary>
        public static List<MovieDbLanguage> LangList => GenerateLangList();


        /// <summary>
        /// Sorted language list
        /// </summary>
        public static List<MovieDbLanguage> SortedLangList
        {
            get
            {
                var sList = GenerateLangList();
                sList.Sort((language, dbLanguage) => string.Compare(language.Name, dbLanguage.Name, StringComparison.Ordinal));
                return sList;
            }
        }

        /// <summary>
        /// Create a List of supported languages
        /// </summary>
        /// <returns></returns>
        private static List<MovieDbLanguage> GenerateLangList()
        {
            var result = new List<MovieDbLanguage>
                {
                    new MovieDbLanguage {Code = "aa", Name = "Afar"},
                    new MovieDbLanguage {Code = "af", Name = "Afrikaans"},
                    new MovieDbLanguage {Code = "ak", Name = "Akana"},
                    new MovieDbLanguage {Code = "am", Name = "Amharic"},
                    new MovieDbLanguage {Code = "an", Name = "Aragones"},
                    new MovieDbLanguage {Code = "ar", Name = "Arabic"},
                    new MovieDbLanguage {Code = "as", Name = "Assamese"},
                    new MovieDbLanguage {Code = "ay", Name = "Aymara"},
                    new MovieDbLanguage {Code = "az", Name = "Azeri"},
                    new MovieDbLanguage {Code = "bar", Name = "Boarisch"},
                    new MovieDbLanguage {Code = "be", Name = "Belarusian"},
                    new MovieDbLanguage {Code = "bg", Name = "Bulgarian"},
                    new MovieDbLanguage {Code = "bh", Name = "Bihari"},
                    new MovieDbLanguage {Code = "bi", Name = "Bislama"},
                    new MovieDbLanguage {Code = "bm", Name = "Bambara"},
                    new MovieDbLanguage {Code = "bn", Name = "Bengali"},
                    new MovieDbLanguage {Code = "bo", Name = "Tibetan"},
                    new MovieDbLanguage {Code = "br", Name = "Breton"},
                    new MovieDbLanguage {Code = "bs", Name = "Bosnian"},
                    new MovieDbLanguage {Code = "ca", Name = "Catalan"},
                    new MovieDbLanguage {Code = "ce", Name = "Chechen"},
                    new MovieDbLanguage {Code = "ch", Name = "Chamorro"},
                    new MovieDbLanguage {Code = "co", Name = "Corsican"},
                    new MovieDbLanguage {Code = "cr", Name = "Cree"},
                    new MovieDbLanguage {Code = "cs", Name = "Czech"},
                    new MovieDbLanguage {Code = "cu", Name = "Old Slavonic"},
                    new MovieDbLanguage {Code = "cv", Name = "Chuvash"},
                    new MovieDbLanguage {Code = "cy", Name = "Welsh"},
                    new MovieDbLanguage {Code = "da", Name = "Danish"},
                    new MovieDbLanguage {Code = "de", Name = "Deutsch"},
                    new MovieDbLanguage {Code = "dv", Name = "Divehi"},
                    new MovieDbLanguage {Code = "dz", Name = "Dzongkha"},
                    new MovieDbLanguage {Code = "ee", Name = "Ewe"},
                    new MovieDbLanguage {Code = "el", Name = "Greek"},
                    new MovieDbLanguage {Code = "en", Name = "English"},
                    new MovieDbLanguage {Code = "eo", Name = "Esperanto"},
                    new MovieDbLanguage {Code = "es", Name = "Spanish"},
                    new MovieDbLanguage {Code = "et", Name = "Estonian"},
                    new MovieDbLanguage {Code = "eu", Name = "Basque"},
                    new MovieDbLanguage {Code = "fa", Name = "Persian"},
                    new MovieDbLanguage {Code = "ff", Name = "Fula"},
                    new MovieDbLanguage {Code = "fi", Name = "Finnish"},
                    new MovieDbLanguage {Code = "fj", Name = "Fijian"},
                    new MovieDbLanguage {Code = "fo", Name = "Faroese"},
                    new MovieDbLanguage {Code = "fr", Name = "French"},
                    new MovieDbLanguage {Code = "fy", Name = "Western Frisian"},
                    new MovieDbLanguage {Code = "ga", Name = "Irish"},
                    new MovieDbLanguage {Code = "gd", Name = "(Scottish) Gaelic"},
                    new MovieDbLanguage {Code = "gl", Name = "Galician"},
                    new MovieDbLanguage {Code = "gn", Name = "Guarani"},
                    new MovieDbLanguage {Code = "gu", Name = "Gujarati"},
                    new MovieDbLanguage {Code = "gv", Name = "Gaelg"},
                    new MovieDbLanguage {Code = "ha", Name = "Hausa"},
                    new MovieDbLanguage {Code = "he", Name = "Hebrew"},
                    new MovieDbLanguage {Code = "hi", Name = "Hindi"},
                    new MovieDbLanguage {Code = "ho", Name = "Hiri Motu"},
                    new MovieDbLanguage {Code = "hr", Name = "Croatian"},
                    new MovieDbLanguage {Code = "ht", Name = "Haitian"},
                    new MovieDbLanguage {Code = "hu", Name = "Hungarian"},
                    new MovieDbLanguage {Code = "hy", Name = "Armenian"},
                    new MovieDbLanguage {Code = "hz", Name = "Herero"},
                    new MovieDbLanguage {Code = "ia", Name = "Interlingua"},
                    new MovieDbLanguage {Code = "id", Name = "Indonesian"},
                    new MovieDbLanguage {Code = "ie", Name = "Interlingue"},
                    new MovieDbLanguage {Code = "ig", Name = "Igbo"},
                    new MovieDbLanguage {Code = "ii", Name = "Nuosu"},
                    new MovieDbLanguage {Code = "ik", Name = "Inupiaq"},
                    new MovieDbLanguage {Code = "io", Name = "Ido"},
                    new MovieDbLanguage {Code = "is", Name = "Icelandic"},
                    new MovieDbLanguage {Code = "it", Name = "Italian"},
                    new MovieDbLanguage {Code = "iu", Name = "Inuktitut"},
                    new MovieDbLanguage {Code = "ja", Name = "Japanese"},
                    new MovieDbLanguage {Code = "jv", Name = "Javanese"},
                    new MovieDbLanguage {Code = "ka", Name = "Georgian"},
                    new MovieDbLanguage {Code = "kg", Name = "Kongo"},
                    new MovieDbLanguage {Code = "ki", Name = "Kikuyu"},
                    new MovieDbLanguage {Code = "kj", Name = "Kwanyama"},
                    new MovieDbLanguage {Code = "kk", Name = "Kazakh"},
                    new MovieDbLanguage {Code = "kl", Name = "Kalaallisut"},
                    new MovieDbLanguage {Code = "km", Name = "Khmer"},
                    new MovieDbLanguage {Code = "kn", Name = "Kannada"},
                    new MovieDbLanguage {Code = "ko", Name = "Korean"},
                    new MovieDbLanguage {Code = "kr", Name = "Kanuri"},
                    new MovieDbLanguage {Code = "ks", Name = "Kashmiri"},
                    new MovieDbLanguage {Code = "ku", Name = "Kurdish"},
                    new MovieDbLanguage {Code = "kv", Name = "Komi"},
                    new MovieDbLanguage {Code = "kw", Name = "Cornish"},
                    new MovieDbLanguage {Code = "ky", Name = "Kyrgyz"},
                    new MovieDbLanguage {Code = "la", Name = "Latin"},
                    new MovieDbLanguage {Code = "lb", Name = "Lëtzebuergesch"},
                    new MovieDbLanguage {Code = "lg", Name = "Ganda"},
                    new MovieDbLanguage {Code = "li", Name = "Limburgish"},
                    new MovieDbLanguage {Code = "ln", Name = "Lingala"},
                    new MovieDbLanguage {Code = "lo", Name = "Lao"},
                    new MovieDbLanguage {Code = "lt", Name = "Lithuanian"},
                    new MovieDbLanguage {Code = "lv", Name = "Latvian"},
                    new MovieDbLanguage {Code = "mg", Name = "Malagasy"},
                    new MovieDbLanguage {Code = "mh", Name = "Marshallese"},
                    new MovieDbLanguage {Code = "mi", Name = "Maori"},
                    new MovieDbLanguage {Code = "mk", Name = "Macedonian"},
                    new MovieDbLanguage {Code = "ml", Name = "Malayalam"},
                    new MovieDbLanguage {Code = "mn", Name = "Mongolian"},
                    new MovieDbLanguage {Code = "mr", Name = "Marathi"},
                    new MovieDbLanguage {Code = "ms", Name = "Bahasa Melayu"},
                    new MovieDbLanguage {Code = "mt", Name = "Maltese"},
                    new MovieDbLanguage {Code = "my", Name = "Burmese"},
                    new MovieDbLanguage {Code = "na", Name = "Nauru"},
                    new MovieDbLanguage {Code = "nd", Name = "North Ndebele"},
                    new MovieDbLanguage {Code = "ne", Name = "Nepali"},
                    new MovieDbLanguage {Code = "ng", Name = "Ndonga"},
                    new MovieDbLanguage {Code = "nl", Name = "Nederlands"},
                    new MovieDbLanguage {Code = "nn", Name = "Norwegian (nynorsk)"},
                    new MovieDbLanguage {Code = "no", Name = "Norwegian"},
                    new MovieDbLanguage {Code = "nr", Name = "South Ndebele"},
                    new MovieDbLanguage {Code = "oc", Name = "Occitan"},
                    new MovieDbLanguage {Code = "oj", Name = "Ojibwe"},
                    new MovieDbLanguage {Code = "om", Name = "Oromo"},
                    new MovieDbLanguage {Code = "or", Name = "Oriya"},
                    new MovieDbLanguage {Code = "os", Name = "Ossetian"},
                    new MovieDbLanguage {Code = "pa", Name = "Panjabi / Punjabi"},
                    new MovieDbLanguage {Code = "pi", Name = "Pali"},
                    new MovieDbLanguage {Code = "pl", Name = "Polish"},
                    new MovieDbLanguage {Code = "ps", Name = "Pashto"},
                    new MovieDbLanguage {Code = "pt", Name = "Portugues"},
                    new MovieDbLanguage {Code = "qu", Name = "Quechua"},
                    new MovieDbLanguage {Code = "rm", Name = "Romansh"},
                    new MovieDbLanguage {Code = "rn", Name = "Kirundi"},
                    new MovieDbLanguage {Code = "ro", Name = "Romanian"},
                    new MovieDbLanguage {Code = "ru", Name = "Russian"},
                    new MovieDbLanguage {Code = "sa", Name = "Sanskrit"},
                    new MovieDbLanguage {Code = "sc", Name = "Sardinian"},
                    new MovieDbLanguage {Code = "sd", Name = "Sindhi"},
                    new MovieDbLanguage {Code = "se", Name = "Northern Sami"},
                    new MovieDbLanguage {Code = "sg", Name = "Sango"},
                    new MovieDbLanguage {Code = "si", Name = "Sinhala"},
                    new MovieDbLanguage {Code = "sk", Name = "Slovak"},
                    new MovieDbLanguage {Code = "sl", Name = "Slovene"},
                    new MovieDbLanguage {Code = "sm", Name = "Samoan"},
                    new MovieDbLanguage {Code = "sn", Name = "Shona"},
                    new MovieDbLanguage {Code = "so", Name = "Somali"},
                    new MovieDbLanguage {Code = "sq", Name = "Albanian"},
                    new MovieDbLanguage {Code = "sr", Name = "Serbian"},
                    new MovieDbLanguage {Code = "ss", Name = "Swati"},
                    new MovieDbLanguage {Code = "st", Name = "Southern Sotho"},
                    new MovieDbLanguage {Code = "su", Name = "Sundanese"},
                    new MovieDbLanguage {Code = "sv", Name = "Swedish"},
                    new MovieDbLanguage {Code = "sw", Name = "Swahili"},
                    new MovieDbLanguage {Code = "ta", Name = "Tamil"},
                    new MovieDbLanguage {Code = "te", Name = "Telugu"},
                    new MovieDbLanguage {Code = "tg", Name = "Tajik"},
                    new MovieDbLanguage {Code = "th", Name = "Thai"},
                    new MovieDbLanguage {Code = "ti", Name = "Tigrinya"},
                    new MovieDbLanguage {Code = "tk", Name = "Turkmen"},
                    new MovieDbLanguage {Code = "tl", Name = "Tagalog"},
                    new MovieDbLanguage {Code = "tn", Name = "Tswana"},
                    new MovieDbLanguage {Code = "to", Name = "Tonga"},
                    new MovieDbLanguage {Code = "tr", Name = "Turkish"},
                    new MovieDbLanguage {Code = "ts", Name = "Tsonga"},
                    new MovieDbLanguage {Code = "tt", Name = "Tatar"},
                    new MovieDbLanguage {Code = "tw", Name = "Twi"},
                    new MovieDbLanguage {Code = "ty", Name = "Tahitian"},
                    new MovieDbLanguage {Code = "ug", Name = "Uighur"},
                    new MovieDbLanguage {Code = "uk", Name = "Ukrainian"},
                    new MovieDbLanguage {Code = "ur", Name = "Urdu"},
                    new MovieDbLanguage {Code = "uz", Name = "Uzbek"},
                    new MovieDbLanguage {Code = "ve", Name = "Venda"},
                    new MovieDbLanguage {Code = "vi", Name = "Vietnamese"},
                    new MovieDbLanguage {Code = "vo", Name = "Volapük"},
                    new MovieDbLanguage {Code = "wa", Name = "Walloon"},
                    new MovieDbLanguage {Code = "wo", Name = "Wolof"},
                    new MovieDbLanguage {Code = "xh", Name = "Xhosa"},
                    new MovieDbLanguage {Code = "yi", Name = "Yiddish"},
                    new MovieDbLanguage {Code = "yo", Name = "Yoruba"},
                    new MovieDbLanguage {Code = "za", Name = "Zhuang"},
                    new MovieDbLanguage {Code = "zh", Name = "Chinese"},
                    new MovieDbLanguage {Code = "zu", Name = "Zulu"}
                };

            return result;
        }
    }
}
