using AudioSwitcher.AudioApi.CoreAudio;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WASApiBassNet;
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
        private bool showAudioSourceText = false;
        private KeyValuePair<string, string> audioSource;
        private string audioSourceText = ""; // Přidáme proměnnou pro uchování audioSourceText
        private int audioSourceTextStartChar = 0;
        private DateTime lastAudioSourceChangeTime, displayedValueShownSince = DateTime.MinValue; // Pro sledování změn audioSource.Value
        private AudioSourceHelper _audioSourceHelper;
        private int showEachSecondsCount = 25;

        /// <summary>
        /// creates a new instance
        /// </summary>
        public WASMainWindow()
        {
            InitializeComponent();

            if (targetScreen != null)
            {
                GoFullScreen();
            }

            this.Title = $"{this.Title} {NetworkHelper.CurrentVersion}";
            TextVersion.Text = $"VERSION: {NetworkHelper.CurrentVersion}";

            soundWaveControl.Width = Panel_SoundWave.Width;

            ButtonStart.Click += ButtonStart_Click;
            ButtonStop.Click += ButtonStop_Click;
            ButtonPlayPause.Click += ButtonPlayPause_Click;
            ButtonNext.Click += ButtonNext_Click;
            ButtonPrevious.Click += ButtonPrevious_Click;

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

                if (App.WASMainViewModel.IsStarted)
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

            // Subscribe on changes made in volume modifier config file (so the user can change it while the app is running)
            string highVolumeThresholdFilePath = Environment.CurrentDirectory + "\\_configVolumeModifier.txt";
            highVolumeThreshold = File.Exists(highVolumeThresholdFilePath) ? Convert.ToInt32(File.ReadAllText(highVolumeThresholdFilePath)) : _highVolumeThreshold;
            FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(highVolumeThresholdFilePath));
            watcher.Filter = Path.GetFileName(highVolumeThresholdFilePath);
            watcher.Changed += (sender, e) =>
            {
                if (e.FullPath == highVolumeThresholdFilePath)
                {
                    highVolumeThreshold = AppHelper.GetHighVolumeThreshold(_highVolumeThreshold);
                }
            };
            watcher.EnableRaisingEvents = true;

            // Subscribe on changes of Audio source (meaning song or program as shown in Windows volume popup)
            _audioSourceHelper = new AudioSourceHelper();
            Task.Run(_audioSourceHelper.RunManagerAsync);
            _audioSourceHelper.AudioSourceChanged += AudioSourceChangedHandler;
        }

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            _audioSourceHelper.TryPlayPrevious();
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            _audioSourceHelper.TryPlayNext();
        }

        private void ButtonPlayPause_Click(object sender, RoutedEventArgs e)
        {
            _audioSourceHelper.TryTogglePlayPause();
        }

        private void AudioSourceChangedHandler(object sender, KeyValuePair<string, string> e)
        {
            audioSource = e;
            audioSourceTextStartChar = 0;
            showAudioSourceText = true;
            lastAudioSourceChangeTime = DateTime.MinValue;
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
                if (WindowStyle == WindowStyle.None)
                {
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    ResizeMode = ResizeMode.CanMinimize;

                    Panel_LengthSampleFrq.Visibility = Visibility.Visible;
                    Panel_ListBoxSoundCards.Visibility = Visibility.Visible;
                    Panel_StartStop.Visibility = Visibility.Visible;
                    Panel_Grid.Visibility = Visibility.Visible;

                    fftControl1.Visibility = Visibility.Visible;
                    fftControl1.Panel_StackPanelBars.Visibility = Visibility.Visible;

                    Panel_FTTControl2.Visibility = Visibility.Visible;
                    Panel_SoundWave.Visibility = Visibility.Visible;
                }
                else
                {
                    GoFullScreen();
                }
            }
            if (e.Key == Key.F9 && WindowStyle == WindowStyle.None)
            {
                if (this.Height == targetScreen.WorkingArea.Height)
                {
                    Settings.Default.SizeAsWorkingArea = false;

                }
                else
                {
                    Settings.Default.SizeAsWorkingArea = true;
                }
                Settings.Default.Save();

                // Restart app
                GoFullScreen();
            }
        }

        private void GoFullScreen()
        {
            if (targetScreen != null)
            {
                if (Settings.Default.SizeAsWorkingArea)
                {
                    this.MaxHeight = targetScreen.WorkingArea.Height;
                    this.MaxWidth = targetScreen.WorkingArea.Width;
                    TextVersion.Margin = new Thickness(0, (targetScreen.Bounds.Height - targetScreen.WorkingArea.Height) + 6, 0, 0);
                }
                else
                {
                    this.MaxHeight = targetScreen.Bounds.Height;
                    this.MaxWidth = targetScreen.Bounds.Width;
                    TextVersion.Margin = new Thickness(0, 56, 0, 0);
                }
            }

            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;

            soundWaveControl.Width = Panel_SoundWave.Width;

            if (App.WASMainViewModel.IsStarted)
            {
                Panel_LengthSampleFrq.Visibility = Visibility.Collapsed;
                Panel_ListBoxSoundCards.Visibility = Visibility.Collapsed;
                Panel_StartStop.Visibility = Visibility.Collapsed;

                fftControl1.Panel_StackPanelBars.Visibility = Visibility.Collapsed;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Fix for fullscreen setting
            if (WindowStyle == WindowStyle.None && App.WASMainViewModel.IsStarted)
            {
                if (Panel_StartStop.Visibility == Visibility.Visible)
                {
                    GoFullScreen();
                }
            }

            // Values used through TICK
            var ActualTime = DateTime.Now;
            bool levelMoreThenZero = App.AppComponents.AudioPluginEngine.GetLevel() > 0;

            UpdateTextToDisplay(ActualTime, levelMoreThenZero);

            // Aktualizace animovaného bloku "PLAY"
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
                            if (!_isMuting)
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

            LoadingScreen.Visibility = Visibility.Hidden;
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

        private void TextVolume_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TextVolume.Foreground = CustomBrushes.LabelChanging;
            audioController.Volume = (e.Delta > 0) ? audioController.Volume + 2 : audioController.Volume - 2;

            e.Handled = true;
        }

        private static System.Windows.Forms.Screen GetRequiredDisplay()
        {
            var screens = System.Windows.Forms.Screen.AllScreens;
            return screens.FirstOrDefault(screen => screen.Bounds.Width == requiredWidth && screen.Bounds.Height == requiredHeight);
        }

        private void UpdateTextToDisplay(DateTime ActualTime, bool levelMoreThenZero)
        {
            string FlashingText;
            int maxCharsInText = 30;

            bool isAudioSourceAppSet = !String.IsNullOrEmpty(_audioSourceHelper.sourceApp);
            ButtonNext.IsEnabled = isAudioSourceAppSet && _audioSourceHelper.CanNext;
            ButtonPrevious.IsEnabled = isAudioSourceAppSet && _audioSourceHelper.CanPrevious;
            ButtonPlayPause.IsEnabled = isAudioSourceAppSet && _audioSourceHelper.CanPlayPauseToggle;
            ButtonPlayPause.Content = levelMoreThenZero && ButtonPlayPause.IsEnabled ? "b" : "a";

            if (isAudioSourceAppSet)// && levelMoreThenZero)
            {
                TextSourceApp.Text = $"{_audioSourceHelper.sourceApp.ToUpperInvariant().Replace(".EXE", "")}";
            }
            else
            {
                TextSourceApp.Text = $"";
            }

            if (WindowStyle == WindowStyle.None && App.WASMainViewModel.IsStarted)
            {

                // If audio source changed, display it
                if (!audioSource.Equals(new KeyValuePair<string, string>(null, null)))
                {
                    if (audioSource.Value != "" && !audioSourceText.Equals(TextHelper.RemoveDiacritics(audioSource.Value)))
                    {
                        audioSourceText = TextHelper.RemoveDiacritics(audioSource.Value);
                        audioSourceTextStartChar = 0;
                        showAudioSourceText = true;
                        lastAudioSourceChangeTime = ActualTime;
                    }
                    else
                    {
                        // Show again each spcified interval in seconds
                        int secondsToShowWholeText = (audioSourceText.Length - maxCharsInText);
                        int secondsOver = (secondsToShowWholeText > 28) ? secondsToShowWholeText - 28 : 0;
                        bool isMoreThenSet = Math.Round((ActualTime - lastAudioSourceChangeTime).TotalSeconds) % (showEachSecondsCount + secondsOver) == 0;
                        if (audioSourceText == TextHelper.RemoveDiacritics(audioSource.Value) && isMoreThenSet)
                        {
                            audioSourceTextStartChar = 0;
                            showAudioSourceText = true;
                        }
                    }
                }


                if (levelMoreThenZero && showAudioSourceText && isAudioSourceAppSet)
                {
                    // Get substring which we are able to show on display
                    string displayedText = audioSourceText.Substring(audioSourceTextStartChar, Math.Min(maxCharsInText, audioSourceText.Length - audioSourceTextStartChar));
                    FlashingText = displayedText;

                    // Until we reach the end of text, increment the trim
                    if (audioSourceTextStartChar + maxCharsInText >= audioSourceText.Length)
                    {
                        if (displayedValueShownSince == DateTime.MinValue)
                        {
                            displayedValueShownSince = ActualTime;
                        }

                        if ((ActualTime - displayedValueShownSince) > TimeSpan.FromSeconds(audioSourceText.Length > 28 ? 2 : 4))
                        {
                            showAudioSourceText = false;
                            displayedValueShownSince = DateTime.MinValue;
                        }
                    }
                    else
                    {
                        audioSourceTextStartChar++;
                    }

                    TextClockLabel.Text = audioSource.Key;
                }
                else
                {
                    audioSourceTextStartChar = 0;
                    showAudioSourceText = false;
                    FlashingText = ActualTime.ToString("HH:mm:ss - ddd MM/dd/yyyy");
                }

                // Update other info shown whole song
                TimeSpan songLenght = _audioSourceHelper.songLength;
                if (!songLenght.Equals(TimeSpan.Zero) && isAudioSourceAppSet)
                {
                    if (songLenght.TotalHours > 0)
                    {
                        TextRemainingTime.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", (int)songLenght.TotalHours, songLenght.Minutes, songLenght.Seconds);
                    }
                    else
                    {
                        TextRemainingTime.Text = string.Format("{0:D2}:{1:D2}", (int)songLenght.TotalMinutes, songLenght.Seconds);
                    }
                }
                else
                {
                    TextRemainingTime.Text = "";
                }
            }
            else
            {
                FlashingText = ActualTime.ToString("HH:mm:ss");
                TextRemainingTime.Text = "";
                TextSourceApp.Text = $"";
            }

            TextClockLabel.Text = (isAudioSourceAppSet && !audioSource.Equals(new KeyValuePair<string, string>(null, null))) ? $"ARTIST: {audioSource.Key.ToUpperInvariant()}" : "WORLD CLOCK";
            TextClock.Text = FlashingText;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LoadingScreen.Visibility = Visibility.Visible;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _audioSourceHelper.Dispose();
            audioController.Dispose();
        }
    }
}
