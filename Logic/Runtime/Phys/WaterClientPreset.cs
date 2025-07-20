using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    [CreateAssetMenu(fileName = nameof(WaterClientPreset), menuName = "MisterGames/Physics/" + nameof(WaterClientPreset))]
    public sealed class WaterClientPreset : ScriptableObject {
        
        [Header("Force")]
        [SerializeField] private bool _overrideMaxSpeed = false;
        [VisibleIf(nameof(_overrideMaxSpeed))]
        [SerializeField] [Min(-1f)] private float _maxSpeed = -1f;
        [SerializeField] private float _forceMul = 1f;
        [SerializeField] private float _torqueMul = 1f;
        [SerializeField] private float _decelerationMul = 1f;
        [SerializeField] private float _torqueDecelerationMul = 1f;
        
        public float MaxSpeed => _overrideMaxSpeed ? _maxSpeed : -2f;
        public float ForceMul => _forceMul;
        public float TorqueMul => _torqueMul;
        public float DecelerationMul => _decelerationMul;
        public float TorqueDecelerationMul => _torqueDecelerationMul;
    }
    
}