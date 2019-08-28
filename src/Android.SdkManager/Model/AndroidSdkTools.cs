using System;
using System.Linq;
using System.Xml;
using Android.SdkManager.Extensions;

namespace Android.SdkManager.Model
{
    public struct AndroidSdkTools
    {
        public AndroidSdkTools(Version version, AndroidSdkArchive[] archiveInfo)
        {
            Version = version;
            ArchiveInfo = archiveInfo;
        }

        public Version Version { get; }

        public AndroidSdkArchive[] ArchiveInfo { get; }

        public static AndroidSdkTools[] FromXmlNodeList(XmlNodeList toolsNodes)
        {
            return (from XmlElement toolsNode in toolsNodes
                    select FromXml(toolsNode))
                .ToArray();
        }

        public static AndroidSdkTools FromXml(XmlElement toolsNode)
        {
            var revision = toolsNode.MustSelectSingleNode("revision");
            var archives = toolsNode.MustSelectNodes("archives");

            return new AndroidSdkTools(
                revision.FromRevisionToVersion(),
                AndroidSdkArchive.FromXml(archives)
            );
        }
    }
}
