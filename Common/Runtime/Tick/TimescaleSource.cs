using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Easing;
using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Common.Tick {
    
    public sealed class TimescaleSource : MonoBehaviour {

        [SerializeField] private LabelValue _timescalePriority;
        [SerializeField] [Min(0f)] private float _timeScale;
        [SerializeField] [Min(0f)] private float _timeScaleDurationOnOpen;
        [SerializeField] [Min(0f)] private float _timeScaleDurationOnClose;
        [SerializeField] private AnimationCurve _timeScaleCurve = EasingType.Linear.ToAnimationCurve();

        private CancellationTokenSource _enableCts;
        private CancellationTokenSource _destroyCts;

        private void Awake() {
            AsyncExt.RecreateCts(ref _destroyCts);
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _destroyCts);
            
            Services.Get<ITimescaleSystem>()?.RemoveTimeScale(this);
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            ChangeTimescale(_timeScale, _timeScaleDurationOnOpen, removeOnFinish: false, _enableCts.Token).Forget();
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            ChangeTimescale(_timeScale, _timeScaleDurationOnOpen, removeOnFinish: false, _destroyCts.Token).Forget();
        }

        private UniTask ChangeTimescale(float timescale, float duration, bool removeOnFinish, CancellationToken cancellationToken) {
            return Services.Get<ITimescaleSystem>()?.ChangeTimeScale(
                source: this,
                _timescalePriority.GetValue(),
                timescale,
                duration,
                removeOnFinish,
                _timeScaleCurve,
                cancellationToken
            ) ?? default;
        }
    }
    
}