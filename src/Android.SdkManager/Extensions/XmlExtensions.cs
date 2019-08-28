using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;

namespace Android.SdkManager.Extensions
{
    public static class XmlExtensions
    {
        public static Version FromRevisionToVersion(this XmlNode revision)
        {
            if (revision == null)
                throw new ArgumentNullException(nameof(revision));

            var major = revision.MustSelectSingleNode("major");
            var minor = revision.MustSelectSingleNode("minor");
            var micro = revision.MustSelectSingleNode("micro");

            var strVer = major.InnerText
                         + "." + minor.InnerText
                         + "." + micro.InnerText;

            return Version.Parse(strVer);
        }

        public static OSPlatform FromHostOSToOSPlatform(this XmlNode hostOs)
        {
            return hostOs.InnerText.FromPlatformStringToOSPlatform();
        }

        public static XmlNodeList MustSelectNodes(this XmlNode xmlNode, string xpath)
        {
            var nodes = xmlNode.SelectNodes(xpath);

            if (nodes == null)
                throw new InvalidDataException(
                    "Invalid node selection." + Environment.NewLine
                                              + "XPath: " + xpath
                );

            return nodes;
        }

        public static XmlNode MustSelectSingleNode(this XmlNode xmlNode, string childNodeName)
        {
            var node = xmlNode.SelectSingleNode(childNodeName);

            if (node == null)
                throw new InvalidDataException("Could not find complete tag");

            return node;
        }
    }
}
