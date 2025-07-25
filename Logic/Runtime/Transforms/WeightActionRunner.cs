using System;
using System.Collections.Generic;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common.Attributes;
using MisterGames.Common.Easing;
using MisterGames.Common.Tick;
using MisterGames.Common.Volumes;
using MisterGames.Tweens;
using UnityEngine;

namespace MisterGames.Logic.Transforms {
    
    public sealed class WeightActionRunner : MonoBehaviour, IUpdate {
        
        [SerializeField] private PositionWeightProvider _weightProvider;
        [SerializeField] private TriggerListenerForRigidbody _triggerListenerForRigidbody;
        [SerializeField] private WeightMode _weightMode = WeightMode.Average;
        [SerializeField] private float _weightMul = 1f;
        [SerializeField] private AnimationCurve _weightCurve = EasingType.Linear.ToAnimationCurve();
        [SerializeReference] [SubclassSelector] private ITweenProgressAction _action;
        
        private enum WeightMode {
            Min,
            Max,
            Average,
        }
        
        public float WeightMul { get => _weightMul; set => _weightMul = value; }

        private readonly HashSet<Rigidbody> _rigidbodies = new();
        
        private void OnEnable() {
            _triggerListenerForRigidbody.TriggerEnter += TriggerEnter;
            _triggerListenerForRigidbody.TriggerExit += TriggerExit;
            
            var rigidbodies = _triggerListenerForRigidbody.EnteredRigidbodies;
            foreach (var rb in rigidbodies) {
                _rigidbodies.Add(rb);
            }
            
            if (_rigidbodies.Count > 0) PlayerLoopStage.Update.Subscribe(this);
        }

        private void OnDisable() {
            _triggerListenerForRigidbody.TriggerEnter -= TriggerEnter;
            _triggerListenerForRigidbody.TriggerExit -= TriggerExit;
            
            _rigidbodies.Clear();
            
            PlayerLoopStage.Update.Unsubscribe(this);
            ApplyWeight(0f);
        }

        private void TriggerEnter(Rigidbody rigidbody) {
            _rigidbodies.Add(rigidbody);
            
            PlayerLoopStage.Update.Subscribe(this);
        }

        private void TriggerExit(Rigidbody rigidbody) {
            _rigidbodies.Remove(rigidbody);

            if (_rigidbodies.Count > 0) return;
            
            PlayerLoopStage.Update.Unsubscribe(this);
            ApplyWeight(0f);
        }

        void IUpdate.OnUpdate(float dt) {
            ApplyWeight(_weightMul * _weightCurve.Evaluate(GetWeight()));
        }

        private void ApplyWeight(float weight) {
            _action?.OnProgressUpdate(weight);
        }

        private float GetWeight() {
            return _weightMode switch {
                WeightMode.Min => GetWeightMin(),
                WeightMode.Max => GetWeightMax(),
                WeightMode.Average => GetWeightAverage(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private float GetWeightMin() {
            int count = 0;
            float weight = float.MaxValue;
                    
            foreach (var rb in _rigidbodies) {
                if (rb == null) continue;

                weight = Mathf.Min(_weightProvider.GetWeight(rb.position), weight);
                count++;
            }
                    
            return count > 0 ? weight : 0f;
        }
        
        private float GetWeightMax() {
            float weight = 0f;
                    
            foreach (var rb in _rigidbodies) {
                if (rb == null) continue;

                weight = Mathf.Max(_weightProvider.GetWeight(rb.position), weight);
            }
                    
            return weight;
        }
        
        private float GetWeightAverage() {
            int count = 0;
            float weight = 0f;
                    
            foreach (var rb in _rigidbodies) {
                if (rb == null) continue;
                
                weight += _weightProvider.GetWeight(rb.position);
                count++;
            }
                    
            return count > 0 ? weight / count : 0f;
        }
    }
    
}