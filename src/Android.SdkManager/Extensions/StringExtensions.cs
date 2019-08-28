using System;

namespace Android.SdkManager.Extensions
{
    public static class StringExtensions
    {
        public static ulong ToULong(this string str)
        {
            return Convert.ToUInt64(str);
        }
    }
}
