using MisterGames.Common.Easing;
using UnityEngine;

namespace MisterGames.Common.Inputs {

    [CreateAssetMenu(fileName = nameof(GamepadHidColorAnimationAsset), menuName = "MisterGames/Input/" + nameof(GamepadHidColorAnimationAsset))]
    public sealed class GamepadHidColorAnimationAsset : ScriptableObject {

        [SerializeField] private int _priority;
        [SerializeField] [Min(0f)] private float _duration = 1f;
        [SerializeField] private Mode _mode;
        [SerializeField] private bool _yoyo;
        [SerializeField] private AnimationCurve _progressCurve = EasingType.Linear.ToAnimationCurve();
        [SerializeField] private GamepadHidColorAnimation _animation;

        public enum Mode {
            OneShot,
            Loop,
        }

        public int Priority => _priority;
        public float Duration => _duration;

        public GamepadHidColorState Evaluate(float t) {
            return _animation.Evaluate(_progressCurve.Evaluate(t));
        }

        public GamepadHidColorState EvaluateAtTime(float time, out bool finished) {
            return Evaluate(GetProgress(time, out finished));
        }

        private float GetProgress(float time, out bool finished) {
            if (_duration <= 0f) {
                finished = _mode == Mode.OneShot;
                return _yoyo ? 0f : 1f;
            }

            float cycle = _yoyo ? 2f * _duration : _duration;

            if (_mode == Mode.OneShot) {
                finished = time >= cycle;
                if (finished) return _yoyo ? 0f : 1f;
            }
            else {
                finished = false;
                time %= cycle;
            }

            float p = time / _duration;
            return _yoyo && p > 1f ? 2f - p : p;
        }
    }

}
