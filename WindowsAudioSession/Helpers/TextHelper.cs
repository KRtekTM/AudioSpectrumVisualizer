using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAudioSession.Helpers
{
    public static class TextHelper
    {
        public static string RemoveDiacriticsAndConvertToAscii(string input)
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
            if(stringBuilder.Length > 28)
            {
                result = $" {stringBuilder} ";
            }
            else
            {
                result = stringBuilder.ToString();
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
    }
}
