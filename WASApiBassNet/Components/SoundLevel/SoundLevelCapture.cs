﻿using System;
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
            if (percent <= 100 && percent > 26) return 1;
            else if (percent > 10 && percent <= 26)
            {
                // Procenta mezi 10% a 26% se mapují plynule z 10 na 1
                return 10 - (int)((percent - 10) * 0.5);
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
