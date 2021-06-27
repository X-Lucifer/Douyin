using System;

namespace X.Lucifer
{
    public static class Extensions
    {
        public static string RemoveIllegal(this string content)
        {
            string result = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(content))
                {
                    return result;
                }

                result = content
                    .Replace(@"\", "")
                    .Replace("/", "")
                    .Replace(":", "")
                    .Replace("*", "")
                    .Replace("?", "")
                    .Replace("\"", "")
                    .Replace("<", "")
                    .Replace(">", "")
                    .Replace("|", "");
                return result;
            }
            catch (Exception)
            {
                return content;
            }
        }
    }
}