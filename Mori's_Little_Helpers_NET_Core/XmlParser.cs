using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Schweigm_NETCore_Helpers
{
    public static class XmlParser
    {
        /// <summary>
        /// Load all XML Files in the given folder and adds them to a list
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<XmlDocument> LoadXmlFiles(string subFolderPath)
        {
            var retVal = new List<XmlDocument>();
            var folderPath = Directory.GetCurrentDirectory();
            folderPath += subFolderPath;


            foreach (var xmlName in Directory.EnumerateFiles(folderPath, "*.xml"))
            {
                try
                {
                    var doc = new XmlDocument();
                    doc.Load(xmlName);
                    retVal.Add(doc);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return retVal;
        }
    }
}
