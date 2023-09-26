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
            int volumeModifier = MapPercentToNumber(Convert.ToInt32(audioController.Volume));
            return level * volumeModifier;
        }

        int MapPercentToNumber(int percent)
        {
            int highVolumeThreshold = AppHelper.GetHighVolumeThreshold(26);

            if (percent <= 100 && percent > highVolumeThreshold) return 1;
            else if (percent > 10 && percent <= highVolumeThreshold)
            {
                // Procenta mezi 10% a highVolumeThreshold se mapují plynule z 10 na 1
                double x1 = 10; // počáteční procento
                double y1 = 10; // počáteční násobič
                double x2 = highVolumeThreshold; // koncové procento
                double y2 = 2; // koncový násobič

                double newValue = (percent - x1) * (y2 - y1) / (x2 - x1) + y1;
                return (int)newValue;
            }
            else if (percent <= 10 && percent > 0) return 10;
            else return 0;
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
