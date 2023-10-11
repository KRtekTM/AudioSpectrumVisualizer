using System;

namespace WASApiBassNet
{
    public static class AppHelper
    {
        private const int DefaultThreshold = 75;
        //public static string highVolumeThresholdFilePath = Environment.CurrentDirectory + "\\_configVolumeModifier.txt";

        public static int GetHighVolumeThreshold()
        {
            //try
            //{
            //    int highVolumeThreshold = System.IO.File.Exists(highVolumeThresholdFilePath) ? Convert.ToInt32(System.IO.File.ReadAllText(highVolumeThresholdFilePath)) : initialThreshold;
            //    if (highVolumeThreshold > 100) highVolumeThreshold = 100;
            //    else if (highVolumeThreshold < 0) highVolumeThreshold = 0;
            //    return highVolumeThreshold;
            //}
            //catch
            //{
            //    return initialThreshold;
            //}
            return DefaultThreshold;
        }
    }
}
