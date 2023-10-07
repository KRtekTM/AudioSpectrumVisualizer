using System;
using Un4seen.Bass;
using Un4seen.BassWasapi;
using WASApiBassNet.Components.AudioCapture;
using AudioSwitcher.AudioApi.CoreAudio;

namespace WASApiBassNet.Components.SoundLevel
{
    public class SoundLevelCapture : ISoundLevelCapture, IAudioPlugin
    {
        private CoreAudioDevice audioController = new CoreAudioController().DefaultPlaybackDevice;
        /// <inheritdoc/>
        public bool IsStarted { get; protected set; }

        /// <inheritdoc/>
        public int LevelLeft { get; protected set; }

        /// <inheritdoc/>
        public int LevelRight { get; protected set; }

        /// <inheritdoc/>
        public void HandleTick()
        {
            if (!IsStarted) return;

            var level = BassWasapi.BASS_WASAPI_GetLevel();
            if (level == -1)
            {
                Stop();
                WASApiBassNetHelper.ThrowsAudioApiErrorException("BASS_WASAPI_GetLevel failed");
            }
            else
            {
                LevelLeft = ModifyVolumeLevelToShow(Utils.LowWord32(level));
                LevelRight = ModifyVolumeLevelToShow(Utils.HighWord32(level));
            }
        }

        /// <summary>
        /// When lower volumes, increase current volume value by 10 to make it visible.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private int ModifyVolumeLevelToShow(int level)
        {
            int volumeModifier = MapPercentToNumber(audioController.Volume);
            return level * volumeModifier;
        }

        int MapPercentToNumber(double percent)
        {
            int highVolumeThreshold = AppHelper.GetHighVolumeThreshold(26);

            if (percent > highVolumeThreshold)
            {
                // Pokud je hlasitost vyšší než highVolumeThreshold, použijeme násobič 1
                return 1;
            }
            else if (percent > 16)
            {
                // Procenta mezi 16% a highVolumeThreshold se mapují plynule z 10 na 2
                double x1 = 16; // počáteční procento
                double y1 = 10; // počáteční násobič
                double x2 = highVolumeThreshold; // koncové procento
                double y2 = 2; // koncový násobič

                double newValue = (percent - x1) * (y2 - y1) / (x2 - x1) + y1;
                return (int)newValue;
            }
            else if (percent > 8)
            {
                // Procenta mezi 16% a 8% použijeme násobič 10
                return 10;
            }
            else if (percent > 6)
            {
                // Procenta mezi 8% a 6% použijeme násobič 30
                return 30;
            }
            else if (percent > 4)
            {
                // Procenta mezi 6% a 4% použijeme násobič 50
                return 50;
            }
            else if (percent > 0)
            {
                // Procenta mezi 4% a 0% použijeme násobič 100
                return 100;
            }
            else
            {
                // Výchozí hodnota, pokud percento je mimo rozsah 0-100
                return 1;
            }
        }


        /// <inheritdoc/>
        public void Start()
        {
            IsStarted = true;
        }

        /// <inheritdoc/>
        public void Stop()
        {
            LevelLeft = LevelRight = 0;
            IsStarted = false;
        }
    }
}
