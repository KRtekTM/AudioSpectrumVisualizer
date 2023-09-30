using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsAudioSession.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using System.Runtime.Remoting.Contexts;
using System.Windows;

namespace WindowsAudioSession.UI
{
    public partial class StyleSettings : Form
    {
        public StyleSettings()
        {
            InitializeComponent();
        }

        StyleJson currentTheme = new StyleJson()
        {
            LabelsActive = Settings.Default.ColorLabelsActive,
            LabelsHeaders = Settings.Default.ColorLabelsHeaders,
            LabelsChanging = Settings.Default.ColorLabelsChanging,
            LabelsHigh = Settings.Default.ColorLabelsHigh,

            SoundWave = Settings.Default.ColorSoundWave,
            FreqPeakMeter = Settings.Default.ColorFreqPeakMeter,

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

            SoundWave = Settings.Default.ColorSoundWave,
            FreqPeakMeter = Settings.Default.ColorFreqPeakMeter,

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

            BtnSoundWaveColor.BackColor = currentTheme.SoundWave;
            BtnFreqPeakMeterColor.BackColor = currentTheme.FreqPeakMeter;

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
            editingTheme.FreqPeakMeter = temp;
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

                Settings.Default.ColorSoundWave = editingTheme.SoundWave;
                Settings.Default.ColorFreqPeakMeter = editingTheme.FreqPeakMeter;

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

            public Color SoundWave { get; set; }
            public Color FreqPeakMeter { get; set; }

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

                    editingTheme.SoundWave = style.SoundWave;
                    BtnSoundWaveColor.BackColor = style.SoundWave;
                    editingTheme.FreqPeakMeter = style.FreqPeakMeter;
                    BtnFreqPeakMeterColor.BackColor = style.FreqPeakMeter;

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

    }
}
