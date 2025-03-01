using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Easing;
using MisterGames.Common.Inputs;
using MisterGames.Common.Labels;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.ActionLib.Inputs {
    
    [Serializable]
    public sealed class PlayGamepadVibrationAction : IActorAction {
        
        public GamepadMotor motor;
        public LabelValue priority;
        [Min(0f)] public float weight = 1f;
        [Min(0f)] public float duration = 1f;
        [Min(0f)] public float durationRandom;
        [Range(0f, 1f)] public float amplitude = 1f;
        [Range(0f, 1f)] public float amplitudeRandom;
        public OscillatedCurve curve;
        
        public async UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var vibration = DeviceService.Instance.GamepadVibration;

            vibration.Register(this, priority.GetValue());
            
            float dur = Mathf.Max(0f, duration + Random.Range(-durationRandom, durationRandom));
            float speed = dur > 0f ? 1f / dur : float.MaxValue;
            float t = 0f;
            
            float amp = Mathf.Max(0f, amplitude + Random.Range(-amplitudeRandom, amplitudeRandom));
            
            while (t < 1f && !cancellationToken.IsCancellationRequested) {
                t = Mathf.Clamp01(t + UnityEngine.Time.deltaTime * speed);
                vibration.SetMotor(this, motor, curve.Evaluate(t) * amp, weight);
                
                await UniTask.Yield();
            }
            
            vibration.Unregister(this);
        }
        
    }
    
}