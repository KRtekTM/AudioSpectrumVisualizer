﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using WASApiBassNet;
using WASApiBassNet.Components.AudioCapture;
using WASApiBassNet.Components.FFT;

using WPFUtilities.CustomBrushes;

namespace WindowsAudioSession.UI.FFT
{
    /// <summary>
    /// fft drawer
    /// </summary>
    public class FFTDrawer : IFFTDrawer, IAudioPlugin
    {
        /// <inheritdoc/>
        public IDrawable Drawable { get; set; }

        /// <inheritdoc/>
        public IFFTAnalyzer FFTAnalyser { get; set; }

        /// <inheritdoc/>
        public double Margin { get; set; } = 8;

        /// <inheritdoc/>
        public double BarWidthPercent { get; set; } = 100;

        Rectangle[] _bars;

        /// <inheritdoc/>
        public Brush BarBrush { get; set; } = CustomBrushes.CurveBrushWhite;//SpectrumBrush.Create();

        /// <inheritdoc/>
        public bool IsStarted { get; protected set; }

        private List<KeyValuePair<int, int>> FreqBarIDs;

        public int ShowingBarsCount { get; set; }

        void Draw(
            double x0,
            double y0,
            double width,
            double height,
            double[] barSizes
            )
        {
            var canvas = Drawable.GetDrawingSurface();
            var barCount = barSizes.Length;
            var showingBarCount = ShowingBarsCount;
            showingBarCount = (showingBarCount > 0 && (showingBarCount % 2) == 0) ? showingBarCount : barCount;
            var showingBarRatio = (showingBarCount > 0) ? barCount / showingBarCount : 1;
            var barMaxWidth = (width - (2d * Margin)) / showingBarCount;
            var barWidth = barMaxWidth * BarWidthPercent / 100d;

            if (_bars == null)
            {
                _bars = new Rectangle[showingBarCount];
                for (var i = 0; i < showingBarCount; i++)
                {
                    var bar = new Rectangle()
                    {
                        Fill = BarBrush
                    };
                    _bars[i] = bar;
                    _ = canvas.Children.Add(bar);
                }
            }

            var x = x0;

            for (var i = 0; i < showingBarCount; i++)
            {
                double maxValue;

                if (showingBarCount == 16)
                {
                    // Určení začátku a konce rozsahu sloupců, které chcete zprůměrovat
                    //int startIndex = i * showingBarRatio;
                    //int endIndex = (i + 1) * showingBarRatio;
                    if (FreqBarIDs == null)
                    {
                        FreqBarIDs = FFTHelper.Frequencies(FFTAnalyser.BarsCount);
                    }

                    int startIndex = FreqBarIDs[i].Key;
                    int endIndex = FreqBarIDs[i].Value;

                    // Inicializace proměnné pro maximální hodnotu
                    maxValue = barSizes.Skip(startIndex).Take(endIndex - startIndex).Max();
                }
                else if (showingBarRatio > 1)
                {
                    int startIndex = i * showingBarRatio;
                    int endIndex = (i + 1) * showingBarRatio;

                    maxValue = barSizes.Skip(startIndex).Take(endIndex - startIndex).Max();
                }
                else
                {
                    // Pokud je rozsah 1 sloupec, použije se přímo hodnota tohoto sloupce
                    maxValue = barSizes[i];
                }


                var barHeight = Math.Max(0, maxValue * (height - 2 * Margin) / 255d) * ((showingBarCount <= 16) ? 1.5 : 1);
                barHeight = Math.Min((showingBarCount <= 16) ? 64 : (Drawable.BarHeight() - ((canvas.Margin.Top + canvas.Margin.Bottom) * 2)), barHeight);
                var y_top = y0 + height - 2 * Margin - barHeight;

                var bar = _bars[i];

                Canvas.SetLeft(bar, x);
                //Canvas.SetLeft(_bars[i], Math.Ceiling(x));

                //bar.Width = barWidth * WidthPercent / 100d;
                bar.Width = Math.Ceiling(barWidth * BarWidthPercent / 100d);

                Canvas.SetTop(bar, y_top);
                bar.Height = barHeight;

                x += barMaxWidth;
            }
        }

        void ResetBars()
        {
            var canvas = Drawable.GetDrawingSurface();
            if (_bars != null)
            {
                foreach (var bar in _bars)
                    canvas.Children.Remove(bar);
            }
            _bars = null;
        }

        /// <inheritdoc/>
        public void HandleTick()
        {
            if (FFTAnalyser == null || !IsStarted) return;

            try
            {
                var canvas = Drawable.GetDrawingSurface();
                var x0 = Margin;
                var y0 = Margin;
                var width = canvas.ActualWidth;
                var height = canvas.ActualHeight;

                Draw(x0, y0, width, height, FFTAnalyser.SpectrumData);
            }
            catch (Exception ex)
            {
                Stop();
                UIHelper.ShowError(
                    ExceptionHelper.BuildException(ex));
            }
        }

        /// <inheritdoc/>
        public void Start()
        {
            ResetBars();
            IsStarted = true;
        }

        /// <inheritdoc/>
        public void Stop()
        {
            ResetBars();
            IsStarted = false;
        }

    }
}
