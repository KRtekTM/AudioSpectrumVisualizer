using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Control;
using WindowsMediaController;
using static WindowsMediaController.MediaManager;

namespace WindowsAudioSession.Helpers
{
    public class AudioSourceHelper
    {
        private KeyValuePair<string, string> _audioSourceText;
        public event EventHandler<KeyValuePair<string, string>> AudioSourceChanged;
        private MediaManager mediaManager;
        private MediaSession currentMediaSession;
        public TimeSpan songLength;
        public string sourceApp;

        public AudioSourceHelper()
        {
            mediaManager = new MediaManager();

            mediaManager.OnFocusedSessionChanged += OnFocusedSessionChanged;

            mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;
            mediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
            mediaManager.OnAnyTimelinePropertyChanged += MediaManager_OnAnyTimelinePropertyChanged;
        }

        private void MediaManager_OnAnyMediaPropertyChanged(MediaManager.MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties)
        {
            var newAudioSource = new KeyValuePair<string, string>(mediaProperties.Artist, mediaProperties.Title);

            if (!newAudioSource.Equals(_audioSourceText))
            {
                _audioSourceText = newAudioSource;
                Handle();
            }
        }

        private void OnFocusedSessionChanged(MediaManager.MediaSession mediaSession)
        {
            Handle();
        }

        private void Handle()
        { 
            if(mediaManager.IsStarted)
            {
                currentMediaSession = mediaManager.GetFocusedSession();

                if (currentMediaSession != null)
                {
                    sourceApp = currentMediaSession != null ? currentMediaSession.ControlSession.SourceAppUserModelId : "";

                    Task.Run(currentMediaSession.ControlSession.TryGetMediaPropertiesAsync);
                }
                else
                {
                    sourceApp = "";
                    _audioSourceText = new KeyValuePair<string, string>();
                }
            }

            AudioSourceChanged?.Invoke(null, _audioSourceText);
        }

        private void MediaManager_OnAnyTimelinePropertyChanged(MediaManager.MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionTimelineProperties timelineProperties)
        {
            songLength = timelineProperties.EndTime;
            Handle();
        }

        private void MediaManager_OnAnyPlaybackStateChanged(MediaManager.MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo)
        {
            Handle();
        }

        public void RunManager()
        {
            if (!mediaManager.IsStarted)
            {
                mediaManager.Start();
            }
        }

        public async void RunManagerAsync()
        {
            if (!mediaManager.IsStarted)
            {
                await mediaManager.StartAsync();
            }
        }

        public void TryTogglePlayPause()
        {
            var session = mediaManager.GetFocusedSession();
            if (session != null)
            {
                if (session.ControlSession.GetPlaybackInfo().Controls.IsPlayPauseToggleEnabled && !String.IsNullOrEmpty(session.ControlSession.SourceAppUserModelId))
                {
                    Task.Run(session.ControlSession.TryTogglePlayPauseAsync);
                }
            }
        }

        public void TryPlayNext()
        {
            var session = mediaManager.GetFocusedSession();
            if (session != null)
            {
                if (session.ControlSession.GetPlaybackInfo().Controls.IsNextEnabled && !String.IsNullOrEmpty(session.ControlSession.SourceAppUserModelId))
                {
                    Task.Run(session.ControlSession.TrySkipNextAsync);
                }
            }
        }

        public void TryPlayPrevious()
        {
            var session = mediaManager.GetFocusedSession();
            if (session != null)
            {
                if (session.ControlSession.GetPlaybackInfo().Controls.IsPreviousEnabled && !String.IsNullOrEmpty(session.ControlSession.SourceAppUserModelId))
                {
                    Task.Run(session.ControlSession.TrySkipPreviousAsync);
                }
            }
        }

        public void Dispose()
        {
            try
            {
                mediaManager.Dispose();
            }
            catch { }
        }
    }
}
