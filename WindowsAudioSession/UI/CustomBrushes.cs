using System;
using System.Windows;
using System.Windows.Media;
using WindowsAudioSession.Properties;
using WPFUtilities.CustomBrushes;

namespace WindowsAudioSession.UI
{
    public static class CustomBrushes
    {
        public static Brush VolumePeakBrush
        {
            get
            {
                System.Drawing.Color drawingColor = (Settings.Default.ColorFreqPeakMeter.IsEmpty) ? System.Drawing.Color.Red : Settings.Default.ColorFreqPeakMeter;
                Color brushColor = Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
                return HatchRawBrush.Create(new SolidColorBrush(brushColor), 4, 3);
            }
        }

        public static Brush VolumePeakTopBrush
        {
            get
            {
                System.Drawing.Color drawingColor = (Settings.Default.ColorFreqPeakMeter.IsEmpty) ? System.Drawing.Color.Red : Settings.Default.ColorFreqPeakMeter;
                Color brushColor = ChangeColorBrightness(Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B), (float)-0.455);
                return new SolidColorBrush(brushColor);
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
                System.Drawing.Color drawingColor = (Settings.Default.ColorSoundWave.IsEmpty) ? System.Drawing.Color.Red : Settings.Default.ColorSoundWave;
                Color brushColor = Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
                return new SolidColorBrush(brushColor);
            }
        }

        public static Brush InactiveLabels
        {
            get
            {
                System.Drawing.Color drawingColor = (Settings.Default.ColorLabelsActive.IsEmpty) ? System.Drawing.Color.Gray : Settings.Default.ColorLabelsActive;
                Color brushColor = ChangeColorBrightness(Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B), (float)-0.455);
                return new SolidColorBrush(brushColor);
            }
        }

        public static Brush Labels
        {
            get
            {
                System.Drawing.Color drawingColor = (Settings.Default.ColorLabelsActive.IsEmpty) ? System.Drawing.Color.White : Settings.Default.ColorLabelsActive;
                Color brushColor = Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
                return new SolidColorBrush(brushColor);
            }
        }

        public static Brush LabelChanging
        {
            get
            {
                System.Drawing.Color drawingColor = (Settings.Default.ColorLabelsChanging.IsEmpty) ? System.Drawing.Color.YellowGreen : Settings.Default.ColorLabelsChanging;
                Color brushColor = Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
                return new SolidColorBrush(brushColor);
            }
        }

        public static Brush HeaderLabels
        {
            get
            {
                System.Drawing.Color drawingColor = (Settings.Default.ColorLabelsHeaders.IsEmpty) ? System.Drawing.Color.White : Settings.Default.ColorLabelsHeaders;
                Color brushColor = Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
                return new SolidColorBrush(brushColor);
            }
        }

        public static Brush Background
        {
            get
            {
                System.Drawing.Color drawingColor = (Settings.Default.ColorBackground.IsEmpty) ? System.Drawing.Color.Black : Settings.Default.ColorBackground;
                Color brushColor = Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
                return new SolidColorBrush(brushColor);
            }
        }

        public static Brush Borders
        {
            get
            {
                System.Drawing.Color drawingColor = (Settings.Default.ColorBorders.IsEmpty) ? System.Drawing.Color.FromArgb(0xFF, 0x1D, 0x1D, 0x1D) : Settings.Default.ColorBorders;
                Color brushColor = Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
                return new SolidColorBrush(brushColor);
            }
        }

        public static Brush LoadingScreen
        {
            get
            {
                System.Drawing.Color drawingColor = (Settings.Default.ColorBorders.IsEmpty) ? System.Drawing.Color.FromArgb(0xCC, 0x1D, 0x1D, 0x1D) : Settings.Default.ColorBorders;
                Color brushColor = Color.FromArgb(0xCC, drawingColor.R, drawingColor.G, drawingColor.B);
                return new SolidColorBrush(brushColor);
            }
        }

        public static Brush LabelsHigh
        {
            get
            {
                System.Drawing.Color drawingColor = (Settings.Default.ColorLabelsHigh.IsEmpty) ? System.Drawing.Color.Red : Settings.Default.ColorLabelsHigh;
                Color brushColor = Color.FromArgb(0xCC, drawingColor.R, drawingColor.G, drawingColor.B);
                return new SolidColorBrush(brushColor);
            }
        }

        public static LinearGradientBrush VuMeterBrush
        {
            get
            {
                System.Drawing.Color labelsActiveDrawingColor = (Settings.Default.ColorLabelsActive.IsEmpty) ? System.Drawing.Color.White : Settings.Default.ColorLabelsActive;
                Color labelsActiveColor = Color.FromArgb(labelsActiveDrawingColor.A, labelsActiveDrawingColor.R, labelsActiveDrawingColor.G, labelsActiveDrawingColor.B);
                System.Drawing.Color labelsHighDrawingColor = (Settings.Default.ColorLabelsHigh.IsEmpty) ? System.Drawing.Color.Red : Settings.Default.ColorLabelsHigh;
                Color labelsHighColor = Color.FromArgb(labelsHighDrawingColor.A, labelsHighDrawingColor.R, labelsHighDrawingColor.G, labelsHighDrawingColor.B);
                LinearGradientBrush brush = new LinearGradientBrush();
                brush.GradientStops.Add(new GradientStop(labelsActiveColor, 0));
                brush.GradientStops.Add(new GradientStop(labelsActiveColor, 0.8));
                brush.GradientStops.Add(new GradientStop(labelsHighColor, 0.8));
                brush.GradientStops.Add(new GradientStop(labelsHighColor, 1));

                return brush;
            }
        }

        private static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(color.A, Convert.ToByte(red), Convert.ToByte(green), Convert.ToByte(blue));
        }
    }
}
