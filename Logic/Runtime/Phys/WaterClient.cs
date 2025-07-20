using System.Collections.Generic;
using System.Linq;
using MisterGames.Actors;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(Rigidbody))]
    public sealed class WaterClient : MonoBehaviour, IActorComponent {

        [SerializeField] private bool _ignoreWaterZone;
        [SerializeField] private WaterClientPreset _preset;
        [SerializeField] private Rigidbody[] _rigidbodies;
        [SerializeField] private Transform[] _mainFloatingPoints;
        [SerializeField] private float _surfaceOffset;
        
        public bool IgnoreWaterZone => _ignoreWaterZone;
        
        public IReadOnlyList<Rigidbody> Rigidbodies => _rigidbodies;
        public IReadOnlyList<Transform> FloatingPoints => _mainFloatingPoints;
        public float SurfaceOffset => _surfaceOffset;
        
        public float MaxSpeed => _preset.MaxSpeed;
        public float ForceMul => _preset.ForceMul;
        public float TorqueMul => _preset.TorqueMul;
        public float DecelerationMul => _preset.DecelerationMul;
        public float TorqueDecelerationMul => _preset.TorqueDecelerationMul;
        
#if UNITY_EDITOR
        private void Reset() {
            var mainRb = GetComponent<Rigidbody>();
            _rigidbodies = GetComponentsInChildren<Rigidbody>().OrderBy(rb => rb == mainRb ? 1 : -1).ToArray();
            _mainFloatingPoints = new []{ mainRb.transform };
        }
#endif
    }
    
}