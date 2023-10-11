using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media;
using Un4seen.Bass;
using Windows.ApplicationModel.Background;
using WindowsAudioSession.UI;

namespace WindowsAudioSession.Helpers
{
    public static class DecibelsHelper
    {
        private static Queue<double> decibelQueue = new Queue<double>();
        private static Queue<double> audioLevelsQueue = new Queue<double>();
        private const int shortQueueSize = 5;// 5 is 1 second because timer interval is 200ms
        private const int longQueueSize = 10;

        private static double lastDecibels = 0;
        static DateTime lastChangeTime = DateTime.MinValue; // Čas poslední změny
        private static double decibelsChangeThreshold = 0;

        /// <summary>
        /// 10 dB – Rustling leaves in light wind
        /// 20 dB – Ticking of a wristwatch
        /// 30 dB – Whispering quietly
        /// 40 dB – Quiet room noise
        /// 50 dB – Normal street noise
        /// 60 dB – Normal conversation
        /// 70 dB – Noisy street, typing on a typewriter
        /// 80 dB – Shouting, heavy traffic
        /// 90 dB – Train, loud music
        /// 100 dB – Jackhammer, motorcycle at full throttle
        /// 110 dB – Discotheque
        /// 120 dB – Airplane takeoff, fireworks
        /// 130 dB – Fireworks, crowd noise in a packed stadium
        /// </summary>
        /// <param name="CurrentAudioLevel">Current audio level (low and high int)</param>
        /// <param name="ActualTime">Actual DateTime to know if it's night time.</param>
        /// <param name="DayHighDecibelsThreshold">Set audio level threshold for day in dB.</param>
        /// <param name="NightHighDecibelsThreshold">Set audio level threshold for night in dB.</param>
        /// <param name="MorningHour">Set morning hour when night ends (e.g. 6 for 6:00).</param>
        /// <param name="NightHour">Set night hour when night starts (e.g. 22 for 22:00).</param>
        /// <returns></returns>
        public static KeyValuePair<string, Brush> GetDecibelsForAudioLevel(
            int CurrentAudioLevel,
            DateTime ActualTime,
            decimal DayHighDecibelsThreshold = 75,
            decimal NightHighDecibelsThreshold = 45,
            decimal MorningHour = 6,
            decimal NightHour = 22)
        {
            // Decibels calculation
            double decibelsLeft = LevelTOdB(Utils.LowWord32(CurrentAudioLevel));
            double decibelsRight = LevelTOdB(Utils.HighWord32(CurrentAudioLevel));
            double decibels = Math.Round(Math.Max(decibelsLeft, decibelsRight));

            decibelQueue.Enqueue(decibels);
            if (decibelQueue.Count > shortQueueSize)
            {
                decibelQueue.Dequeue();
            }

            audioLevelsQueue.Enqueue(Math.Max(Utils.LowWord32(CurrentAudioLevel), Utils.HighWord32(CurrentAudioLevel)));
            if (audioLevelsQueue.Count > longQueueSize)
            {
                audioLevelsQueue.Dequeue();
            }

            double averageDecibels = decibelQueue.Average();

            // React immediatelly on start if was silence in queue
            if (averageDecibels < 1 && decibels > 0) averageDecibels = decibels;
            else if (averageDecibels > 1) averageDecibels = Math.Round(Math.Max(LevelTOdB(audioLevelsQueue.Average()), LevelTOdB(audioLevelsQueue.Max())));

            // Change colors of dB display
            bool nightTime = (ActualTime.Hour < MorningHour || ActualTime.Hour >= NightHour);
            bool decibelsHighCondition = averageDecibels > Convert.ToDouble(nightTime ? NightHighDecibelsThreshold : DayHighDecibelsThreshold);

            Brush changingColor;
            if (AreDecibelsChanging(averageDecibels, decibelsChangeThreshold)) changingColor = CustomBrushes.LabelChanging;
            else changingColor = CustomBrushes.Labels;

            Brush brush = averageDecibels < 1 ? CustomBrushes.Labels : (decibelsHighCondition ? CustomBrushes.LabelsHigh : changingColor);

            if(averageDecibels < 1)
            {
                audioLevelsQueue.Clear();
                audioLevelsQueue.Enqueue(0);
            }

            return new KeyValuePair<string, Brush>($"{averageDecibels:F0}", brush);
        }


        public static double LevelTOdB(double audioLevel)
        {
            return 20 * Math.Log10(audioLevel);
        }

        static bool AreDecibelsChanging(double decibels, double threshold)
        {
            TimeSpan timeSinceLastChange = DateTime.Now - lastChangeTime;
            bool result = true;

            if (Math.Abs(decibels - lastDecibels) <= threshold && timeSinceLastChange < TimeSpan.FromSeconds(2))
            {
                result = false;
            }

            lastChangeTime = DateTime.Now;
            lastDecibels = decibels;
            return result;
        }

    }
}
