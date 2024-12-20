using MisterGames.Common;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Dbg.Behaviours {
    
    internal sealed class RigidbodyTester : MonoBehaviour {
        
        [SerializeField] private Vector3 _direction = Vector3.up;
        [SerializeField] private bool _randomizeDir = true;
        
        [Header("Force")]
        [SerializeField] private float _force = 10f;
        [SerializeField] private float _torque = 10f;
        [SerializeField] private ForceMode _forceMode = ForceMode.VelocityChange;
        
        [Button]
        private void ApplyForce() {
            if (!Application.isPlaying) return;
            
            var f = _randomizeDir ? Random.onUnitSphere * _force : _direction * _force;
            var t = _randomizeDir ? Random.onUnitSphere * _torque : _direction * _torque;
            var rb = GetComponent<Rigidbody>();
            
            rb.AddForce(f, _forceMode);
            rb.AddTorque(t, _forceMode);
            
            DebugExt.DrawRay(rb.position, f, Color.magenta, 3f);
        }
        
        [Header("Apply Velocity")]
        [SerializeField] private float _velocityMagnitude = 10f;
        
        [Button]
        private void ApplyVelocity() {
            if (!Application.isPlaying) return;
            
            var f = _randomizeDir ? Random.onUnitSphere * _velocityMagnitude : _direction * _velocityMagnitude;
            var rb = GetComponent<Rigidbody>();
            
            rb.linearVelocity = f;
            
            DebugExt.DrawRay(rb.position, f, Color.cyan, 3f);
        }
    }
    
}