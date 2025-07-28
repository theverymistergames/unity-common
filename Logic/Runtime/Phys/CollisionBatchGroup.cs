using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common.Layers;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Logic.Phys {

    public sealed class CollisionBatchGroup : MonoBehaviour {

        [SerializeField] private LayerMask _layerMask;
        
        private readonly struct ContactInfo {
            
            public readonly int thisColliderId;
            public readonly int otherColliderId;
            public readonly int thisBodyID;
            public readonly int otherBodyID;
            public readonly float3 point;
            public readonly float3 normal;
            public readonly float3 impulse;
            
            public ContactInfo(int thisColliderId, int otherColliderId, int thisBodyID, int otherBodyID, float3 point, float3 normal, float3 impulse) {
                this.thisColliderId = thisColliderId;
                this.otherColliderId = otherColliderId;
                this.thisBodyID = thisBodyID;
                this.otherBodyID = otherBodyID;
                this.point = point;
                this.normal = normal;
                this.impulse = impulse;
            }
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

        private void OnEnable() {
            Physics.ContactEvent += OnContactEvent;
        }

        private void OnDisable() {
            Physics.ContactEvent -= OnContactEvent;
        }

        public void Register(Rigidbody rigidbody, int surfaceMaterial = 0) {
            _rbIdToDataMap[rigidbody.GetInstanceID()] = new RigidbodyData(rigidbody, surfaceMaterial);
        }

        public void Unregister(Rigidbody rigidbody) {
            _rbIdToDataMap.Remove(rigidbody.GetInstanceID());
        }

        private void OnContactEvent(PhysicsScene scene, NativeArray<ContactPairHeader>.ReadOnly headers) {
            int count = headers.Length;

            var contactEnterArray = new NativeArray<ContactInfo>(count, Allocator.TempJob);
            var contactStayArray = new NativeArray<ContactInfo>(count, Allocator.TempJob);
            var contactExitArray = new NativeArray<ContactInfo>(count, Allocator.TempJob);

            var job = new CalculateContactJob {
                headers = headers,
                contactEnterArray = contactEnterArray,
                contactStayArray = contactStayArray,
                contactExitArray = contactExitArray,
            };

            job.Schedule(count, innerloopBatchCount: 256).Complete();
            
            for (int i = 0; i < count; i++) {
                var info = contactEnterArray[i];
                if (IsValidContact(info.thisBodyID, info.otherBodyID, info.otherColliderId, out var rb, out int surfaceMaterial, out var collider)) {
                    OnContact.Invoke(TriggerEventType.Enter, rb, surfaceMaterial, collider, info.point, info.normal, info.impulse);
                }
                if (IsValidContact(info.otherBodyID, info.thisBodyID, info.thisColliderId, out rb, out surfaceMaterial, out collider)) {
                    OnContact.Invoke(TriggerEventType.Enter, rb, surfaceMaterial, collider, info.point, info.normal, info.impulse);
                }
                
                info = contactStayArray[i];
                if (IsValidContact(info.thisBodyID, info.otherBodyID, info.otherColliderId, out rb, out surfaceMaterial, out collider)) {
                    OnContact.Invoke(TriggerEventType.Stay, rb, surfaceMaterial, collider, info.point, info.normal, info.impulse);
                }
                if (IsValidContact(info.otherBodyID, info.thisBodyID, info.thisColliderId, out rb, out surfaceMaterial, out collider)) {
                    OnContact.Invoke(TriggerEventType.Stay, rb, surfaceMaterial, collider, info.point, info.normal, info.impulse);
                }
                
                info = contactExitArray[i];
                if (IsValidContact(info.thisBodyID, info.otherBodyID, info.otherColliderId, out rb, out surfaceMaterial, out collider)) {
                    OnContact.Invoke(TriggerEventType.Exit, rb, surfaceMaterial, collider, info.point, info.normal, info.impulse);
                }
                if (IsValidContact(info.otherBodyID, info.thisBodyID, info.thisColliderId, out rb, out surfaceMaterial, out collider)) {
                    OnContact.Invoke(TriggerEventType.Exit, rb, surfaceMaterial, collider, info.point, info.normal, info.impulse);
                }
            }

            contactEnterArray.Dispose();
            contactStayArray.Dispose(); 
            contactExitArray.Dispose();
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

        [BurstCompile]
        private struct CalculateContactJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<ContactPairHeader>.ReadOnly headers;
            
            public NativeArray<ContactInfo> contactEnterArray;
            public NativeArray<ContactInfo> contactStayArray;
            public NativeArray<ContactInfo> contactExitArray;
            
            public void Execute(int index) {
                float3 averageNormalEnter = default;
                float3 averagePointEnter = default;
                float3 averageImpulseEnter = default;
                
                float3 averageNormalStay = default;
                float3 averagePointStay = default;
                float3 averageImpulseStay = default;
                
                float3 averageNormalExit = default;
                float3 averagePointExit = default;
                float3 averageImpulseExit = default;
                
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
                            averageNormalEnter += (float3) contact.normal;
                            averagePointEnter += (float3) contact.position;
                            averageImpulseEnter += (float3) contact.impulse;
                        }

                        countEnter += pair.contactCount; 
                        continue;
                    }

                    if (pair.isCollisionStay) {
                        thisColliderStay = pair.colliderInstanceID;
                        otherColliderStay = pair.otherColliderInstanceID;
                        
                        for (int k = 0; k < pair.contactCount; k++) {
                            ref readonly var contact = ref pair.GetContactPoint(k);
                            averageNormalStay += (float3) contact.normal;
                            averagePointStay += (float3) contact.position;
                            averageImpulseStay += (float3) contact.impulse;
                        }

                        countStay += pair.contactCount;
                    }
                    
                    for (int k = 0; k < pair.contactCount; k++) {
                        thisColliderExit = pair.colliderInstanceID;
                        otherColliderExit = pair.otherColliderInstanceID;
                        
                        ref readonly var contact = ref pair.GetContactPoint(k);
                        averageNormalExit += (float3) contact.normal;
                        averagePointExit += (float3) contact.position;
                        averageImpulseExit += (float3) contact.impulse;
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
                    ? new ContactInfo(
                        thisColliderEnter,
                        otherColliderEnter,
                        header.bodyInstanceID, 
                        header.otherBodyInstanceID, 
                        averageNormalEnter, 
                        averagePointEnter,
                        averageImpulseEnter
                    )
                    : default;
                
                contactStayArray[index] = countStay > 0 
                    ? new ContactInfo(
                        thisColliderStay,
                        otherColliderStay,
                        header.bodyInstanceID, 
                        header.otherBodyInstanceID, 
                        averageNormalStay, 
                        averagePointStay,
                        averageImpulseStay
                    )
                    : default;
                
                contactExitArray[index] = countExit > 0 
                    ? new ContactInfo( 
                        thisColliderExit,
                        otherColliderExit,
                        header.bodyInstanceID, 
                        header.otherBodyInstanceID, 
                        averageNormalExit, 
                        averagePointExit,
                        averageImpulseExit
                    )
                    : default;
            }
        }
    }
    
}