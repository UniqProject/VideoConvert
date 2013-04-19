using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace UpdateCore
{
    public class Updater
    {
        private static List<PackageInfo> _updateList;
        
        public static List<PackageInfo> LoadUpdateList (string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<PackageInfo>));
            XmlTextReader xmlTextReader = new XmlTextReader(fileName);
            _updateList = (List<PackageInfo>)serializer.Deserialize(xmlTextReader);
            return _updateList;
        }

        public static void SaveUpdateList(string fileName, List<PackageInfo> updateList)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<PackageInfo>));
            using (FileStream writer = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                serializer.Serialize(writer, updateList);
            }
        }

        private static UpdateFileInfo _updateFile;

        public static UpdateFileInfo LoadUpdateFile(string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UpdateFileInfo));
            XmlTextReader xmlTextReader = new XmlTextReader(fileName);
            _updateFile = (UpdateFileInfo)serializer.Deserialize(xmlTextReader);
            return _updateFile;
        }

        public static UpdateFileInfo LoadUpdateFileFromStream(Stream xmlStream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UpdateFileInfo));
            XmlTextReader xmlTextReader = new XmlTextReader(xmlStream);
            _updateFile = (UpdateFileInfo)serializer.Deserialize(xmlTextReader);
            return _updateFile;
        }

        public static void SaveUpdateFile(string fileName, UpdateFileInfo updateFile)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UpdateFileInfo));
            using (FileStream writer = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                serializer.Serialize(writer, updateFile);
            }
        }
    }
}
