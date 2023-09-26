using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using WASApiBassNet.Components.AudioCapture;
using WASApiBassNet.Components.FFT;

namespace WindowsAudioSession.UI.FFT
{
    /// <summary>
    /// fft peak drawer
    /// </summary>
    public class FFTPeakDrawer : IFFTPeakDrawer, IAudioPlugin
    {
        /// <inheritdoc/>
        public IFFTPeakAnalyzer FFTPeakAnalyser { get; set; }

        /// <inheritdoc/>
        public IDrawable Drawable { get; set; }

        /// <inheritdoc/>
        public double Margin { get; set; } = 8;

        /// <inheritdoc/>
        public double WidthPercent { get; set; } = 100;

        Rectangle[] _bars;

        /// <inheritdoc/>
        public Brush BarBrush { get; set; }
            = CustomBrushes.VolumePeakTopBrush;

        /// <inheritdoc/>
        public double PeakBarHeight { get; set; } = 1d;

        /// <inheritdoc/>
        public bool IsStarted { get; protected set; }

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
            var showingBarCount = FFTPeakAnalyser.ShowingBarsCount;
            showingBarCount = (showingBarCount > 0 && (showingBarCount % 2) == 0) ? showingBarCount : barCount;
            var showingBarRatio = (showingBarCount > 0) ? barCount / showingBarCount : 1;
            var barMaxWidth = (width - (2d * Margin)) / showingBarCount;
            var barWidth = barMaxWidth * WidthPercent / 100d;

            if (_bars == null)
            {
                _bars = new Rectangle[showingBarCount];
                for (var i = 0; i < showingBarCount; i++)
                {
                    var bar = new Rectangle();
                    _bars[i] = bar;
                    bar.Fill = BarBrush;
                    _ = canvas.Children.Add(bar);
                }
            }

            var x = x0;

            for (var i = 0; i < showingBarCount; i++)
            {
                double maxValue;

                if (showingBarRatio > 1)
                {
                    // Pokud je rozsah více než 1 sloupec, provede se zprůměrování

                    // Určení začátku a konce rozsahu sloupců, které chcete zprůměrovat
                    int startIndex = i * showingBarRatio;
                    int endIndex = (i + 1) * showingBarRatio;

                    // Inicializace proměnné pro maximální hodnotu
                    maxValue = barSizes.Skip(startIndex).Take(endIndex - startIndex).Max();
                }
                else
                {
                    // Pokud je rozsah 1 sloupec, použije se přímo hodnota tohoto sloupce
                    maxValue = barSizes[i];
                }


                var barHeight = Math.Max(0, maxValue * (height - 2 * Margin) / 255d) * ((showingBarRatio > 1) ? 2.15 : 1);
                var y_top = (y0 + height - 2 * Margin - barHeight);

                var bar = _bars[i];

                Canvas.SetLeft(bar, x);

                bar.Width = Math.Ceiling(barWidth * WidthPercent / 100d);

                Canvas.SetTop(bar, y_top);
                bar.Height = PeakBarHeight;

                x += barMaxWidth;
            }
        }

        void ResetBars()
        {
            Drawable.GetDrawingSurface().Children.Clear();
            _bars = null;
        }

        /// <inheritdoc/>
        public void HandleTick()
        {
            if (FFTPeakAnalyser == null || !IsStarted) return;

            try
            {
                var x0 = Margin;
                var y0 = Margin;
                var canvas = Drawable.GetDrawingSurface();
                var width = canvas.ActualWidth;
                var height = canvas.ActualHeight;

                Draw(x0, y0, width, height, FFTPeakAnalyser.SpectrumPeakData);
            }
            catch (Exception ex)
            {
                Stop();
                UIHelper.ShowError(ex);
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
