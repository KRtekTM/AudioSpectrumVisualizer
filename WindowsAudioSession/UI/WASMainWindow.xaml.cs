using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WindowsAudioSession.UI.FFT;
using AudioSwitcher.AudioApi.CoreAudio;
using Microsoft.Win32;

namespace WindowsAudioSession.UI
{
    /// <summary>
    /// main window
    /// </summary>
    public partial class WASMainWindow : Window
    {
        /// <summary>
        /// creates a new instance
        /// </summary>
        public WASMainWindow()
        {
            InitializeComponent();
            this.MaxHeight = 400;
            this.MinWidth = 1280;
            this.Height = 400;
            this.Width = 1280;
            this.ResizeMode = ResizeMode.NoResize;

            _windowStyle = this.WindowStyle;
            _windowState = this.WindowState;
            _resizeMode = this.ResizeMode;

            soundWaveControl.Width = Panel_SoundWave.Width;

            // Vytvoření a inicializace DispatcherTimeru
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += Timer_Tick;

            // Spuštění timeru
            timer.Start();

            // Přidejte manipulaci s dotykovými událostmi
            this.TouchDown += WASMainWindow_TouchDown;
            this.TouchMove += WASMainWindow_TouchMove;
            this.TouchUp += WASMainWindow_TouchUp;

            // Tato událost se zavolá, když dojde ke změně nastavení monitoru
            SystemEvents.DisplaySettingsChanged += (sender, e) =>
            {
                if(ButtonStop.IsEnabled)
                {
                    App.Current.Shutdown();
                }
            };
        }

