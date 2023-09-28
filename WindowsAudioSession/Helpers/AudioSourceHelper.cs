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

                AudioSourceChanged?.Invoke(null, _audioSourceText);
            }            
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
            Task.Run(mediaManager.GetFocusedSession().ControlSession.TryTogglePlayPauseAsync);
        }

        public void TryPlayNext()
        {
            Task.Run(mediaManager.GetFocusedSession().ControlSession.TrySkipNextAsync);
        }

        public void TryPlayPrevious()
        {
            Task.Run(mediaManager.GetFocusedSession().ControlSession.TrySkipPreviousAsync);
        }

        public void Dispose()
        {
            mediaManager.Dispose();
        }
    }
}
