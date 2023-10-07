using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsAudioSession.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using System.Runtime.Remoting.Contexts;
using System.Windows;
using System.Windows.Controls;
using Button = System.Windows.Forms.Button;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace WindowsAudioSession.UI
{
    public partial class StyleSettings : Form
    {
        public StyleSettings()
        {
            InitializeComponent();

            BoxDayDb.Value = Settings.Default.DayDecibelThreshold;
            BoxNightDb.Value = Settings.Default.NightDecibelThreshold;
            BoxMorningHour.Value = Settings.Default.MorningHour;
            BoxNightHour.Value = Settings.Default.NightHour;
            BoxVolumeStep.Value = Settings.Default.VolumeStep;
            ChckBoxSourceTitleAsChanging.Checked = Settings.Default.SourceTitleAsChanging;
        }

        StyleJson currentTheme = new StyleJson()
        {
            LabelsActive = Settings.Default.ColorLabelsActive,
            LabelsHeaders = Settings.Default.ColorLabelsHeaders,
            LabelsChanging = Settings.Default.ColorLabelsChanging,
            LabelsHigh = Settings.Default.ColorLabelsHigh,

            VolumePeakMeterLow = Settings.Default.ColorVolumePeakMeterLow,
            VolumePeakMeterHigh = Settings.Default.ColorVolumePeakMeterHigh,
            SoundWave = Settings.Default.ColorSoundWave,
            FreqPeakMeter = Settings.Default.ColorFreqPeakMeter,
            SpectrumAnalyser = Settings.Default.ColorSpectrumAnalyser,
            SpectrumAnalyserHighPeak = Settings.Default.ColorSpectrumAnalyserHighPeaks,

            Borders = Settings.Default.ColorBorders,
            Background = Settings.Default.ColorBackground,

            FontVFD = Settings.Default.FontVFD,
            FontHeaders = Settings.Default.FontHeaders,
            FontNumeric = Settings.Default.FontNumeric,
            FontHeadersSize = Settings.Default.FontHeadersSize,
            FontVFDSize = Settings.Default.FontVFDSize


        };

        StyleJson editingTheme = new StyleJson()
        {
            LabelsActive = Settings.Default.ColorLabelsActive,
            LabelsHeaders = Settings.Default.ColorLabelsHeaders,
            LabelsChanging = Settings.Default.ColorLabelsChanging,
            LabelsHigh = Settings.Default.ColorLabelsHigh,

            VolumePeakMeterLow = Settings.Default.ColorVolumePeakMeterLow,
            VolumePeakMeterHigh = Settings.Default.ColorVolumePeakMeterHigh,
            SoundWave = Settings.Default.ColorSoundWave,
            FreqPeakMeter = Settings.Default.ColorFreqPeakMeter,
            SpectrumAnalyser = Settings.Default.ColorSpectrumAnalyser,
            SpectrumAnalyserHighPeak = Settings.Default.ColorSpectrumAnalyserHighPeaks,

            Borders = Settings.Default.ColorBorders,
            Background = Settings.Default.ColorBackground,

            FontVFD = Settings.Default.FontVFD,
            FontHeaders = Settings.Default.FontHeaders,
            FontNumeric = Settings.Default.FontNumeric,
            FontHeadersSize = Settings.Default.FontHeadersSize,
            FontVFDSize = Settings.Default.FontVFDSize
        };

        private void StyleSettings_Load(object sender, EventArgs e)
        {
            BtnLabelsActiveColor.BackColor = currentTheme.LabelsActive;
            BtnLabelsHeadersColor.BackColor = currentTheme.LabelsHeaders;
            BtnLabelsChangingColor.BackColor = currentTheme.LabelsChanging;
            BtnLabelsHighColor.BackColor = currentTheme.LabelsHigh;

            BtnColorVolPeakLow.BackColor = currentTheme.VolumePeakMeterLow;
            BtnColorVolPeakHigh.BackColor = currentTheme.VolumePeakMeterHigh;
            BtnSoundWaveColor.BackColor = currentTheme.SoundWave;
            BtnFreqPeakMeterColor.BackColor = currentTheme.FreqPeakMeter;
            BtnColorSpectrumAnalyser.BackColor = currentTheme.SpectrumAnalyser;
            BtnColorSpectrumAnalyserHighPeaks.BackColor = currentTheme.SpectrumAnalyserHighPeak;

            BtnBordersColor.BackColor = currentTheme.Borders;
            BtnBackgroundColor.BackColor = currentTheme.Background;

            BtnFontVFD.Text = $"{currentTheme.FontVFD} ({currentTheme.FontVFDSize}pt)";
            BtnFontHeaders.Text = $"{currentTheme.FontHeaders} ({currentTheme.FontHeadersSize}pt)";
            BtnFontNumeric.Text = $"{currentTheme.FontNumeric}";
        }

        private void UserColorInput(Button button, out Color color)
        {
            ColorPickerDialog.Color = button.BackColor;
            color = button.BackColor;

            if (ColorPickerDialog.ShowDialog() == DialogResult.OK)
            {
                button.BackColor = ColorPickerDialog.Color;
                color = ColorPickerDialog.Color;
            }
        }

        private void UserResetColor(Button button, Color color)
        {
            button.BackColor = color;
        }

        private void UserFontInput(Button button, FontType fontFamilySettingsName, bool fontSize = false)
        {
            if(FontPickerDialog.ShowDialog() == DialogResult.OK)
            {
                UserFontReset(button, fontFamilySettingsName, $"{FontPickerDialog.Font.FontFamily}", fontSize ? FontPickerDialog.Font.SizeInPoints : 0);
            }
        }

        private void UserFontReset(Button button, FontType fontFamilySettingsName, string fontFamily, double fontSize = 0)
        {
            button.Text = $"{fontFamily} {(fontSize > 1 ? $"({fontSize}pt)" : "")}";

            switch(fontFamilySettingsName)
            {
                case FontType.VFD:
                    editingTheme.FontVFD = fontFamily;
                    editingTheme.FontVFDSize = fontSize;
                    break;
                case FontType.Headers:
                    editingTheme.FontHeaders = fontFamily;
                    editingTheme.FontHeadersSize = fontSize;
                    break;
                case FontType.Numeric:
                    editingTheme.FontNumeric = fontFamily;
                    break;
            }
        }

        private void BtnLabelsActiveColor_Click(object sender, EventArgs e)
        {
            Color temp;
            UserColorInput(BtnLabelsActiveColor, out temp);
            editingTheme.LabelsActive = temp;
        }

        private void BtnLabelsHeadersColor_Click(object sender, EventArgs e)
        {
            Color temp;
            UserColorInput(BtnLabelsHeadersColor, out temp);
            editingTheme.LabelsHeaders = temp;
        }

        private void BtnLabelsChangingColor_Click(object sender, EventArgs e)
        {
            Color temp;
            UserColorInput(BtnLabelsChangingColor, out temp);
            editingTheme.LabelsChanging = temp;
        }

        private void BtnLabelsHighColor_Click(object sender, EventArgs e)
        {
            Color temp;
            UserColorInput(BtnLabelsHighColor, out temp);
            editingTheme.LabelsHigh = temp;
        }

        private void BtnSoundWaveColor_Click(object sender, EventArgs e)
        {
            Color temp;
            UserColorInput(BtnSoundWaveColor, out temp);
            editingTheme.SoundWave = temp;
        }

        private void BtnFreqPeakMeterColor_Click(object sender, EventArgs e)
        {
            Color temp;
            UserColorInput(BtnFreqPeakMeterColor, out temp);
            editingTheme.FreqPeakMeter = temp;
        }

        private void BtnBordersColor_Click(object sender, EventArgs e)
        {
            Color temp;
            UserColorInput(BtnBordersColor, out temp);
            editingTheme.Borders = temp;
        }

        private void BtnBackgroundColor_Click(object sender, EventArgs e)
        {
            Color temp;
            UserColorInput(BtnBackgroundColor, out temp);
            editingTheme.Background = temp;
        }

        private void BtnColorVolPeakLow_Click(object sender, EventArgs e)
        {
            Color temp;
            UserColorInput(BtnColorVolPeakLow, out temp);
            editingTheme.VolumePeakMeterLow = temp;
        }

        private void BtnColorSpectrumAnalyser_Click(object sender, EventArgs e)
        {
            Color temp;
            UserColorInput(BtnColorSpectrumAnalyser, out temp);
            editingTheme.SpectrumAnalyser = temp;
        }

        private void BtnColorVolPeakHigh_Click(object sender, EventArgs e)
        {
            Color temp;
            UserColorInput(BtnColorVolPeakHigh, out temp);
            editingTheme.VolumePeakMeterHigh = temp;
        }

        private void BtnColorSpectrumAnalyserHighPeaks_Click(object sender, EventArgs e)
        {
            Color temp;
            UserColorInput(BtnColorSpectrumAnalyserHighPeaks, out temp);
            editingTheme.SpectrumAnalyserHighPeak = temp;
        }

        private void BtnResetColorVolPeakHigh_Click(object sender, EventArgs e)
        {
            UserResetColor(BtnColorVolPeakHigh, currentTheme.VolumePeakMeterHigh);
            editingTheme.VolumePeakMeterHigh = currentTheme.VolumePeakMeterHigh;
        }
        private void BtnResetColorAnalyserHighPeaks_Click(object sender, EventArgs e)
        {
            UserResetColor(BtnColorSpectrumAnalyserHighPeaks, currentTheme.SpectrumAnalyserHighPeak);
            editingTheme.SpectrumAnalyserHighPeak = currentTheme.SpectrumAnalyserHighPeak;
        }

        private void BtnResetColorSpectrumAnalyser_Click(object sender, EventArgs e)
        {
            UserResetColor(BtnColorSpectrumAnalyser, currentTheme.SpectrumAnalyser);
            editingTheme.SpectrumAnalyser = currentTheme.SpectrumAnalyser;
        }

        private void BtnResetColorVolPeakLow_Click(object sender, EventArgs e)
        {
            UserResetColor(BtnColorVolPeakLow, currentTheme.VolumePeakMeterLow);
            editingTheme.VolumePeakMeterLow = currentTheme.VolumePeakMeterLow;
        }

        private void BtnResetLabelsActiveColor_Click(object sender, EventArgs e)
        {
            UserResetColor(BtnLabelsActiveColor, currentTheme.LabelsActive);
            editingTheme.LabelsActive = currentTheme.LabelsActive;
        }

        private void BtnResetLabelsHeadersColor_Click(object sender, EventArgs e)
        {
            UserResetColor(BtnLabelsHeadersColor, currentTheme.LabelsHeaders);
            editingTheme.LabelsHeaders = currentTheme.LabelsHeaders;
        }

        private void BtnResetLabelsChangingColor_Click(object sender, EventArgs e)
        {
            UserResetColor(BtnLabelsChangingColor, currentTheme.LabelsChanging);
            editingTheme.LabelsChanging = currentTheme.LabelsChanging;
        }

        private void BtnResetLabelsHighColor_Click(object sender, EventArgs e)
        {
            UserResetColor(BtnLabelsHighColor, currentTheme.LabelsHigh);
            editingTheme.LabelsHigh = currentTheme.LabelsHigh;
        }

        private void BtnResetSoundWaveColor_Click(object sender, EventArgs e)
        {
            UserResetColor(BtnSoundWaveColor, currentTheme.SoundWave);
            editingTheme.SoundWave = currentTheme.SoundWave;
        }

        private void BtnResetFreqPeakMeterColor_Click(object sender, EventArgs e)
        {
            UserResetColor(BtnFreqPeakMeterColor, currentTheme.FreqPeakMeter);
            editingTheme.FreqPeakMeter = currentTheme.FreqPeakMeter;
        }

        private void BtnResetBordersColor_Click(object sender, EventArgs e)
        {
            UserResetColor(BtnBordersColor, currentTheme.Borders);
            editingTheme.Borders = currentTheme.Borders;
        }

        private void BtnResetBackgroundColor_Click(object sender, EventArgs e)
        {
            UserResetColor(BtnBackgroundColor, currentTheme.Background);
            editingTheme.Background = currentTheme.Background;
        }

        private void BtnResultOK_Click(object sender, EventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("Applying theme changes will restart the application.", "Apply Theme", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
            {
                Settings.Default.ColorLabelsActive = editingTheme.LabelsActive;
                Settings.Default.ColorLabelsHeaders = editingTheme.LabelsHeaders;
                Settings.Default.ColorLabelsChanging = editingTheme.LabelsChanging;
                Settings.Default.ColorLabelsHigh = editingTheme.LabelsHigh;

                Settings.Default.ColorVolumePeakMeterLow = editingTheme.VolumePeakMeterLow;
                Settings.Default.ColorVolumePeakMeterHigh = editingTheme.VolumePeakMeterHigh;
                Settings.Default.ColorSoundWave = editingTheme.SoundWave;
                Settings.Default.ColorFreqPeakMeter = editingTheme.FreqPeakMeter;
                Settings.Default.ColorSpectrumAnalyser = editingTheme.SpectrumAnalyser;
                Settings.Default.ColorSpectrumAnalyserHighPeaks = editingTheme.SpectrumAnalyserHighPeak;

                Settings.Default.ColorBorders = editingTheme.Borders;
                Settings.Default.ColorBackground = editingTheme.Background;

                Settings.Default.FontVFD = editingTheme.FontVFD;
                Settings.Default.FontHeaders = editingTheme.FontHeaders;
                Settings.Default.FontNumeric = editingTheme.FontNumeric;
                Settings.Default.FontHeadersSize = editingTheme.FontHeadersSize;
                Settings.Default.FontVFDSize = editingTheme.FontVFDSize;

                Settings.Default.Save();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void BtnResultCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void BtnFontNumeric_Click(object sender, EventArgs e)
        {
            UserFontInput(BtnFontNumeric, FontType.Numeric);
        }

        private void BtnFontHeaders_Click(object sender, EventArgs e)
        {
            UserFontInput(BtnFontHeaders, FontType.Headers, true);
        }

        private void BtnFontVFD_Click(object sender, EventArgs e)
        {
            UserFontInput(BtnFontVFD, FontType.VFD, true);
        }

        private void BtnFontHeadersReset_Click(object sender, EventArgs e)
        {
            UserFontReset(BtnFontHeaders, FontType.Headers, currentTheme.FontHeaders, currentTheme.FontHeadersSize);
        }

        private void BtnFontNumericReset_Click(object sender, EventArgs e)
        {
            UserFontReset(BtnFontNumeric, FontType.Numeric, currentTheme.FontNumeric);
        }

        private void BtnFontVFDReset_Click(object sender, EventArgs e)
        {
            UserFontReset(BtnFontVFD, FontType.VFD, currentTheme.FontVFD, currentTheme.FontVFDSize);
        }

        private enum FontType
        {
            VFD,
            Headers,
            Numeric
        }

        private struct StyleJson
        {
            public Color LabelsActive { get; set; }
            public Color LabelsHeaders { get; set; }
            public Color LabelsChanging { get; set; }
            public Color LabelsHigh { get; set; }

            public Color VolumePeakMeterLow { get; set; }
            public Color VolumePeakMeterHigh { get; set; }
            public Color SoundWave { get; set; }
            public Color FreqPeakMeter { get; set; }
            public Color SpectrumAnalyser { get; set; }
            public Color SpectrumAnalyserHighPeak { get; set; }

            public Color Borders { get; set; }
            public Color Background { get; set; }

            public string FontVFD { get; set; }
            public double FontVFDSize { get; set; }
            public string FontHeaders { get; set; }
            public double FontHeadersSize { get; set; }
            public string FontNumeric { get; set; }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            JObject jsonStyle = JObject.FromObject(editingTheme);

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(saveFileDialog1.FileName, jsonStyle.ToString());
            }
        }


        private void BtnImport_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string jsonText = System.IO.File.ReadAllText(openFileDialog1.FileName);
                    StyleJson style = JsonConvert.DeserializeObject<StyleJson>(jsonText);

                    editingTheme.LabelsActive = style.LabelsActive;
                    BtnLabelsActiveColor.BackColor = style.LabelsActive;
                    editingTheme.LabelsHeaders = style.LabelsHeaders;
                    BtnLabelsHeadersColor.BackColor = style.LabelsHeaders;
                    editingTheme.LabelsChanging = style.LabelsChanging;
                    BtnLabelsChangingColor.BackColor = style.LabelsChanging;
                    editingTheme.LabelsHigh = style.LabelsHigh;
                    BtnLabelsHighColor.BackColor = style.LabelsHigh;

                    editingTheme.VolumePeakMeterLow = style.VolumePeakMeterLow;
                    BtnColorVolPeakLow.BackColor = style.VolumePeakMeterLow;
                    editingTheme.VolumePeakMeterHigh = style.VolumePeakMeterHigh;
                    BtnColorVolPeakHigh.BackColor = style.VolumePeakMeterHigh;
                    editingTheme.SoundWave = style.SoundWave;
                    BtnSoundWaveColor.BackColor = style.SoundWave;
                    editingTheme.FreqPeakMeter = style.FreqPeakMeter;
                    BtnFreqPeakMeterColor.BackColor = style.FreqPeakMeter;
                    editingTheme.SpectrumAnalyser = style.SpectrumAnalyser;
                    BtnColorSpectrumAnalyser.BackColor = style.SpectrumAnalyser;
                    editingTheme.SpectrumAnalyserHighPeak = style.SpectrumAnalyserHighPeak;
                    BtnColorSpectrumAnalyserHighPeaks.BackColor = style.SpectrumAnalyserHighPeak;

                    editingTheme.Borders = style.Borders;
                    BtnBordersColor.BackColor = style.Borders;
                    editingTheme.Background = style.Background;
                    BtnBackgroundColor.BackColor = style.Background;

                    editingTheme.FontVFD = style.FontVFD;
                    editingTheme.FontVFDSize = style.FontVFDSize;
                    BtnFontVFD.Text = $"{style.FontVFD} ({style.FontVFDSize}pt)";
                    editingTheme.FontHeaders = style.FontHeaders;
                    editingTheme.FontHeadersSize = style.FontHeadersSize;
                    BtnFontHeaders.Text = $"{style.FontHeaders} ({style.FontHeadersSize}pt)";
                    editingTheme.FontNumeric = style.FontNumeric;
                    BtnFontNumeric.Text = $"{style.FontNumeric}";
                }
                catch(Exception ex) {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    System.Windows.Forms.MessageBox.Show($"Theme file was loaded partially or not at all.{Environment.NewLine}Please check the file syntax.", "Theme file corrupted", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (sender.Equals(BtnResetDayDb)) BoxDayDb.Value = 75;
            if (sender.Equals(BtnResetNightDb)) BoxNightDb.Value = 45;
            if (sender.Equals(BtnResetMorningHour)) BoxMorningHour.Value = 6;
            if (sender.Equals(BtnResetNightHour)) BoxNightHour.Value = 22;
            if (sender.Equals(BtnResetVolumeStep)) BoxVolumeStep.Value = 2;
        }

        private void BtnApplyThresholds_Click(object sender, EventArgs e)
        {
            Settings.Default.DayDecibelThreshold = BoxDayDb.Value;
            Settings.Default.NightDecibelThreshold = BoxNightDb.Value;
            Settings.Default.MorningHour = BoxMorningHour.Value;
            Settings.Default.NightHour = BoxNightHour.Value;
            Settings.Default.VolumeStep = BoxVolumeStep.Value;
            Settings.Default.SourceTitleAsChanging = ChckBoxSourceTitleAsChanging.Checked;

            Settings.Default.Save();
        }

        private void BtnSetAccentColor_Click(object sender, EventArgs e)
        {
            var uiSettings = new UISettings();
            var accentColor = uiSettings.GetColorValue(UIColorType.Accent);
            var drawingColor = Color.FromArgb(accentColor.A, accentColor.R, accentColor.G, accentColor.B);

            editingTheme.LabelsActive = drawingColor;
            BtnLabelsActiveColor.BackColor = drawingColor;
            editingTheme.LabelsHeaders = drawingColor;
            BtnLabelsHeadersColor.BackColor = drawingColor;
            editingTheme.LabelsChanging = drawingColor;
            BtnLabelsChangingColor.BackColor = drawingColor;
            editingTheme.LabelsHigh = drawingColor;
            BtnLabelsHighColor.BackColor = drawingColor;

            editingTheme.VolumePeakMeterLow = drawingColor;
            BtnColorVolPeakLow.BackColor = drawingColor;
            editingTheme.VolumePeakMeterHigh = drawingColor;
            BtnColorVolPeakHigh.BackColor = drawingColor;
            editingTheme.SoundWave = drawingColor;
            BtnSoundWaveColor.BackColor = drawingColor;
            editingTheme.FreqPeakMeter = drawingColor;
            BtnFreqPeakMeterColor.BackColor = drawingColor;
            editingTheme.SpectrumAnalyser = drawingColor;
            BtnColorSpectrumAnalyser.BackColor = drawingColor;
            editingTheme.SpectrumAnalyserHighPeak = drawingColor;
            BtnColorSpectrumAnalyserHighPeaks.BackColor = drawingColor;
        }

    }
}
