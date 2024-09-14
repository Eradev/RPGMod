using System.Text.RegularExpressions;

namespace RPGMod.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveReplacementTokens(this string self)
        {
            return Regex.Replace(self, @"\s?{\d+}\s?", string.Empty, RegexOptions.Multiline).Trim();
        }
    }
}
