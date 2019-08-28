using System.IO;
using Xunit;
using System.Threading.Tasks;

namespace Android.SdkManager.Tests
{
    public class AndroidSdkManagerTests
    {
        public AndroidSdkManagerSettings TestSettings { get; }
            = new AndroidSdkManagerSettings
            {
                SdkRoot = Path.Combine(Directory.GetCurrentDirectory(), ".tmp")
            };

        [Fact]
        public async Task DownloadWorks()
        {
            var asm = new AndroidSdkManager(TestSettings);

            await asm.DownloadSdkAsync();

            Assert.True(Directory.Exists(TestSettings.SdkRoot));
            Assert.NotEmpty(Directory.EnumerateFileSystemEntries(TestSettings.SdkRoot));
        }
    }
}
