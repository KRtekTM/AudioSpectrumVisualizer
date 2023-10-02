using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;

namespace WindowsAudioSession.Helpers
{
    public static class TextHelper
    {
        public static string RemoveDiacritics(string input)
        {
            string accentsToReplace = "ÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝßàáâãäåçèéêëìíîïñòóôõöùúûüýÿĀāĂăĄąĆćĈĉĊċČčĎďĐđĒēĔĕĖėĘęĚěĜĝĞğĠġĢģĤĥĦħĨĩĪīĬĭĮįİıĶķĸĹĺĻļĽľſŁłŃńŅņŇňŉŊŋŌōŎŏŐőŔŕŖŗŘřŚśŜŝŞşŠšŢţŤťŦŧŨũŪūŬŭŮůŰűŲųŴŵŶŷŸŹźŻżŽž";
            string accentsAscii =     "AAAAAACEEEEIIIINOOOOOUUUUYsaaaaaaceeeeiiiinooooouuuuyyAaAaAaCcCcCcCcDdDdEeEeEeEeEeGgGgGgGgHhHhIiIiIiIiIiKkkLlLlLllLlNnNnNnNnNOoOoOoRrRrRrSsSsSsSsTtTtTtUuUuUuUuUuUuWwYyYZzZzZz";

            for (int i = 0; i < accentsToReplace.Length; i++)
            {
                input = input.Replace(accentsToReplace[i], accentsAscii[i]);
            }

            input = RemoveDiacriticsAndConvertToAscii(input);
            return input;
        }

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
            result = TryTrimNotesInSongTitle(stringBuilder.ToString());
            if (stringBuilder.Length > 28)
            {
                result = $" {result} ";
            }
            return result;
        }

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
    }
}
