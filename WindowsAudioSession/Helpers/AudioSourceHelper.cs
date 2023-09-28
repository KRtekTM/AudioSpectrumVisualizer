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

        public AudioSourceHelper()
        {
            mediaManager = new MediaManager();
            mediaManager.OnAnySessionOpened += Handle;
            mediaManager.OnAnySessionClosed += Handle;
            mediaManager.OnFocusedSessionChanged += Handle;
            mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;
            mediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
            mediaManager.OnAnyTimelinePropertyChanged += MediaManager_OnAnyTimelinePropertyChanged;
        }


        private void MediaManager_OnAnyMediaPropertyChanged(MediaManager.MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties)
        {
            var newAudioSource = new KeyValuePair<string, string>(mediaProperties.Artist, mediaProperties.Title);

            // Porovnáme nový audioSource s aktuálním
            if (!newAudioSource.Equals(_audioSourceText))
            {
                _audioSourceText = newAudioSource;
                Handle(mediaSession);
            }
        }

        private void Handle(MediaManager.MediaSession mediaSession)
        {
            Handle(mediaSession, null);
        }

        private void Handle(MediaManager.MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionTimelineProperties timelineProperties = null)
        { 
            if(mediaManager.IsStarted)
            {
                currentMediaSession = mediaSession;

                Task.Run(mediaSession.ControlSession.TryGetMediaPropertiesAsync);

                AudioSourceChanged?.Invoke(null, _audioSourceText);
            }            
        }

        private void MediaManager_OnAnyTimelinePropertyChanged(MediaManager.MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionTimelineProperties timelineProperties)
        {
            Handle(mediaSession, timelineProperties);
        }

        private void MediaManager_OnAnyPlaybackStateChanged(MediaManager.MediaSession mediaSession, GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo)
        {
            Handle(mediaSession);
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
            Task.Run(currentMediaSession.ControlSession.TryTogglePlayPauseAsync);
        }

        public void TryPlayNext()
        {
            Task.Run(currentMediaSession.ControlSession.TrySkipNextAsync);
        }

        public void TryPlayPrevious()
        {
            Task.Run(currentMediaSession.ControlSession.TrySkipPreviousAsync);
        }
    }
}
