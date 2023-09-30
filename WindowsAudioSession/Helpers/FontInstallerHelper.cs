using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WindowsAudioSession.Helpers
{
    public class FontInstallerHelper
    {
        [DllImport("gdi32.dll")]
        private static extern int AddFontResource(string lpszFilename);

        [DllImport("gdi32.dll")]
        private static extern int RemoveFontResource(string lpFileName);

        [DllImport("gdi32.dll")]
        private static extern int SendMessage(int hWnd, int Msg, int wParam, int lParam);

        private const int WM_FONTCHANGE = 0x001D;

        public static bool InstallFontFromResource(string resourceName)
        {
            if (IsFontInstalled(resourceName)) return true;

            try
            {
                // Načtení bytů fontu z Resources
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Console.WriteLine("Resource nebyl nalezen.");
                        return false;
                    }

                    // Vytvoření dočasného souboru pro font
                    string tempFontFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".ttf");
                    using (FileStream fileStream = File.Create(tempFontFile))
                    {
                        stream.CopyTo(fileStream);
                        fileStream.Close();
                    }

                    // Instalace fontu
                    int result = AddFontResource(tempFontFile);

                    // Aktualizace systémového fontového cache
                    SendMessage(0xFFFF, WM_FONTCHANGE, 0, 0);

                    // Odstranění dočasného souboru
                    File.Delete(tempFontFile);

                    if (result == 0)
                    {
                        Console.WriteLine("Font nebyl úspěšně nainstalován.");
                        return false;
                    }

                    Console.WriteLine("Font byl úspěšně nainstalován.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chyba: " + ex.Message);
                return false;
            }
        }

        public static bool UninstallFont(string fontFileName)
        {
            int result = RemoveFontResource(fontFileName);

            // Aktualizace systémového fontového cache
            SendMessage(0xFFFF, WM_FONTCHANGE, 0, 0);

            return result != 0;
        }

        public static bool IsFontInstalled(string fontName)
        {
            try
            {
                using (InstalledFontCollection fontsCollection = new InstalledFontCollection())
                {
                    FontFamily[] fontFamilies = fontsCollection.Families;

                    foreach (FontFamily fontFamily in fontFamilies)
                    {
                        if (string.Equals(fontFamily.Name, fontName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chyba: " + ex.Message);
            }

            return false;
        }
    }

}
