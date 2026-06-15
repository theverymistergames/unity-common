using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Colors;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace MisterGames.Scenes.Splash {
    
    public sealed class SplashScreenVideo : MonoBehaviour
    {
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RawImage targetRawImage;
        [SerializeField] private VideoClip videoClip;
        [SerializeField] [Min(0f)] private float _delay = 1f;
        [SerializeField] [Min(0f)] private float _fadeOut = 0.25f;

        private CancellationTokenSource _destroyCts;
        private float _awakeTime;
        
        private void Awake() {
            AsyncExt.RecreateCts(ref _destroyCts);
            
            _awakeTime = Time.realtimeSinceStartup;
            targetRawImage.color = Color.black;
            
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.APIOnly;

            if (videoClip != null)
            {
                videoPlayer.source = VideoSource.VideoClip;
                videoPlayer.clip = videoClip;
            }

            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.Prepare();
        }

        private void OnDestroy()
        {
            AsyncExt.DisposeCts(ref _destroyCts);
            
            if (videoPlayer != null)
            {
                videoPlayer.prepareCompleted -= OnVideoPrepared;
            }
        }

        private void OnVideoPrepared(VideoPlayer source) {
            float preparedInTime = Time.realtimeSinceStartup - _awakeTime;
            float delay = Mathf.Max(0f, _delay - preparedInTime);
            
            PlayDelayed(source, targetRawImage, delay, _fadeOut, _destroyCts.Token).Forget();
        }

        private static async UniTask PlayDelayed(
            VideoPlayer videoPlayer,
            RawImage target,
            float delay,
            float fadeOut,
            CancellationToken cancellationToken) 
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken)
                .SuppressCancellationThrow();
            if (cancellationToken.IsCancellationRequested) return;
            
            target.texture = videoPlayer.texture;
            videoPlayer.Play();

            var color0 = Color.black;
            var color1 = Color.white;
            float t = 0f;
            float speed = fadeOut > 0f ? 1f / fadeOut : float.MaxValue;
            
            while (!cancellationToken.IsCancellationRequested) {
                t += Time.unscaledDeltaTime * speed;
                target.color = Color.Lerp(color0, color1, t);

                await UniTask.Yield();
            }
        }
    }
    
}