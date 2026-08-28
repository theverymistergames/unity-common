using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Tick;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace MisterGames.Scenes.Splash {
    
    public sealed class SplashScreenVideo : MonoBehaviour {
        
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RawImage targetRawImage;
        [SerializeField] private VideoClip videoClip;
        [SerializeField] [Min(0f)] private float _delay = 1f;
        [SerializeField] [Min(0f)] private float _fadeOut = 0.25f;

        private CancellationTokenSource _destroyCts;
        private float _awakeTime;
        private bool _playRequested;
        private bool _focused = true;
        
        private void Awake() {
            AsyncExt.RecreateCts(ref _destroyCts);
            
            _awakeTime = TimeSources.unscaledTime;
            targetRawImage.color = Color.black;
            
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.APIOnly;

            if (videoClip != null) {
                videoPlayer.source = VideoSource.VideoClip;
                videoPlayer.clip = videoClip;
            }

            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.Prepare();
        }

        private void OnDestroy()
        {
            AsyncExt.DisposeCts(ref _destroyCts);
            
            if (videoPlayer != null) {
                videoPlayer.prepareCompleted -= OnVideoPrepared;
            }
        }

        private void OnApplicationFocus(bool hasFocus) {
            SetFocused(hasFocus);
        }

        private void OnApplicationPause(bool pauseStatus) {
            // Not every platform reports going into background as a focus change.
            SetFocused(!pauseStatus);
        }

        private void SetFocused(bool focused) {
            if (_focused == focused) return;

            _focused = focused;

            // Coming back needs a resync whether or not the video was paused on the way out: the audio
            // can fall behind while the app is away even where the player itself keeps running.
            UpdateState(resync: focused);
        }

        private void UpdateState(bool resync) {
            // A focus change that arrives before the video is asked to play, or before it is prepared,
            // leaves its state here, and the play call that follows applies it instead of overriding it.
            if (!_playRequested || !videoPlayer.isPrepared) return;

            bool play = _focused;

#if UNITY_EDITOR
            // The editor keeps playing while it has no focus, so the splash is not interrupted there.
            play = true;
#endif

            if (!play) {
                videoPlayer.Pause();
                return;
            }

            // Audio of this player goes out through the audio engine, which is suspended and rebuilt
            // with the app while the video clock keeps its own count: seeking to where the player
            // stands is what puts the two tracks back on the same time.
            if (resync) videoPlayer.time = videoPlayer.time;

            videoPlayer.Play();
        }
        
        private void OnVideoPrepared(VideoPlayer source) {
            float preparedInTime = TimeSources.unscaledTime - _awakeTime;
            float delay = Mathf.Max(0f, _delay - preparedInTime);
            
            PlayDelayed(delay, _fadeOut, _destroyCts.Token).Forget();
        }

        private async UniTask PlayDelayed(float delay, float fadeOut, CancellationToken cancellationToken) {
            await AsyncExt.DelayUnscaled(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;

            targetRawImage.texture = videoPlayer.texture;

            // Starting the video is a request, not a play call: an app that is already away keeps it
            // paused until it comes back.
            _playRequested = true;
            UpdateState(resync: false);

            var color0 = Color.black;
            var color1 = Color.white;
            float t = 0f;
            float speed = fadeOut > 0f ? 1f / fadeOut : float.MaxValue;

            while (t < 1f && !cancellationToken.IsCancellationRequested) {
                t += TimeSources.unscaledDeltaTime * speed;
                targetRawImage.color = Color.Lerp(color0, color1, t);

                await UniTask.Yield();
            }
        }
    }
    
}