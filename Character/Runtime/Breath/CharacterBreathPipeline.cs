using MisterGames.Character.Core;
using MisterGames.Tick.Core;
using UnityEngine;

namespace MisterGames.Character.Breath {

    public sealed class CharacterBreathPipeline : CharacterPipelineBase, ICharacterBreathPipeline, IUpdate {

        [SerializeField] private PlayerLoopStage _playerLoopStage = PlayerLoopStage.Update;

        [Header("Period")]
        [SerializeField] [Min(0f)] private float _period = 5f;
        [SerializeField] [Min(0f)] private float _periodRandom = 1f;

        [Header("Amplitude")]
        [SerializeField] [Min(0f)] private float _amplitude = 1f;
        [SerializeField] [Min(0f)] private float _amplitudeRandom = 0.1f;

        public event BreathCallback OnInhale = delegate {  };
        public event BreathCallback OnExhale = delegate {  };

        public override bool IsEnabled { get => enabled; set => enabled = value; }

        public float Period {
            get => _period;
            set => _period = value;
        }

        public float Amplitude {
            get => _amplitude;
            set => _amplitude = value;
        }

        private float _targetPeriod;
        private float _timer;
        private int _dir;

        private void OnEnable() {
            TimeSources.Get(_playerLoopStage).Subscribe(this);
        }

        private void OnDisable() {
            _timer = 0f;
            TimeSources.Get(_playerLoopStage).Unsubscribe(this);
        }

        public void OnUpdate(float dt) {
            int lastDirection = _dir;

            if (_timer <= 0f || _timer >= _targetPeriod) {
                _dir = 1;
                _timer = 0f;
                _targetPeriod = _period + Random.Range(-_periodRandom, _periodRandom);
            }
            else if (_timer >= _targetPeriod * 0.5f) {
                _dir = -1;
            }

            _timer += dt;

            if (lastDirection == _dir) return;

            float targetAmplitude = _amplitude + Random.Range(-_amplitudeRandom, _amplitudeRandom);

            if (_dir > 0) OnInhale.Invoke(_targetPeriod * 0.5f, targetAmplitude);
            else OnExhale.Invoke(_targetPeriod * 0.5f, targetAmplitude);
        }
    }

}
