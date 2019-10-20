using System;
using System.Text;

namespace Android.SdkManager.Extensions
{
    public static class StringExtensions
    {
        public static ulong ToULong(this string str)
        {
            return Convert.ToUInt64(str);
        }

        public static string ToHexString(this byte[] hashBytes)
        {
            var sb = new StringBuilder(hashBytes.Length * 2);

            foreach (var b in hashBytes)
            {
                sb.AppendFormat(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
