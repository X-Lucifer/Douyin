using System;
using System.Text.RegularExpressions;

namespace X.Lucifer
{
    public static class Extensions
    {
        /// <summary>
        /// 移除非法字符
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RemoveIllegal(this string content)
        {
            var result = string.Empty;
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

        /// <summary>
        /// 提取url地址
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string GetUrl(this string content)
        {
            var result = "";
            try
            {
                var match = Regex.Match(content,
                    @"(https?|ftp|file)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]");
                return match.Length > 0 ? match.Value : result;
            }
            catch (Exception)
            {
                return content;
            }
        }
    }
}