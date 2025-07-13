using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Logic.Phys {
    
    [RequireComponent(typeof(CollisionBatchGroup))]
    public sealed class CollisionBatchGroupSounds : MonoBehaviour {

        [SerializeField] private CollisionBatchGroup _collisionBatchGroup;
        [SerializeField] [Min(0f)] private float _soundCooldown = 0.25f;

        private readonly Dictionary<int, float> _lastSoundTimeMap = new();
        
        private void OnEnable() {
            _collisionBatchGroup.OnContact += OnContact;
        }

        private void OnDisable() 
        {
            _collisionBatchGroup.OnContact -= OnContact;
        }

        private void OnContact(TriggerEventType evt, Rigidbody rb, int surfaceMaterial, Vector3 point, Vector3 normal, Vector3 impulse) {
            if (evt != TriggerEventType.Enter ||
                _lastSoundTimeMap.TryGetValue(rb.GetInstanceID(), out float lastSoundTime) && TimeSources.scaledTime < lastSoundTime + _soundCooldown) 
            {
                return;
            }
            
            _lastSoundTimeMap[rb.GetInstanceID()] = TimeSources.scaledTime;
            
            Debug.Log($"CollisionBatchGroupSounds.OnContact: f {Time.frameCount}, rb {rb}, surfaceMaterial {surfaceMaterial}, impulse {impulse.magnitude}, vel {rb.linearVelocity.magnitude}");
        }

#if UNITY_EDITOR
        private void Reset() {
            _collisionBatchGroup = GetComponent<CollisionBatchGroup>();
        }
#endif
    }
    
}