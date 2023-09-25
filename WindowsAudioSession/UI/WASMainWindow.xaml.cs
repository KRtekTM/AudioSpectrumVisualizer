using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WindowsAudioSession.UI.FFT;
using AudioSwitcher.AudioApi.CoreAudio;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.IO;
using AudioSwitcher.AudioApi;

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

            ButtonStart.Click += ButtonStart_Click;
            ButtonStop.Click += ButtonStop_Click;

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

            string highVolumeThresholdFilePath = Environment.CurrentDirectory + "\\_configVolumeModifier.txt";
            highVolumeThreshold = File.Exists(highVolumeThresholdFilePath) ? Convert.ToInt32(File.ReadAllText(highVolumeThresholdFilePath)) : _highVolumeThreshold;
            // Vytvořte instanci FileSystemWatcher
            FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(highVolumeThresholdFilePath));
            // Nastavte filtrování na konkrétní soubor
            watcher.Filter = Path.GetFileName(highVolumeThresholdFilePath);
            // Přidejte obslužnou metodu pro událost Changed
            watcher.Changed += (sender, e) =>
            {
                if (e.FullPath == highVolumeThresholdFilePath)
                {
                    // Soubor _configVolumeModifier.txt byl změněn
                    highVolumeThreshold = File.Exists(highVolumeThresholdFilePath) ? Convert.ToInt32(File.ReadAllText(highVolumeThresholdFilePath)) : _highVolumeThreshold;

                    // Zde můžete provést další akce s highVolumeThreshold
                }
            };
            // Zapněte sledování změn
            watcher.EnableRaisingEvents = true;
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            TextVolume.Text = "---";
            TextAudioOut.Text = "NONE";
            TextStereo.Foreground = CustomBrushes.VolumePeakTopBrush;
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            TextAudioOut.Text = Panel_ListBoxSoundCards.SelectedItem.ToString().Split(' ').FirstOrDefault();
            TextStereo.Foreground = CustomBrushes.VolumePeakBrush;
        }

        private WindowStyle _windowStyle;
        private WindowState _windowState;
        private ResizeMode _resizeMode;
        private DispatcherTimer timer;
        private CoreAudioDevice audioController = new CoreAudioController().DefaultPlaybackDevice;
        private Point lastTouchPosition, startingTouchPosition; // Poslední pozice dotyku
        private DateTime touchStartTime;
        private TimeSpan requiredTouchDuration = TimeSpan.FromSeconds(2);
        private int touchCount = 0;
        private int _highVolumeThreshold = 70;
        private bool _isTouching, _isMuting, _isTouchMoving = false;
        private int highVolumeThreshold;

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

            if (ButtonStop.IsEnabled)
            {
                Panel_LengthSampleFrq.Visibility = Visibility.Collapsed;
                Panel_ListBoxSoundCards.Visibility = Visibility.Collapsed;
                Panel_StartStop.Visibility = Visibility.Collapsed;

                fftControl1.Panel_StackPanelBars.Visibility = Visibility.Collapsed;
            }
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
            // Aktualizace obsahu TextBlocku s aktuálním časem
            string FlashingText;
            if (WindowStyle == WindowStyle.None && ButtonStop.IsEnabled)
            {
                if (Panel_StartStop.Visibility == Visibility.Visible)
                {
                    GoFullScreen();
                }
                FlashingText = DateTime.Now.ToString("HH:mm:ss - ddd MM/dd/yyyy");
            }
            else
            {
                FlashingText = DateTime.Now.ToString("HH:mm:ss");
            }
            TextClock.Text = FlashingText;

            // Aktualizace animovaného bloku "PLAY"
            bool levelMoreThenZero = App.AppComponents.AudioPluginEngine.GetLevel() > 0;
            TextPlay.Foreground = (ButtonStop.IsEnabled && levelMoreThenZero) ? Brushes.White : Brushes.Gray;
            TextPlay.Text = "PLAY ";
            if (ButtonStop.IsEnabled && levelMoreThenZero)
            {
                if ((DateTime.Now.Second % 4) == 0) TextPlay.Text = "PLAY ";
                if ((DateTime.Now.Second % 4) == 1) TextPlay.Text = "PLAY ▶";
                if ((DateTime.Now.Second % 4) == 2) TextPlay.Text = "PLAY ▶▶";
                if ((DateTime.Now.Second % 4) == 3) TextPlay.Text = "PLAY ▶▶▶";
            }

            // Aktualizace bloku s hlasitostí
            if (ButtonStop.IsEnabled)
            {
                int currentVolume = (int)Math.Round(audioController.Volume);
                TextVolume.Text = (audioController.IsMuted) ? (_isTouching && !_isMuting ? $"MUT {currentVolume:D2}%" : "MUTED") : $"{currentVolume:D2}%"; //format current volume with leading zero for 0-9%

                if (currentVolume >= highVolumeThreshold || (audioController.IsMuted && (!_isTouching || _isMuting)) || TextVolume.Text == "MUTED")
                {
                    TextVolume.Foreground = CustomBrushes.VolumePeakTopBrush;
                }
                else if (_isTouching)
                {
                    TextVolume.Foreground = CustomBrushes.LabelChanging;
                }
                else
                {
                    TextVolume.Foreground = CustomBrushes.Labels;
                }

                if (!_isTouching && touchCount == 1 && _isMuting)
                {
                    _isMuting = false;
                }
            }
        }

        private void WASMainWindow_TouchDown(object sender, TouchEventArgs e)
        {
            _isTouching = true;

            if (ButtonStop.IsEnabled)
            {
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
            startingTouchPosition = e.GetTouchPoint(this).Position;
            lastTouchPosition = startingTouchPosition;
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
                //double delta = (currentTouchPosition.X - lastTouchPosition.X);
                double delta = (currentTouchPosition.X - lastTouchPosition.X);

                if (delta != 0)
                {
                    _isTouchMoving = true;
                }

                // Nastavení hodnoty hlasitosti (předpokládáme, že audioController umožňuje nastavení hlasitosti)
                if (delta != 0 && Math.Abs(delta) > 15)
                {
                    switch (touchCount)
                    {
                        case 0:
                            // Pokud je ztlumeno, při pohybu zruš ztlumení
                            if (audioController.IsMuted)
                            {
                                audioController.ToggleMute();
                            }

                            // Sniž/zvyš hlasitost o 2 procenta
                            audioController.Volume = (delta > 0) ? audioController.Volume + 2 : audioController.Volume - 2;
                            break;
                        case 1:
                            // Handle tap and swipe gesture
                            if(!_isMuting)
                            {
                                audioController.ToggleMute();
                            }
                            _isMuting = true;
                            break;
                    }

                    lastTouchPosition = currentTouchPosition; // Uložení aktuální pozice pro příští krok
                }
            }

            e.Handled = true;
        }

        private void WASMainWindow_TouchUp(object sender, TouchEventArgs e)
        {
            TimeSpan touchDuration = DateTime.Now - touchStartTime;

            // Handle four taps gestures
            if (touchDuration <= requiredTouchDuration && touchCount == 3)
            {
                if (WindowStyle != WindowStyle.None)
                {
                    GoFullScreen();
                    touchStartTime = DateTime.Now;
                }
                else
                {
                    if (!_isTouchMoving && !_isMuting)
                    {
                        App.Current.Shutdown();
                    }
                }
            }

            _isTouching = false;
            _isTouchMoving = false;

            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Požadované rozlišení
            int requiredWidth = 1280;
            int requiredHeight = 400;

            // Získání seznamu všech monitorů
            var screens = System.Windows.Forms.Screen.AllScreens;

            // Najdeme monitor s požadovaným rozlišením
            var targetScreen = screens.FirstOrDefault(screen => screen.Bounds.Width == requiredWidth && screen.Bounds.Height == requiredHeight);

            if (targetScreen != null)
            {
                // Nastavení okna na požadované rozlišení
                this.Left = targetScreen.Bounds.Left;
                this.Top = targetScreen.Bounds.Top;
                this.Width = targetScreen.Bounds.Width;
                this.Height = targetScreen.Bounds.Height;

                // Nastavení režimu fullscreen
                GoFullScreen();
            }
        }
    }
}
