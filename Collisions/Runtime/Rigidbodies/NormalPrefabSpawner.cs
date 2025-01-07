using System;
using MisterGames.Collisions.Core;
using MisterGames.Common.Maths;
using MisterGames.Common.Pooling;
using UnityEngine;

namespace MisterGames.Collisions.Rigidbodies {
    
    public sealed class NormalPrefabSpawner : MonoBehaviour {

        [SerializeField] private CollisionEmitter _collisionEmitter;
        [SerializeField] private TriggerEventType _triggerEvent;
        [SerializeField] private SpawnData[] prefabs;
        
        [Serializable]
        private struct SpawnData {
            public ParentMode parentMode;
            public ScaleMode scaleMode;
            public Vector3 positionOffset;
            public Vector3 rotationOffset;
            public Vector3 scale;
            public GameObject[] prefabs;
        }

        private enum ParentMode {
            None,
            UseTransform,
            UseCollidedTransform,
        }

        private enum ScaleMode {
            UsePrefabScale,
            UseExplicitScale,
            MultiplyPrefabScale,
        }
        
        private Transform _transform;

        private void Awake() {
            _transform = transform;
        }

        private void OnEnable() {
            _collisionEmitter.Subscribe(_triggerEvent, OnCollision);
        }

        private void OnDisable() {
            _collisionEmitter.Unsubscribe(_triggerEvent, OnCollision);
        }

        private void OnCollision(Collision collision) {
            var contact = collision.GetContact(0);
            var rot = Quaternion.LookRotation(contact.normal, _transform.forward);

            for (int i = 0; i < prefabs.Length; i++) {
                Spawn(ref prefabs[i], contact.point, rot, contact.otherCollider);
            }
        }

        private void Spawn(ref SpawnData spawnData, Vector3 position, Quaternion rotation, Collider collider) {
            var parent = spawnData.parentMode switch {
                ParentMode.None => PrefabPool.Main.ActiveSceneRoot,
                ParentMode.UseTransform => _transform,
                ParentMode.UseCollidedTransform => collider.transform,
                _ => throw new ArgumentOutOfRangeException()
            };

            var pos = position + rotation * spawnData.positionOffset;
            var rot = rotation * Quaternion.Euler(spawnData.rotationOffset); 
            
            for (int i = 0; i < spawnData.prefabs.Length; i++) {
                var t = PrefabPool.Main.Get(spawnData.prefabs[i], pos, rot).transform;
                var localScale = t.localScale;
                
                t.localScale = spawnData.scaleMode switch {
                    ScaleMode.UsePrefabScale => localScale,
                    ScaleMode.UseExplicitScale => spawnData.scale,
                    ScaleMode.MultiplyPrefabScale => localScale.Multiply(spawnData.scale),
                    _ => throw new ArgumentOutOfRangeException()
                };

                t.SetParent(parent, worldPositionStays: true);
            }
        }
    }
    
}