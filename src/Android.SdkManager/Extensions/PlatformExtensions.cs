using System;
using System.Runtime.InteropServices;

namespace Android.SdkManager.Extensions
{
    public static class PlatformExtensions
    {
        public static string GetPlatformString(this AndroidSdkManager _)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "windows";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "macosx";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "linux";

            throw new PlatformNotSupportedException($"The current platform is not supported");
        }

        public static OSPlatform FromPlatformStringToOSPlatform(this string platformString)
        {
            switch (platformString)
            {
                case "linux":
                    return OSPlatform.Linux;
                case "windows":
                    return OSPlatform.Windows;
                case "macosx":
                    return OSPlatform.OSX;
                default:
                    throw new ArgumentOutOfRangeException(nameof(platformString), "Invalid platform string");
            }
        }
    }
}
