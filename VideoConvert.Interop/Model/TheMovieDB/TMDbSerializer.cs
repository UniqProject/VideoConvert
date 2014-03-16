// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TMDbSerializer.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The TmDbSerializer
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.TheMovieDB
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// The TmDbSerializer
    /// </summary>
    public class TmDbSerializer
    {
        private static readonly XmlSerializerNamespaces SerializerNamespaces;

        static TmDbSerializer()
        {
            SerializerNamespaces = new XmlSerializerNamespaces();
            SerializerNamespaces.Add("", "");
        }

        /// <summary>
        /// Serialize an object to an XMLDocument using the built-in xml serializer.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize</typeparam>
        /// <param name="obj">Object to serialize</param>
        /// <param name="alternateNamespaces"> </param>
        /// <returns>XmlDocument containing the serialized data</returns>
        public static XmlDocument Serialize<T>(T obj, XmlSerializerNamespaces alternateNamespaces = null)
        {
            var s = new XmlSerializer(obj.GetType());

            using (var stream = new MemoryStream())
            using (var sw = new StreamWriter(stream))
            {
                s.Serialize(sw, obj, alternateNamespaces ?? SerializerNamespaces);

                stream.Position = 0;
                stream.Flush();

                var doc = new XmlDocument();
                doc.Load(stream);
                return doc;
            }
        }

        /// <summary>
        /// Deserialize the XmlDocument given, into an object of type T.
        /// T must have a parameterless constructor.
        /// </summary>
        /// <typeparam name="T">Type of the object to deserialize</typeparam>
        /// <param name="doc">The document to deserialize</param>
        /// <returns>A fresh object containing the information from the document</returns>
        public static T Deserialize<T>(XmlDocument doc)
        {
            // Use awesomeness of Activator
            var tmp = Activator.CreateInstance<T>();
            var serializer = new XmlSerializer(tmp.GetType());
            if (doc.DocumentElement != null)
            {
                var objectToSerialize = (T)serializer.Deserialize(new XmlNodeReader(doc.DocumentElement));
                return objectToSerialize;
            }
            return default(T);
        }
    }
}
