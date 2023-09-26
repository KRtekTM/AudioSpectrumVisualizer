using System;

namespace WASApiBassNet
{
    public static class AppHelper
    {
        private const int DefaultThreshold = 100;

        public static int GetHighVolumeThreshold(int initialThreshold = DefaultThreshold)
        {
            try
            {
                string highVolumeThresholdFilePath = Environment.CurrentDirectory + "\\_configVolumeModifier.txt";
                int highVolumeThreshold = System.IO.File.Exists(highVolumeThresholdFilePath) ? Convert.ToInt32(System.IO.File.ReadAllText(highVolumeThresholdFilePath)) : initialThreshold;
                if (highVolumeThreshold > 100) highVolumeThreshold = 100;
                return highVolumeThreshold;
            }
            catch
            {
                return initialThreshold;
            }
        }
    }
}
