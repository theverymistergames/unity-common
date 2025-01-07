using MisterGames.Common.Pooling;
using MisterGames.Common.Tick;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(DecalProjector))]
    [RequireComponent(typeof(PoolElement))]
    public sealed class HDRPDecalFaderByLifetime : MonoBehaviour, IUpdate {

        [SerializeField] [Range(0f, 1f)] private float _startFadeFactor = 1f;
        [SerializeField] [Range(0f, 1f)] private float _startAfterLifetimeProgress = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float _endFadeFactor = 0f;
        
        private DecalProjector _decalProjector;
        private PoolElement _poolElement;

        private void Awake() {
            _decalProjector = GetComponent<DecalProjector>();
            _poolElement = GetComponent<PoolElement>();
        }

        private void OnEnable() {
            PlayerLoopStage.LateUpdate.Subscribe(this);
        }

        private void OnDisable() {
            PlayerLoopStage.LateUpdate.Unsubscribe(this);
        }

        void IUpdate.OnUpdate(float dt) {
            float t = _poolElement.LifetimeProgress;
            float p = t < _startAfterLifetimeProgress 
                ? 0f 
                : _startAfterLifetimeProgress < 1f 
                    ? (t - _startAfterLifetimeProgress) / (1f - _startAfterLifetimeProgress)
                    : 1f;
            
            _decalProjector.fadeFactor = Mathf.Lerp(_startFadeFactor, _endFadeFactor, p);
        }
    }
    
}