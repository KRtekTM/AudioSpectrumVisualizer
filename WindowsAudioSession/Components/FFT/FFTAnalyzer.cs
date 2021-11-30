﻿using System;

using Un4seen.BassWasapi;

using WindowsAudioSession.Components.AudioCapture;

namespace WindowsAudioSession.Components.FFT
{
    public class FFTAnalyzer : ISoundCaptureHandler
    {
        readonly float[] _fft;
        readonly SampleLength _sampleLength;
        readonly int _linesCount;

        public double[] Spectrumdata;

        public FFTAnalyzer(
            SampleLength sampleLength,
            int linesCount)
        {
            _linesCount = linesCount;
            Spectrumdata = new double[_linesCount];
            _sampleLength = sampleLength;
            _fft = new float[sampleLength.ToBufferSize()];
        }

        public void HandleTick()
        {
            var ret = BassWasapi.BASS_WASAPI_GetData(
                _fft,
                (int)_sampleLength.ToBassData());

            if (ret < -1) return;
            int x;
            double y;
            var b0 = 0;
            var _bufferLastIndex = _sampleLength.ToBufferSize() - 1;

            //computes the spectrum data, the code is taken from a bass_wasapi sample.
            for (x = 0; x < _linesCount; x++)
            {
                double peak = 0;
                var b1 = (int)Math.Pow(2, x * 10.0 / (_linesCount - 1));
                if (b1 > _bufferLastIndex) b1 = _bufferLastIndex;
                if (b1 <= b0) b1 = b0 + 1;
                for (; b0 < b1; b0++)
                {
                    if (peak < _fft[1 + b0]) peak = _fft[1 + b0];
                }
                y = (Math.Sqrt(peak) * 3 * 255) - 4;
                if (y > 255) y = 255;
                if (y < 0) y = 0;

                Spectrumdata[x] = y;

                //Console.Write("{0, 3} ", y);
            }

            //if (DisplayEnable) _spectrum.Set(_spectrumdata);
            //for (x = 0; x < _linesCount; x++) _spectrumdata[x] = 0;

            // level sound capture handler
            /*int level = BassWasapi.BASS_WASAPI_GetLevel();
            _l.Value = Utils.LowWord32(level);
            _r.Value = Utils.HighWord32(level);
            if (level == _lastlevel && level != 0) _hanctr++;
            _lastlevel = level;
            */

            //Required, because some programs hang the output. If the output hangs for a 75ms
            //this piece of code re initializes the output so it doesn't make a gliched sound for long.
            /*
            if (_hanctr > 3)
            {
                _hanctr = 0;
                _l.Value = 0;
                _r.Value = 0;
                Free();
                Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
                _initialized = false;
                Enable = true;
            }
            */
        }

        public void Start()
        {

        }

        public void Stop()
        {
            for (var x = 0; x < _linesCount; x++) Spectrumdata[x] = 0;
        }
    }
}
