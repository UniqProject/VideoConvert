﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializableDictionary.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Collections
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Xml.Serialization;

    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        #region IXmlSerializable Members

        /// <summary>
        /// Get the Schema
        /// </summary>
        /// <returns>
        /// Nothing. We don't use this.
        /// </returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Deserialize some XML into a dictionary
        /// </summary>
        /// <param name="reader">
        /// The reader.
        /// </param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");

                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                TValue value;
                if (reader.Name.Contains("ArrayOfString"))
                {
                    XmlSerializer scSerializer = new XmlSerializer(typeof(StringCollection));
                    value = (TValue)scSerializer.Deserialize(reader);
                }
                else
                {
                    value = (TValue)valueSerializer.Deserialize(reader);
                }
                reader.ReadEndElement();

                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        /// <summary>
        /// Write the Dictionary out to XML
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                TValue value = this[key];

                if (value.GetType() == typeof(StringCollection))
                {
                    XmlSerializer scSerializer = new XmlSerializer(typeof(StringCollection));
                    scSerializer.Serialize(writer, value);
                    writer.WriteEndElement();
                }
                else
                {
                    valueSerializer.Serialize(writer, value);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        }
        #endregion
    }
}