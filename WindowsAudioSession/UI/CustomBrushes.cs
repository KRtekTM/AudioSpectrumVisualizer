using System.Windows;
using System.Windows.Media;
using WPFUtilities.CustomBrushes;

namespace WindowsAudioSession.UI
{
    public static class CustomBrushes
    {
        public static Brush VolumePeakBrush
        {
            get
            {
                return HatchRawBrush.Create(Brushes.Red, 4, 3);
            }
        }

        public static Brush VolumePeakTopBrush
        {
            get
            {
                return Brushes.DarkRed;
            }
        }

        public static LinearGradientBrush CurveBrushWhite
        {
            get
            {
                LinearGradientBrush brush = new LinearGradientBrush();
                brush.StartPoint = new Point(0, 0);
                brush.EndPoint = new Point(0, 1);

                brush.GradientStops.Add(new GradientStop(Colors.DarkRed, 0));
                brush.GradientStops.Add(new GradientStop(Colors.DarkRed, 0.1));
                brush.GradientStops.Add(new GradientStop(Colors.YellowGreen, 0.1));
                brush.GradientStops.Add(new GradientStop(Colors.DarkGreen, 0.8));
                brush.GradientStops.Add(new GradientStop(Colors.Black, 1));

                return brush;
            }
        }

        public static Brush SoundWaveBrush
        {
            get
            {
                return Brushes.Red;
            }
        }

        public static Brush InactiveLabels
        {
            get
            {
                return Brushes.Gray;
            }
        }

        public static Brush Labels
        {
            get
            {
                return Brushes.White;
            }
        }

        public static Brush LabelChanging
        {
            get
            {
                return Brushes.YellowGreen;
            }
        }

        public static Brush HeaderLabels
        {
            get
            {
                return Brushes.White;
            }
        }

        public static Brush Background
        {
            get
            {
                return Brushes.Black;
            }
        }

        public static Brush Borders
        {
            get
            {
                return new SolidColorBrush(Color.FromArgb(0xFF, 0x1D, 0x1D, 0x1D));
            }
        }

        public static Brush LoadingScreen
        {
            get
            {
                return new SolidColorBrush(Color.FromArgb(0xCC, 0x1D, 0x1D, 0x1D));
            }
        }
    }
}