        private WindowStyle _windowStyle;
        private WindowState _windowState;
        private ResizeMode _resizeMode;
        private DispatcherTimer timer;
        private CoreAudioDevice audioController = new CoreAudioController().DefaultPlaybackDevice;
        private double touchValue = 0; // Aktuální hodnota
        private Point lastTouchPosition; // Poslední pozice dotyku
        private DateTime touchStartTime;
        private TimeSpan requiredTouchDuration = TimeSpan.FromSeconds(5);
        private int touchCount = 0;
        private int _highVolumeThreshold = 70;
        private bool _isTouching = false;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                if (WindowState == _windowState)
                {
                    GoFullScreen();
                }
                else
                {
                    WindowState = _windowState;
                    WindowStyle = _windowStyle;
                    ResizeMode = _resizeMode;

                    Panel_LengthSampleFrq.Visibility = Visibility.Visible;
                    Panel_ListBoxSoundCards.Visibility = Visibility.Visible;
                    Panel_StartStop.Visibility = Visibility.Visible;
                    Panel_Grid.Visibility = Visibility.Visible;

                    fftControl1.Visibility = Visibility.Visible;
                    fftControl1.Panel_StackPanelBars.Visibility = Visibility.Visible;

                    Panel_FTTControl2.Visibility = Visibility.Visible;
                    Panel_SoundWave.Visibility = Visibility.Visible;
                }
            }
        }

        private void GoFullScreen()
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;

            Panel_LengthSampleFrq.Visibility = Visibility.Collapsed;
            Panel_ListBoxSoundCards.Visibility = Visibility.Collapsed;
            Panel_StartStop.Visibility = Visibility.Collapsed;

            fftControl1.Panel_StackPanelBars.Visibility = Visibility.Collapsed;
        }

        private bool _inStateChange;

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowStyle == WindowStyle.None && !_inStateChange)
            {
                _inStateChange = true;
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                ResizeMode = ResizeMode.NoResize;

                soundWaveControl.Width = Panel_SoundWave.Width;
                _inStateChange = false;
            }

            base.OnStateChanged(e);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            string FlashingText;

            // Aktualizace obsahu TextBlocku s aktuálním časem
            if (WindowStyle == WindowStyle.None)
            {
                FlashingText = DateTime.Now.ToString("HH:mm:ss - ddd MM/dd/yyyy");
            }
            else
            {
                FlashingText = DateTime.Now.ToString("HH:mm:ss");
            }
            TextClock.Text = FlashingText;

            TextStereo.Foreground = (ButtonStop.IsEnabled) ? Brushes.Red : Brushes.DarkRed;
            TextPlay.Foreground = (ButtonStop.IsEnabled && App.AppComponents.AudioPluginEngine.GetLevel() > 0) ? Brushes.White : Brushes.Gray;
            TextPlay.Text = "PLAY ";
            if (ButtonStop.IsEnabled && App.AppComponents.AudioPluginEngine.GetLevel() > 0) {
                if ((DateTime.Now.Second % 4) == 0) TextPlay.Text = "PLAY ";
                if ((DateTime.Now.Second % 4) == 1) TextPlay.Text = "PLAY ▶";
                if ((DateTime.Now.Second % 4) == 2) TextPlay.Text = "PLAY ▶▶";
                if ((DateTime.Now.Second % 4) == 3) TextPlay.Text = "PLAY ▶▶▶";
            }
            
            if(ButtonStop.IsEnabled)
            {
                int currentVolume = (int)Math.Round(audioController.Volume);
                TextVolume.Text = $"{currentVolume:D2}%"; //format current volume with leading zero for 0-9%
                string highVolumeThresholdFilePath = Environment.CurrentDirectory + "\\_configVolumeModifier.txt";
                int highVolumeThreshold = System.IO.File.Exists(highVolumeThresholdFilePath) ? Convert.ToInt32(System.IO.File.ReadAllText(highVolumeThresholdFilePath)) : _highVolumeThreshold;
                TextVolume.Foreground = (currentVolume >= highVolumeThreshold) ? CustomBrushes.VolumePeakTopBrush : (_isTouching ? CustomBrushes.LabelChanging : CustomBrushes.Labels);
                TextAudioOut.Text = Panel_ListBoxSoundCards.SelectedItem.ToString().Split(' ').FirstOrDefault();
            }
            else
            {
                TextVolume.Text = "---";
            }
        }

        private void WASMainWindow_TouchDown(object sender, TouchEventArgs e)
        {
            _isTouching = true;

            if (ButtonStop.IsEnabled)
            {
                touchValue = audioController.Volume;
                TextVolume.Foreground = CustomBrushes.LabelChanging;
            }

            TimeSpan touchDuration = DateTime.Now - touchStartTime;
            touchStartTime = DateTime.Now;
            if (touchDuration <= requiredTouchDuration)
            {
                touchCount++;
            }
            else
            {
                touchCount = 0;
            }

            // Nastavte počáteční pozici dotyku
            e.TouchDevice.Capture(this);
            lastTouchPosition = e.GetTouchPoint(this).Position;
            e.Handled = true;
        }

        private void WASMainWindow_TouchMove(object sender, TouchEventArgs e)
        {
            if (ButtonStop.IsEnabled)
            {
                // Získání aktuální pozice dotyku
                var touchPoint = e.GetTouchPoint(this);
                var currentTouchPosition = touchPoint.Position;

                // Výpočet změny hodnoty podle pohybu
                double delta = currentTouchPosition.X - lastTouchPosition.X;

                // Aktualizace hodnoty
                touchValue += delta / 10; // Pravděpodobně budete potřebovat jiný koeficient, abyste dosáhli žádaného rozsahu
                touchValue = Math.Max(0, Math.Min(100, touchValue)); // Omezení hodnoty na rozsah 0-100

                // Aktualizace zobrazení hodnoty
                //TextVolume.Text = touchValue.ToString("F1"); // "F1" formátuje hodnotu s jedním desetinným místem

                // Nastavení hodnoty hlasitosti (předpokládáme, že audioController umožňuje nastavení hlasitosti)
                audioController.Volume = touchValue;

                lastTouchPosition = currentTouchPosition; // Uložení aktuální pozice pro příští krok
            }

            e.Handled = true;
        }

        private void WASMainWindow_TouchUp(object sender, TouchEventArgs e)
        {
            TimeSpan touchDuration = DateTime.Now - touchStartTime;
            if (touchDuration <= requiredTouchDuration && touchCount == 2)
            {
                if (WindowStyle != WindowStyle.None)
                {
                    GoFullScreen();
                    touchStartTime = DateTime.Now;
                }
                else if (ButtonStop.IsEnabled)
                {
                    App.Current.Shutdown();
                }
            }

            if (ButtonStop.IsEnabled)
            {
                TextVolume.Foreground = CustomBrushes.Labels;
            }

            _isTouching = false;

            e.Handled = true;
        }
    }
}
