using System;
using MisterGames.Common.Maths;
using MisterGames.Interact.Interactives;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(Interactive))]
    public sealed class ApplyForceOnInteraction : MonoBehaviour {
        
        [SerializeField] private Rigidbody _rigidbody;
        
        [Header("Force")]
        [SerializeField] private ForceMode _forceMode = ForceMode.VelocityChange;
        [SerializeField] private float _forceMin;
        [SerializeField] private float _forceMax;
        
        [Header("Direction")]
        [SerializeField] private Mode _directionMode;
        [SerializeField] private Vector3 _rotationOffset;
        [SerializeField] [Range(0f, 180f)] private float _randomizeAngle;

        private enum Mode {
            ViewVector,
            OnUnitSphere,
            OnUnitCircle,
        }
        
        private Interactive _interactive;

        private void Awake() {
            _interactive  = GetComponent<Interactive>();
        }

        private void OnEnable() {
            _interactive.OnStartInteract += OnStartInteract;
        }

        private void OnDisable() {
            _interactive.OnStartInteract -= OnStartInteract;
        }

        private void OnStartInteract(IInteractiveUser user) {
            var dir = _directionMode switch {
                Mode.ViewVector => Quaternion.Euler(_rotationOffset) * (_rigidbody.position - user.ViewOrigin.position).normalized,
                Mode.OnUnitSphere => Random.onUnitSphere,
                Mode.OnUnitCircle => RandomExtensions.OnUnitCircle(Quaternion.Euler(_rotationOffset) * Vector3.forward),
                _ => throw new ArgumentOutOfRangeException()
            };

            var rot = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(dir, Random.onUnitSphere), _randomizeAngle / 180f);
            float force = Random.Range(_forceMin, _forceMax);
            
            _rigidbody.AddForce(rot * dir * force, _forceMode);
        }

#if UNITY_EDITOR
        private void Reset() {
            _rigidbody = GetComponent<Rigidbody>();
        }
#endif
    }
    
}