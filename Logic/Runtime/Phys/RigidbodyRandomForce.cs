using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Logic.Phys {

    public sealed class RigidbodyRandomForce : MonoBehaviour {

        [SerializeField] private Rigidbody _rb;
        [SerializeField] private bool _randomizeRotation = true;
        [SerializeField] [Min(0f)] private float _forceStrength = 0.1f;
        [SerializeField] [Min(0f)] private float _torqueStrength = 0.5f;
        [SerializeField] private ForceMode _forceMode = ForceMode.VelocityChange;

        private void Awake() {
            _rb = GetComponent<Rigidbody>();
        }

        private void OnEnable() {
            if (_randomizeRotation) _rb.rotation = Random.rotation;
            
            ApplyRandomImpulse();
        }

        private void ApplyRandomImpulse() {
            _rb.linearVelocity = Random.insideUnitSphere * _forceStrength;
            _rb.angularVelocity = Random.insideUnitSphere * _torqueStrength;
        }

#if UNITY_EDITOR
        private void Reset() {
            _rb = GetComponent<Rigidbody>();
        }
#endif
    }
    
}