using AnyAscii;
using System;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Foundation.Metadata;

namespace WindowsAudioSession.Helpers
{
    public static class TextHelper
    {
        public static string RemoveDiacritics(string input)
        {
            string result = TryTrimNotesInSongTitle(input.Transliterate());
            if (result.Length > 28)
            {
                result = $" {result} ";
            }
            return result;
        }

        [Deprecated("We are using AnyAscii .Transliterate() function now.", DeprecationType.Remove, 0)]
        public static string RemoveDiacriticsDeprecated(string input)
        {
            string accentsToReplace = "ÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝßàáâãäåçèéêëìíîïñòóôõöùúûüýÿĀāĂăĄąĆćĈĉĊċČčĎďĐđĒēĔĕĖėĘęĚěĜĝĞğĠġĢģĤĥĦħĨĩĪīĬĭĮįİıĶķĸĹĺĻļĽľſŁłŃńŅņŇňŉŊŋŌōŎŏŐőŔŕŖŗŘřŚśŜŝŞşŠšŢţŤťŦŧŨũŪūŬŭŮůŰűŲųŴŵŶŷŸŹźŻżŽž";
            string accentsAscii = "AAAAAACEEEEIIIINOOOOOUUUUYsaaaaaaceeeeiiiinooooouuuuyyAaAaAaCcCcCcCcDdDdEeEeEeEeEeGgGgGgGgHhHhIiIiIiIiIiKkkLlLlLllLlNnNnNnNnNOoOoOoRrRrRrSsSsSsSsTtTtTtUuUuUuUuUuUuWwYyYZzZzZz";

            for (int i = 0; i < accentsToReplace.Length; i++)
            {
                input = input.Replace(accentsToReplace[i], accentsAscii[i]);
            }

            return RemoveDiacriticsAndConvertToAscii(input);
        }

        [Deprecated("We are using AnyAscii .Transliterate() function now.", DeprecationType.Deprecate, 0)]
        private static string RemoveDiacriticsAndConvertToAscii(string input)
        {
            // Nahradí diakritiku za znaky bez diakritiky
            string normalizedString = input.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                // Pokud je znak ASCII, přidáme ho k výstupu
                if ((int)c <= 127)
                {
                    stringBuilder.Append(c);
                }
                // Jinak převedeme znak na ekvivalentní ASCII znak, pokud možno
                else
                {
                    stringBuilder.Append(ConvertToAscii(c));
                }
            }


            string result;
            result = stringBuilder.ToString();
            return result;
        }

        [Deprecated("We are using AnyAscii .Transliterate() function now.", DeprecationType.Deprecate, 0)]
        private static string ConvertToAscii(char c)
        {
            try
            {
                // Převede znak na jeho ekvivalentní ASCII reprezentaci
                byte[] bytes = Encoding.GetEncoding("ISO-8859-8").GetBytes(c.ToString());
                return Encoding.ASCII.GetString(bytes);
            }
            catch (Exception)
            {
                // Pokud se nepodaří převést znak na ASCII, vrátíme prázdný řetězec
                return string.Empty;
            }
        }

        public static string TryTrimNotesInSongTitle(string inputText)
        {
            // Regulární výraz pro hledání a odstranění podřetězců v závorkách s ignorováním velikosti písmen
            // REGEX which matches all things like (Official video) [REMASTERED 4k] etc.
            string pattern = @"\s*\[[^\]]*(?:video|official|remastered|remaster|hd|original)[^\]]*\]|\s*\([^)]*(?:video|official|remastered|remaster|hd|original)[^)]*\)";
            
            return Regex.Replace(inputText, pattern, "", RegexOptions.IgnoreCase);
        }

        internal static bool DetectRickAstley(string audioSource)
        {
            audioSource = audioSource.ToLowerInvariant();

            bool result = audioSource.Contains("rick");
            result &= audioSource.Contains("astley");
            result &= audioSource.Contains("never");
            result &= audioSource.Contains("gonna");
            result &= audioSource.Contains("give");
            result &= audioSource.Contains("you");
            result &= audioSource.Contains("up");

            return result;
        }
    }
}
