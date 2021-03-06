﻿//============================================================================
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
using System.Xml;
using System.Xml.Serialization;

namespace VideoConvert.Core.Helpers.TheMovieDB
{
    class TMDbSerializer
    {
        private static readonly XmlSerializerNamespaces SerializerNamespaces;

        static TMDbSerializer()
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
            XmlSerializer s = new XmlSerializer(obj.GetType());

            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter sw = new StreamWriter(stream))
            {
                s.Serialize(sw, obj, alternateNamespaces ?? SerializerNamespaces);

                stream.Position = 0;
                stream.Flush();

                XmlDocument doc = new XmlDocument();
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
            T tmp = Activator.CreateInstance<T>();
            XmlSerializer serializer = new XmlSerializer(tmp.GetType());
            if (doc.DocumentElement != null)
            {
                T objectToSerialize = (T)serializer.Deserialize(new XmlNodeReader(doc.DocumentElement));
                return objectToSerialize;
            }
            return default(T);
        }
    }
}
