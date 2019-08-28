using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using Android.SdkManager.Extensions;

namespace Android.SdkManager.Model
{
    public struct AndroidSdkArchive
    {
        public AndroidSdkArchive(ulong sizeInBytes, string checksum, Uri downloadUri, OSPlatform platform)
        {
            SizeInBytes = sizeInBytes;
            Checksum = checksum;
            DownloadUri = downloadUri;
            Platform = platform;
        }

        public ulong SizeInBytes { get; }

        public string Checksum { get; }

        public Uri DownloadUri { get; }

        public OSPlatform Platform { get; }

        public static AndroidSdkArchive[] FromXml(XmlNodeList listArchives)
        {
            if (listArchives == null)
                throw new InvalidDataException("Could not find archives tag");

            return (
                    from XmlNode archives in listArchives
                    from XmlNode archive in archives.MustSelectNodes("archive")
                    let complete = archive.MustSelectSingleNode("complete")
                    let hostOs = archive.MustSelectSingleNode("host-os")
                    let size = complete.MustSelectSingleNode("size")
                    let checksum = complete.MustSelectSingleNode("checksum")
                    let url = complete.MustSelectSingleNode("url")
                    select new AndroidSdkArchive(
                        size.InnerText.ToULong(),
                        checksum.InnerText,
                        new Uri(AndroidSdkManager.RepositoryUriBase, url.InnerText),
                        hostOs.FromHostOSToOSPlatform()
                    )
                )
                .ToArray();
        }
    }
}
