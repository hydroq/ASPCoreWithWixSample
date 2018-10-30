using System;
using REFLEKT.ONEAuthor.Application.Models;

namespace REFLEKT.ONEAuthor.Application.Utilities
{
    public class LocaleConverter
    {
        public void FixPublishedScenarioTitle(PublishedScenario target)
        {
            target.title = RemoveGermanLocaleSpecifics(target.title);
            target.title = target.title
                .Replace(" ", "_")
                .Replace("[", "")
                .Replace("]", "");
        }

        public string FixFileNaming(string targetFileName)
        {
            targetFileName = RemoveGermanLocaleSpecifics(targetFileName);
            targetFileName = targetFileName
                .Replace(":", ".")
                .Replace("+", "_");

            return targetFileName;
        }

        public static string ToUrlSafeBase64(byte[] source)
        {
            return Convert.ToBase64String(source).Replace("+", "-").Replace("/", "_").Replace("=", "_");
        }

        private string RemoveGermanLocaleSpecifics(string target)
        {
            return target.Replace("ü", "u").Replace("ö", "o").Replace("ä ", "a");
        }
    }
}