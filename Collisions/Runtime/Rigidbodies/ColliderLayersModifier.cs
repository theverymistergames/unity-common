using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class ColliderLayersModifier : MonoBehaviour {

        [SerializeField] private bool _resetOnDisable = true;
        [SerializeField] private Collider[] _colliders;
        [SerializeField] private Modifier[] _modifiers;

        [Serializable]
        private struct Modifier {
            [Min(0f)] public float delay;
            public LayerMask includeLayers;
            public LayerMask excludeLayers;
        }

        private struct LayersData {
            public LayerMask includeLayers;
            public LayerMask excludeLayers;
        }

        private CancellationTokenSource _enableCts;
        private LayersData[] _layersData;

        private void Awake() {
            _layersData = new LayersData[_colliders.Length];
            
            for (int i = 0; i < _colliders.Length; i++) {
                var c = _colliders[i];
                _layersData[i] = new LayersData { includeLayers = c.includeLayers, excludeLayers = c.excludeLayers };
            }
        }

        private void OnEnable() { 
            AsyncExt.RecreateCts(ref _enableCts);
            ApplyModifiers(_enableCts.Token);            
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            if (_resetOnDisable) ResetModifiers();
        }

        private void ApplyModifiers(CancellationToken cancellationToken) {
            for (int i = 0; i < _modifiers.Length; i++) {
                ApplyModifierDelayed(_modifiers[i], cancellationToken).Forget();
            }
        }

        private void ResetModifiers() {
            for (int i = 0; i < _colliders.Length; i++) {
                var c = _colliders[i];
                var data = _layersData[i];
                c.includeLayers = data.includeLayers;
                c.excludeLayers = data.excludeLayers;
            }
        }

        private async UniTask ApplyModifierDelayed(Modifier modifier, CancellationToken cancellationToken) {
            await UniTask.Delay(TimeSpan.FromSeconds(modifier.delay), cancellationToken: cancellationToken)
                .SuppressCancellationThrow();
            
            if (cancellationToken.IsCancellationRequested) return;

            for (int i = 0; i < _colliders.Length; i++) {
                var c = _colliders[i];
                c.includeLayers = modifier.includeLayers;
                c.excludeLayers = modifier.excludeLayers;
            }
        }
    }
    
}