using AudioSwitcher.AudioApi.CoreAudio;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WindowsAudioSession.Commands;
using WindowsAudioSession.Helpers;
using WindowsAudioSession.Properties;

namespace WindowsAudioSession.UI
{
    /// <summary>
    /// main window
    /// </summary>
    public partial class WASMainWindow : Window
    {

        private const int requiredWidth = 1280;
        private const int requiredHeight = 400;
        private WindowStyle _windowStyle;
        private WindowState _windowState;
        private ResizeMode _resizeMode;
        private DispatcherTimer timer;
        private CoreAudioDevice audioController = new CoreAudioController().DefaultPlaybackDevice;
        private Point lastTouchPosition, startingTouchPosition;
        private DateTime touchStartTime;
        private TimeSpan requiredTouchDuration = TimeSpan.FromSeconds(2);
        private int touchCount = 0;
        private int _highVolumeThreshold = 70;
        private bool _isTouching, _isMuting, _isTouchMoving = false;
        private int highVolumeThreshold;
        private DateTime _lastUpdateCheck;
        private TimeSpan durationBetweenUpdateCheck = TimeSpan.FromHours(1);
        private KeyValuePair<bool, Version> checkedVersion;
        private System.Windows.Forms.Screen targetScreen = GetRequiredDisplay();


        /// <summary>
        /// creates a new instance
        /// </summary>
        public WASMainWindow()
        {
            InitializeComponent();

            if(targetScreen != null)
            {
                this.Width = targetScreen.WorkingArea.Width;
                this.Height = targetScreen.WorkingArea.Height;
                this.MaxWidth = this.Width;
                this.MaxHeight = this.Height;
                this.ResizeMode = ResizeMode.NoResize;
                WindowStyle = WindowStyle.None;

            }

            this.Title = $"{this.Title} {NetworkHelper.CurrentVersion}";
            TextVersion.Text = $"VERSION: {NetworkHelper.CurrentVersion}";

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
                
                if(App.WASMainViewModel.IsStarted)
                {
                    ButtonStop.Command.Execute(sender);
                    Commands.Commands.Stop.Execute(sender);
                    ButtonStop_Click(sender, null);

                    Panel_LengthSampleFrq.Visibility = Visibility.Visible;
                    Panel_ListBoxSoundCards.Visibility = Visibility.Visible;
                    Panel_StartStop.Visibility = Visibility.Visible;

                    fftControl1.Panel_StackPanelBars.Visibility = Visibility.Visible;

                    App.WASMainViewModel.IsStarted = false;
                    App.WASMainViewModel.CanStart = true;
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
            Settings.Default.LastAudioOut = Panel_ListBoxSoundCards.SelectedItem.ToString();
            Settings.Default.Save();
            TextAudioOut.Text = Panel_ListBoxSoundCards.SelectedItem.ToString().Split(' ').FirstOrDefault();
            TextStereo.Foreground = CustomBrushes.VolumePeakBrush; 
        }

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
            WindowState = WindowState.Maximized;

            if (App.WASMainViewModel.IsStarted)
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
            var ActualTime = DateTime.Now;

            // Aktualizace obsahu TextBlocku s aktuálním časem
            string FlashingText;
            if (WindowStyle == WindowStyle.None && App.WASMainViewModel.IsStarted)
            {
                if (Panel_StartStop.Visibility == Visibility.Visible)
                {
                    GoFullScreen();
                }
                FlashingText = ActualTime.ToString("HH:mm:ss - ddd MM/dd/yyyy");
            }
            else
            {
                FlashingText = ActualTime.ToString("HH:mm:ss");
            }
            TextClock.Text = FlashingText;

            // Aktualizace animovaného bloku "PLAY"
            bool levelMoreThenZero = App.AppComponents.AudioPluginEngine.GetLevel() > 0;
            TextPlay.Foreground = (App.WASMainViewModel.IsStarted && levelMoreThenZero) ? Brushes.White : Brushes.Gray;
            TextPlay.Text = "PLAY ";
            if (App.WASMainViewModel.IsStarted && levelMoreThenZero)
            {
                if ((ActualTime.Second % 4) == 0) TextPlay.Text = "PLAY ";
                if ((ActualTime.Second % 4) == 1) TextPlay.Text = "PLAY ▶";
                if ((ActualTime.Second % 4) == 2) TextPlay.Text = "PLAY ▶▶";
                if ((ActualTime.Second % 4) == 3) TextPlay.Text = "PLAY ▶▶▶";
            }

            // Aktualizace bloku s hlasitostí
            if (App.WASMainViewModel.IsStarted)
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

            // Check for updates
            TimeSpan updCheckTimeSpan = ActualTime - _lastUpdateCheck;
            // Handle four taps gestures
            if (updCheckTimeSpan >= durationBetweenUpdateCheck)
            {
                CheckForUpdates();
            }
            // If there is no new update change "up to date :)" text back to version
            if (updCheckTimeSpan > TimeSpan.FromSeconds(5) && !checkedVersion.Key)
            {
                TextVersion.Text = $"VERSION: {NetworkHelper.CurrentVersion}";
            }
        }

        private void WASMainWindow_TouchDown(object sender, TouchEventArgs e)
        {
            _isTouching = true;

            if (App.WASMainViewModel.IsStarted)
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
            if (App.WASMainViewModel.IsStarted)
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
            if (targetScreen != null)
            {
                this.Left = targetScreen.WorkingArea.Left;
                this.Top = targetScreen.WorkingArea.Top;
                GoFullScreen();
            }

            CheckForUpdates();


            if (Settings.Default.LastAudioOut != null)
            {
                int indexPolozky = -1;

                // Najděte index položky s hledaným textem
                for (int i = 0; i < Panel_ListBoxSoundCards.Items.Count; i++)
                {
                    string item = $"{Panel_ListBoxSoundCards.Items[i]}";
                    if (item == Settings.Default.LastAudioOut)
                    {
                        indexPolozky = i;
                    }
                }

                if (indexPolozky != -1)
                {
                    // Nastavte vybraný index, aby označil položku
                    Panel_ListBoxSoundCards.SelectedIndex = indexPolozky;

                    App.WASMainViewModel.IsStarted = true;
                    App.WASMainViewModel.CanStart = false;
                    ButtonStart.Command.Execute(sender);
                    Commands.Commands.Start.Execute(sender);
                    ButtonStart_Click(sender, null);
                }
            }
                
        }

        private void CheckForUpdates()
        {
            _lastUpdateCheck = DateTime.Now;
            TextVersion.Text = $"VERSION: checking...";

            // Check for updates
            checkedVersion = NetworkHelper.CheckUpdate();
            if (checkedVersion.Key)
            {
                TextVersion.Text = $"VERSION: new update!";

                if (System.Windows.Forms.MessageBox.Show($"There is new version available.{Environment.NewLine}Current version: {NetworkHelper.CurrentVersion}{Environment.NewLine}New version: {checkedVersion.Value}{Environment.NewLine}{Environment.NewLine}Do you want to open download website?", "Update available", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(NetworkHelper.DownloadUrl);
                }
            }
            else
            {
                TextVersion.Text = $"VERSION: up to date :)";
            }
        }

        private static System.Windows.Forms.Screen GetRequiredDisplay()
        {
            var screens = System.Windows.Forms.Screen.AllScreens;
            return screens.FirstOrDefault(screen => screen.Bounds.Width == requiredWidth && screen.Bounds.Height == requiredHeight);
        }
    }
}
