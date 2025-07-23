using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common.Layers;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MisterGames.Logic.Phys {

    public sealed class CollisionBatchGroup : MonoBehaviour {

        [SerializeField] private LayerMask _layerMask;
        
        private struct ContactInfo {
            public int thisColliderId;
            public int otherColliderId;
            public int thisBodyID;
            public int otherBodyID;
            public Vector3 point;
            public Vector3 normal;
            public Vector3 impulse;
        }

        private readonly struct RigidbodyData {
            public readonly Rigidbody rigidbody;
            public readonly int surfaceMaterial;

            public RigidbodyData(Rigidbody rigidbody, int surfaceMaterial) {
                this.rigidbody = rigidbody;
                this.surfaceMaterial = surfaceMaterial;
            }
        }

        public event ContactEvent OnContact = delegate { };
        public delegate void ContactEvent(TriggerEventType evt, Rigidbody rigidbody, int rbMaterial, Collider collider, Vector3 point, Vector3 normal, Vector3 impulse);

        private readonly Dictionary<int, RigidbodyData> _rbIdToDataMap = new();

        private NativeArray<ContactInfo> _contactEnterArray;
        private NativeArray<ContactInfo> _contactStayArray;
        private NativeArray<ContactInfo> _contactExitArray;
        private int _contactInfoCount;

        private void OnEnable() {
            Physics.ContactEvent += OnContactEvent;
        }

        private void OnDisable() {
            Physics.ContactEvent -= OnContactEvent;

            _contactEnterArray.Dispose();
            _contactStayArray.Dispose();
            _contactExitArray.Dispose();
        }

        public void Register(Rigidbody rigidbody, int surfaceMaterial = 0) {
            _rbIdToDataMap[rigidbody.GetInstanceID()] = new RigidbodyData(rigidbody, surfaceMaterial);
        }

        public void Unregister(Rigidbody rigidbody) {
            _rbIdToDataMap.Remove(rigidbody.GetInstanceID());
        }

        private void OnContactEvent(PhysicsScene scene, NativeArray<ContactPairHeader>.ReadOnly headers) {
            int count = headers.Length;

            if (_contactEnterArray.Length < count) {
                _contactEnterArray.Dispose();
                _contactStayArray.Dispose();
                _contactExitArray.Dispose();

                int n = Mathf.NextPowerOfTwo(count);
                _contactEnterArray = new NativeArray<ContactInfo>(n, Allocator.Persistent);
                _contactStayArray = new NativeArray<ContactInfo>(n, Allocator.Persistent);
                _contactExitArray = new NativeArray<ContactInfo>(n, Allocator.Persistent);
            }

            _contactInfoCount = count;

            var job = new CalculateContactJob {
                headers = headers,
                contactEnterArray = _contactEnterArray,
                contactStayArray = _contactStayArray,
                contactExitArray = _contactExitArray,
            };

            job.Schedule(count, innerloopBatchCount: 256).Complete();
            
            NotifyCollisions();
        }

        private void NotifyCollisions() {
            for (int i = 0; i < _contactInfoCount; i++) {
                var info = _contactEnterArray[i];
                if (IsValidContact(info.thisBodyID, info.otherBodyID, info.otherColliderId, out var rb, out int surfaceMaterial, out var collider)) {
                    OnContact.Invoke(TriggerEventType.Enter, rb, surfaceMaterial, collider, info.point, info.normal, info.impulse);
                }
                if (IsValidContact(info.otherBodyID, info.thisBodyID, info.thisColliderId, out rb, out surfaceMaterial, out collider)) {
                    OnContact.Invoke(TriggerEventType.Enter, rb, surfaceMaterial, collider, info.point, info.normal, info.impulse);
                }
                
                info = _contactStayArray[i];
                if (IsValidContact(info.thisBodyID, info.otherBodyID, info.otherColliderId, out rb, out surfaceMaterial, out collider)) {
                    OnContact.Invoke(TriggerEventType.Stay, rb, surfaceMaterial, collider, info.point, info.normal, info.impulse);
                }
                if (IsValidContact(info.otherBodyID, info.thisBodyID, info.thisColliderId, out rb, out surfaceMaterial, out collider)) {
                    OnContact.Invoke(TriggerEventType.Stay, rb, surfaceMaterial, collider, info.point, info.normal, info.impulse);
                }
                
                info = _contactExitArray[i];
                if (IsValidContact(info.thisBodyID, info.otherBodyID, info.otherColliderId, out rb, out surfaceMaterial, out collider)) {
                    OnContact.Invoke(TriggerEventType.Exit, rb, surfaceMaterial, collider, info.point, info.normal, info.impulse);
                }
                if (IsValidContact(info.otherBodyID, info.thisBodyID, info.thisColliderId, out rb, out surfaceMaterial, out collider)) {
                    OnContact.Invoke(TriggerEventType.Exit, rb, surfaceMaterial, collider, info.point, info.normal, info.impulse);
                }
            }
        }

        private bool IsValidContact(
            int thisBodyId,
            int otherBodyId,
            int colliderId,
            out Rigidbody rigidbody,
            out int surfaceMaterial,
            out Collider collider) 
        {
            if (thisBodyId == 0 || colliderId == 0 ||
                !_rbIdToDataMap.TryGetValue(thisBodyId, out var data)) 
            {
                rigidbody = null;
                collider = null;
                surfaceMaterial = 0;
                return false;
            }
            
            rigidbody = data.rigidbody;
            surfaceMaterial = data.surfaceMaterial;
            collider = CollisionUtils.GetColliderByInstanceId(colliderId);

            return _layerMask.Contains(otherBodyId != 0 ? collider.attachedRigidbody.gameObject.layer : collider.gameObject.layer);
        }

        private struct CalculateContactJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<ContactPairHeader>.ReadOnly headers;
            public NativeArray<ContactInfo> contactEnterArray;
            public NativeArray<ContactInfo> contactStayArray;
            public NativeArray<ContactInfo> contactExitArray;
            
            public void Execute(int index) {
                var averageNormalEnter = Vector3.zero;
                var averagePointEnter = Vector3.zero;
                var averageImpulseEnter = Vector3.zero;
                
                var averageNormalStay = Vector3.zero;
                var averagePointStay = Vector3.zero;
                var averageImpulseStay = Vector3.zero;
                
                var averageNormalExit = Vector3.zero;
                var averagePointExit = Vector3.zero;
                var averageImpulseExit = Vector3.zero;
                
                int countEnter = 0;
                int countStay = 0;
                int countExit = 0;

                var header = headers[index];

                int thisColliderEnter = 0;
                int thisColliderStay = 0;
                int thisColliderExit = 0;
                
                int otherColliderEnter = 0;
                int otherColliderStay = 0;
                int otherColliderExit = 0;
                
                for (int i = 0; i < header.pairCount; i++) {
                    ref readonly var pair = ref header.GetContactPair(i);
                    
                    if (pair.isCollisionEnter) {
                        thisColliderEnter = pair.colliderInstanceID;
                        otherColliderEnter = pair.otherColliderInstanceID;
                        
                        for (int k = 0; k < pair.contactCount; k++) {
                            ref readonly var contact = ref pair.GetContactPoint(k);
                            averageNormalEnter += contact.normal;
                            averagePointEnter += contact.position;
                            averageImpulseEnter += contact.impulse;
                        }

                        countEnter += pair.contactCount; 
                        continue;
                    }

                    if (pair.isCollisionStay) {
                        thisColliderStay = pair.colliderInstanceID;
                        otherColliderStay = pair.otherColliderInstanceID;
                        
                        for (int k = 0; k < pair.contactCount; k++) {
                            ref readonly var contact = ref pair.GetContactPoint(k);
                            averageNormalStay += contact.normal;
                            averagePointStay += contact.position;
                            averageImpulseStay += contact.impulse;
                        }

                        countStay += pair.contactCount;
                    }
                    
                    for (int k = 0; k < pair.contactCount; k++) {
                        thisColliderExit = pair.colliderInstanceID;
                        otherColliderExit = pair.otherColliderInstanceID;
                        
                        ref readonly var contact = ref pair.GetContactPoint(k);
                        averageNormalExit += contact.normal;
                        averagePointExit += contact.position;
                        averageImpulseExit += contact.impulse;
                    }

                    countExit += pair.contactCount;
                }

                if (countEnter > 0) {
                    averageNormalEnter /= countEnter;
                    averagePointEnter /= countEnter;
                    averageImpulseEnter /= countEnter;
                }

                if (countStay > 0) {
                    averageNormalStay /= countStay;
                    averagePointStay /= countStay;
                    averageImpulseStay /= countStay;
                }
                
                if (countExit > 0) {
                    averageNormalExit /= countExit;
                    averagePointExit /= countExit;
                    averageImpulseExit /= countExit;
                }
                
                contactEnterArray[index] = countEnter > 0 
                    ? new ContactInfo { 
                        thisColliderId = thisColliderEnter,
                        otherColliderId = otherColliderEnter,
                        thisBodyID = header.bodyInstanceID, 
                        otherBodyID = header.otherBodyInstanceID, 
                        normal = averageNormalEnter, 
                        point = averagePointEnter,
                        impulse = averageImpulseEnter,
                    }
                    : default;
                
                contactStayArray[index] = countStay > 0 
                    ? new ContactInfo {
                        thisColliderId = thisColliderStay,
                        otherColliderId = otherColliderStay,
                        thisBodyID = header.bodyInstanceID, 
                        otherBodyID = header.otherBodyInstanceID, 
                        normal = averageNormalStay, 
                        point = averagePointStay,
                        impulse = averageImpulseStay,
                    }
                    : default;
                
                contactExitArray[index] = countExit > 0 
                    ? new ContactInfo { 
                        thisColliderId = thisColliderExit,
                        otherColliderId = otherColliderExit,
                        thisBodyID = header.bodyInstanceID, 
                        otherBodyID = header.otherBodyInstanceID, 
                        normal = averageNormalExit, 
                        point = averagePointExit,
                        impulse = averageImpulseExit,
                    }
                    : default;
            }
        }
    }
    
}