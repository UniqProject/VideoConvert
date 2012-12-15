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
    }
}
