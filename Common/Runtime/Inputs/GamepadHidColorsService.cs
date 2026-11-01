using System.Collections.Generic;
using MisterGames.Common.Service;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Common.Inputs {

    public sealed class GamepadHidColorsService : MonoBehaviour, IGamepadHidColors, IUpdate {

        [SerializeField] private GamepadHidColorAnimationAsset _defaultAnimation;

        private readonly struct Data {

            public readonly int priority;
            public readonly int order;
            public readonly bool hasState;
            public readonly GamepadHidColorState state;

            public Data(int priority, int order = 0, bool hasState = false, GamepadHidColorState state = default) {
                this.priority = priority;
                this.order = order;
                this.hasState = hasState;
                this.state = state;
            }
        }

        private sealed class AnimationPlayback {

            public readonly object source;
            public GamepadHidColorAnimationAsset animation;
            public float time;

            public AnimationPlayback(object source, GamepadHidColorAnimationAsset animation) {
                this.source = source;
                this.animation = animation;
            }
        }

        private IDeviceService _deviceService;
        private readonly Dictionary<int, Data> _dataMap = new();
        private readonly List<AnimationPlayback> _animations = new();
        private GamepadHidColorState _resultState;
        private int _orderCounter;
        private bool _enabled;

        private void Awake() {
            _deviceService = Services.Get<IDeviceService>();
        }

        private void OnEnable() {
            _enabled = true;
            
            ApplyState(_resultState);

            if (_defaultAnimation != null) PlayAnimation(this, _defaultAnimation);
            if (_animations.Count > 0) PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDisable() {
            _enabled = false;
            
            PlayerLoopStage.Update.Unsubscribe(this);
            ApplyState(default);
        }

        public void Register(object source, int priority) {
            int hash = source.GetHashCode();
            _dataMap[hash] = _dataMap.TryGetValue(hash, out var data)
                ? new Data(priority, data.order, data.hasState, data.state)
                : new Data(priority);

            UpdateResultState();
        }

        public void Unregister(object source) {
            RemoveAnimation(source);

            if (!_dataMap.Remove(source.GetHashCode())) return;

            UpdateResultState();
        }

        public void SetState(object source, GamepadHidColorState state) {
            int hash = source.GetHashCode();
            if (!_dataMap.TryGetValue(hash, out var data)) return;

            if (data.hasState && data.state == state) return;

            _dataMap[hash] = new Data(data.priority, data.hasState ? data.order : ++_orderCounter, hasState: true, state);

            UpdateResultState();
        }

        public void PlayAnimation(object source, GamepadHidColorAnimationAsset animation) {
            _dataMap[source.GetHashCode()] = new Data(animation.Priority);

            float time = GetPlayingAnimationTime(animation, source);
            int index = IndexOfAnimation(source);

            if (index < 0) {
                _animations.Add(new AnimationPlayback(source, animation) { time = time });
            }
            else {
                _animations[index].animation = animation;
                _animations[index].time = time;
            }

            SetState(source, animation.EvaluateAtTime(time, out _));

            if (_enabled) PlayerLoopStage.Update.Subscribe(this);
        }

        public void StopAnimation(object source) {
            Unregister(source);
        }

        public bool IsAnimationPlaying(object source) {
            return IndexOfAnimation(source) >= 0;
        }

        void IUpdate.OnUpdate(float dt) {
            float delta = Time.unscaledDeltaTime;

            for (int i = _animations.Count - 1; i >= 0; i--) {
                var playback = _animations[i];
                playback.time += delta;

                var state = playback.animation.EvaluateAtTime(playback.time, out bool finished);
                if (finished) _animations.RemoveAt(i);

                SetState(playback.source, state);
            }

            if (_animations.Count == 0) PlayerLoopStage.Update.Unsubscribe(this);
        }

        private void RemoveAnimation(object source) {
            int index = IndexOfAnimation(source);
            if (index < 0) return;

            _animations.RemoveAt(index);
            if (_animations.Count == 0) PlayerLoopStage.Update.Unsubscribe(this);
        }

        private float GetPlayingAnimationTime(GamepadHidColorAnimationAsset animation, object exceptSource) {
            for (int i = 0; i < _animations.Count; i++) {
                var playback = _animations[i];
                if (playback.animation == animation && playback.source != exceptSource) return playback.time;
            }

            return 0f;
        }

        private int IndexOfAnimation(object source) {
            for (int i = 0; i < _animations.Count; i++) {
                if (_animations[i].source == source) return i;
            }

            return -1;
        }

        private void UpdateResultState() {
            var state = BuildResultState();
            if (state == _resultState) return;

            _resultState = state;
            if (_enabled) ApplyState(_resultState);
        }

        private void ApplyState(GamepadHidColorState state) {
            _deviceService.DualSenseAdapter.SetHidColors(state);
        }

        private GamepadHidColorState BuildResultState() {
            bool found = false;
            var top = default(Data);

            foreach (var data in _dataMap.Values) {
                if (!data.hasState) continue;

                if (found &&
                    (data.priority < top.priority || data.priority == top.priority && data.order < top.order))
                {
                    continue;
                }

                top = data;
                found = true;
            }

            return found ? top.state : default;
        }
    }

}
