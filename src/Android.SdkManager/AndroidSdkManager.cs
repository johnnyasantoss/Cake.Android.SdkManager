using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Android.SdkManager.Extensions;
using Android.SdkManager.Model;

namespace Android.SdkManager
{
    public class AndroidSdkManager : IDisposable
    {
        public const string RepositoryUrlBase = "https://dl.google.com/android/repository/";
        public static Uri RepositoryUriBase = new Uri(RepositoryUrlBase);
        public const string RepositoryUrl = RepositoryUrlBase + "repository2-1.xml";
        public const string RepositorySdkPattern = RepositoryUrlBase + "tools_r{0}.{1}.{2}-{3}.zip";

        private string _platformString;

        public Version FallbackVersion { get; set; } = new Version(25, 2, 5);

        public AndroidSdkManagerSettings Settings { get; }

        public XmlDocument RepositoryInfo { get; set; }

        public AndroidSdkTools[] AvailableAndroidSdkTools { get; set; }

        public string PlatformString => _platformString ??= this.GetPlatformString();

        public HttpClient HttpClient { get; } = GetHttpClient();

        public AndroidSdkManager(AndroidSdkManagerSettings settings = null)
        {
            Settings = settings ?? new AndroidSdkManagerSettings();
        }

        ~AndroidSdkManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// Downloads the Android SDK.
        /// Uses <see cref="Settings"/> to locate the tools directory
        /// </summary>
        /// <param name="specificVersion">Specific version, or latest if none is specified.</param>
        public async Task DownloadSdkAsync(Version specificVersion = null)
        {
            if (specificVersion == null)
            {
                try
                {
                    specificVersion = await GetLatestVersion()
                        .ConfigureAwait(false);
                }
                catch
                {
                    //TODO: Change this fallback
                    specificVersion = FallbackVersion;
                }
            }

            var sdkToolInfo = await GetSdkToolInfo(specificVersion)
                .ConfigureAwait(false);

            var sdkZipFile = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "android-sdk.zip"));

            void CleanTempFile()
            {
                if (File.Exists(sdkZipFile))
                    File.Delete(sdkZipFile);
            }

            //TODO: Maybe impl a checksum to reuse the file
            CleanTempFile();

            try
            {
                var streamTask = HttpClient.GetStreamAsync(sdkToolInfo.DownloadUri)
                    .ConfigureAwait(false);

                using (var httpStream = await streamTask)
                using (var fileStream = File.Open(sdkZipFile, FileMode.OpenOrCreate))
                {
                    await httpStream.CopyToAsync(fileStream)
                        .ConfigureAwait(false);
                }

                var sdkRoot = Path.GetFullPath(Settings.SdkRoot);
                if (!Directory.Exists(sdkRoot))
                    Directory.CreateDirectory(sdkRoot);

                ZipFile.ExtractToDirectory(sdkZipFile, sdkRoot);
            }
            finally
            {
                CleanTempFile();
            }
        }

        public async Task<AndroidSdkArchive> GetSdkToolInfo(Version version)
        {
            var tools = await GetAvailableSdkTools()
                .ConfigureAwait(false);

            return (from tool in tools
                where tool.Version == version
                from archive in tool.ArchiveInfo
                where archive.Platform == PlatformString.FromPlatformStringToOSPlatform()
                select archive).FirstOrDefault();
        }

        public async Task<AndroidSdkTools[]> GetAvailableSdkTools()
        {
            if (AvailableAndroidSdkTools != null)
                return AvailableAndroidSdkTools;

            await LoadRepositoryInfoAsync()
                .ConfigureAwait(false);

            var toolsNodes = RepositoryInfo
                .MustSelectNodes("//remotePackage[@path='tools']");

            return AvailableAndroidSdkTools ??=
                AndroidSdkTools.FromXmlNodeList(toolsNodes);
        }

        public async Task<Version> GetLatestVersion()
        {
            var versions = await GetAvailableSdkTools()
                .ConfigureAwait(false);

            return versions.Max(v => v.Version);
        }

        private async Task LoadRepositoryInfoAsync()
        {
            //TODO: needs better caching, maybe per repo
            if (RepositoryInfo != null)
                return;

            var data = await HttpClient.GetStringAsync(RepositoryUrl)
                .ConfigureAwait(false);

            RepositoryInfo = new XmlDocument
            {
                PreserveWhitespace = false
            };

            RepositoryInfo.LoadXml(data);
        }

        private static HttpClient GetHttpClient()
        {
            //TODO: Move this to another class
            var http = new HttpClient();

            http.DefaultRequestHeaders.TryAddWithoutValidation(
                "Accept", "text/html,application/xhtml+xml,application/xml"
            );
            http.DefaultRequestHeaders.TryAddWithoutValidation(
                "Accept-Encoding", "gzip, deflate"
            );
            http.DefaultRequestHeaders.TryAddWithoutValidation(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0"
            );
            http.DefaultRequestHeaders.TryAddWithoutValidation(
                "Accept-Charset", "ISO-8859-1"
            );

            return http;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                HttpClient?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
