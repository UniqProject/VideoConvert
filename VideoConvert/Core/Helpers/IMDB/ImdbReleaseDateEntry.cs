using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace VideoConvert.Core.Helpers.IMDB
{
    public class ImdbReleaseDateEntry
    {
        [XmlArray("remarks")]
        [XmlArrayItem("item")]
        public List<string> Remarks { get; set; }

        [XmlElement("country")]
        public string Country { get; set; }

        [XmlElement("year")]
        public int Year { get; set; }

        [XmlElement("month")]
        public int Month { get; set; }

        [XmlElement("day")]
        public int Day { get; set; }

        [XmlIgnore]
        public DateTime Date { get { return new DateTime(Year, Month, Day); } }

        public ImdbReleaseDateEntry()
        {
            Remarks = new List<string>();
            Country = string.Empty;
            Year = 0;
            Month = 0;
            Day = 0;
        }
    }
}