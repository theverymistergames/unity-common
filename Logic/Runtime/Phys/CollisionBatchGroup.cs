using System.Runtime.CompilerServices;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Utils;
using MisterGames.Common.Jobs;
using MisterGames.Common.Layers;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MisterGames.Logic.Phys {

    public sealed class CollisionBatchGroup : MonoBehaviour {

        [SerializeField] private LayerMask _layerMask;

        public event ContactEvent OnContact = delegate { };
        public delegate void ContactEvent(TriggerEventType evt, Rigidbody rigidbody, int rbMaterial, Collider collider, Vector3 point, Vector3 normal, Vector3 impulse);

        private NativeHashMap<EntityId, int> _rbIdToMaterialMap;

        private void Awake() {
            _rbIdToMaterialMap = new NativeHashMap<EntityId, int>(2, Allocator.Persistent);
        }

        private void OnEnable() {
            Physics.ContactEvent += OnContactEvent;
        }

        private void OnDisable() {
            Physics.ContactEvent -= OnContactEvent;
        }

        private void OnDestroy() {
            if (_rbIdToMaterialMap.IsCreated) _rbIdToMaterialMap.Dispose();
        }

        public void Register(Rigidbody rigidbody, int surfaceMaterial = 0) {
            _rbIdToMaterialMap[rigidbody.GetEntityId()] = surfaceMaterial;
        }

        public void Unregister(Rigidbody rigidbody) {
            _rbIdToMaterialMap.Remove(rigidbody.GetEntityId());
        }

        private void OnContactEvent(PhysicsScene scene, NativeArray<ContactPairHeader>.ReadOnly headers) {
            int count = headers.Length;

            var contactEnterArray = new NativeArray<ContactInfo>(count, Allocator.TempJob);
            var contactStayArray = new NativeArray<ContactInfo>(count, Allocator.TempJob);
            var contactExitArray = new NativeArray<ContactInfo>(count, Allocator.TempJob);

            var contactResultEnterArray = new NativeArray<ContactResult>(count, Allocator.TempJob);
            var contactResultStayArray = new NativeArray<ContactResult>(count, Allocator.TempJob);
            var contactResultExitArray = new NativeArray<ContactResult>(count, Allocator.TempJob);
            
            var calculateContactJob = new CalculateContactJob {
                headers = headers,
                contactEnterArray = contactEnterArray,
                contactStayArray = contactStayArray,
                contactExitArray = contactExitArray,
            };

            var filterValidContactJob = new FilterValidContactJob {
                registeredBodyIdToMaterialMap = _rbIdToMaterialMap,
                contactEnterArray = contactEnterArray,
                contactStayArray = contactStayArray,
                contactExitArray = contactExitArray,
                contactResultEnterArray = contactResultEnterArray,
                contactResultStayArray = contactResultStayArray,
                contactResultExitArray = contactResultExitArray,
            };

            int batchCount = JobExt.BatchFor(count);
            
            var calculateContactJobHandle = calculateContactJob.Schedule(count, batchCount);
            var filterContactJobHandle = filterValidContactJob.Schedule(count, batchCount, calculateContactJobHandle);
            
            filterContactJobHandle.Complete();
            
            contactEnterArray.Dispose();
            contactStayArray.Dispose(); 
            contactExitArray.Dispose();

            for (int i = 0; i < count; i++) {
                ref var result = ref contactResultEnterArray.GetRef(i);
                
                if (result.thisBodyId != EntityId.None && 
                    CollisionUtils.GetColliderByEntityId(result.otherColliderId) is var c0 && 
                    IsValidContact(c0, result.otherBodyId)) 
                {
                    OnContact.Invoke(
                        TriggerEventType.Enter, 
                        CollisionUtils.GetRigidbodyByEntityId(result.thisBodyId), result.thisBodyMaterial, c0, 
                        result.point, result.normal, result.impulse
                    );
                }
                
                if (result.otherBodyId != EntityId.None && 
                    CollisionUtils.GetColliderByEntityId(result.thisColliderId) is var c1 && 
                    IsValidContact(c1, result.thisBodyId)) 
                {
                    OnContact.Invoke(
                        TriggerEventType.Enter, 
                        CollisionUtils.GetRigidbodyByEntityId(result.otherBodyId), result.otherBodyMaterial, c1, 
                        result.point, result.normal, result.impulse
                    );
                }
                
                result = ref contactResultStayArray.GetRef(i);
                
                if (result.thisBodyId != EntityId.None && 
                    CollisionUtils.GetColliderByEntityId(result.otherColliderId) is var c2 && 
                    IsValidContact(c2, result.otherBodyId)) 
                {
                    OnContact.Invoke(
                        TriggerEventType.Stay, 
                        CollisionUtils.GetRigidbodyByEntityId(result.thisBodyId), result.thisBodyMaterial, c2, 
                        result.point, result.normal, result.impulse
                    );
                }
                
                if (result.otherBodyId != EntityId.None && 
                    CollisionUtils.GetColliderByEntityId(result.thisColliderId) is var c3 && 
                    IsValidContact(c3, result.thisBodyId)) 
                {
                    OnContact.Invoke(
                        TriggerEventType.Stay, 
                        CollisionUtils.GetRigidbodyByEntityId(result.otherBodyId), result.otherBodyMaterial, c3, 
                        result.point, result.normal, result.impulse
                    );
                }
                
                result = ref contactResultExitArray.GetRef(i);
                
                if (result.thisBodyId != EntityId.None && 
                    CollisionUtils.GetColliderByEntityId(result.otherColliderId) is var c4 && 
                    IsValidContact(c4, result.otherBodyId)) 
                {
                    OnContact.Invoke(
                        TriggerEventType.Exit, 
                        CollisionUtils.GetRigidbodyByEntityId(result.thisBodyId), result.thisBodyMaterial, c4, 
                        result.point, result.normal, result.impulse
                    );
                }
                
                if (result.otherBodyId != EntityId.None && 
                    CollisionUtils.GetColliderByEntityId(result.thisColliderId) is var c5 && 
                    IsValidContact(c5, result.thisBodyId)) 
                {
                    OnContact.Invoke(
                        TriggerEventType.Exit, 
                        CollisionUtils.GetRigidbodyByEntityId(result.otherBodyId), result.otherBodyMaterial, c5, 
                        result.point, result.normal, result.impulse
                    );
                }
            }
            
            contactResultEnterArray.Dispose();
            contactResultStayArray.Dispose(); 
            contactResultExitArray.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValidContact(Collider collider, EntityId otherBodyId) {
            return _layerMask.Contains(otherBodyId != EntityId.None ? collider.attachedRigidbody.gameObject.layer : collider.gameObject.layer);
        }
        
        private readonly struct ContactInfo {
            
            public readonly EntityId thisColliderId;
            public readonly EntityId otherColliderId;
            public readonly EntityId thisBodyId;
            public readonly EntityId otherBodyId;
            public readonly float3 point;
            public readonly float3 normal;
            public readonly float3 impulse;
            
            public ContactInfo(
                EntityId thisColliderId, EntityId otherColliderId, EntityId thisBodyId, EntityId otherBodyId, 
                float3 point, float3 normal, float3 impulse) 
            {
                this.thisColliderId = thisColliderId;
                this.otherColliderId = otherColliderId;
                this.thisBodyId = thisBodyId;
                this.otherBodyId = otherBodyId;
                this.point = point;
                this.normal = normal;
                this.impulse = impulse;
            }
        }
        
        private readonly struct ContactResult {
            
            public readonly EntityId thisBodyId;
            public readonly EntityId otherBodyId;
            public readonly int thisBodyMaterial;
            public readonly int otherBodyMaterial;
            public readonly EntityId thisColliderId;
            public readonly EntityId otherColliderId;
            public readonly float3 point;
            public readonly float3 normal;
            public readonly float3 impulse;
            
            public ContactResult(
                EntityId thisBodyId, EntityId otherBodyId, int thisBodyMaterial, int otherBodyMaterial, 
                EntityId thisColliderId, EntityId otherColliderId, 
                float3 point, float3 normal, float3 impulse) 
            {
                this.thisBodyId = thisBodyId;
                this.otherBodyId = otherBodyId;
                this.thisBodyMaterial = thisBodyMaterial;
                this.otherBodyMaterial = otherBodyMaterial;
                this.thisColliderId = thisColliderId;
                this.otherColliderId = otherColliderId;
                this.point = point;
                this.normal = normal;
                this.impulse = impulse;
            }
        }

        [BurstCompile]
        private struct FilterValidContactJob : IJobParallelFor {
            
            [ReadOnly] public NativeHashMap<EntityId, int> registeredBodyIdToMaterialMap;
            [ReadOnly] public NativeArray<ContactInfo> contactEnterArray;
            [ReadOnly] public NativeArray<ContactInfo> contactStayArray;
            [ReadOnly] public NativeArray<ContactInfo> contactExitArray;

            [WriteOnly] public NativeArray<ContactResult> contactResultEnterArray;
            [WriteOnly] public NativeArray<ContactResult> contactResultStayArray;
            [WriteOnly] public NativeArray<ContactResult> contactResultExitArray;
            
            public void Execute(int index) {
                var info = contactEnterArray[index];
                CreateContactResult(ref info, out var result);
                contactResultEnterArray[index] = result;
                
                info = contactStayArray[index];
                CreateContactResult(ref info, out result);
                contactResultStayArray[index] = result;
                
                info = contactExitArray[index];
                CreateContactResult(ref info, out result);
                contactResultExitArray[index] = result;
            }

            private void CreateContactResult(ref ContactInfo info, out ContactResult result) {
                var thisBodyId = EntityId.None;
                var otherBodyId = EntityId.None;
                int thisBodyMaterial = 0;
                int otherBodyMaterial = 0;
                var thisColliderId = EntityId.None;
                var otherColliderId = EntityId.None;

                if (info.thisBodyId != EntityId.None && info.otherColliderId != EntityId.None && 
                    registeredBodyIdToMaterialMap.TryGetValue(info.thisBodyId, out thisBodyMaterial)) 
                {
                    thisBodyId = info.thisBodyId;
                    otherColliderId = info.otherColliderId;
                }
                
                if (info.otherBodyId != EntityId.None && info.thisColliderId != EntityId.None && 
                    registeredBodyIdToMaterialMap.TryGetValue(info.otherBodyId, out otherBodyMaterial)) 
                {
                    otherBodyId = info.otherBodyId;
                    thisColliderId = info.thisColliderId;
                }

                if (thisBodyId == EntityId.None && otherColliderId == EntityId.None && 
                    otherBodyId == EntityId.None && thisColliderId == EntityId.None) 
                {
                    result = default;
                    return;
                }
                
                result = new ContactResult(
                    thisBodyId, otherBodyId, thisBodyMaterial, otherBodyMaterial, 
                    thisColliderId, otherColliderId, 
                    info.point, info.normal, info.impulse
                );
            }
        }

        [BurstCompile]
        private struct CalculateContactJob : IJobParallelFor {
            
            [ReadOnly] public NativeArray<ContactPairHeader>.ReadOnly headers;
            
            [WriteOnly] public NativeArray<ContactInfo> contactEnterArray;
            [WriteOnly] public NativeArray<ContactInfo> contactStayArray;
            [WriteOnly] public NativeArray<ContactInfo> contactExitArray;
            
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

                var thisColliderEnter = EntityId.None;
                var thisColliderStay = EntityId.None;
                var thisColliderExit = EntityId.None;
                
                var otherColliderEnter = EntityId.None;
                var otherColliderStay = EntityId.None;
                var otherColliderExit = EntityId.None;
                
                for (int i = 0; i < header.pairCount; i++) {
                    ref readonly var pair = ref header.GetContactPair(i);
                    
                    if (pair.isCollisionEnter) {
                        thisColliderEnter = pair.colliderEntityId;
                        otherColliderEnter = pair.otherColliderEntityId;
                        
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
                        thisColliderStay = pair.colliderEntityId;
                        otherColliderStay = pair.otherColliderEntityId;
                        
                        for (int k = 0; k < pair.contactCount; k++) {
                            ref readonly var contact = ref pair.GetContactPoint(k);
                            averageNormalStay += (float3) contact.normal;
                            averagePointStay += (float3) contact.position;
                            averageImpulseStay += (float3) contact.impulse;
                        }

                        countStay += pair.contactCount;
                    }
                    
                    for (int k = 0; k < pair.contactCount; k++) {
                        thisColliderExit = pair.colliderEntityId;
                        otherColliderExit = pair.otherColliderEntityId;
                        
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
                        header.bodyEntityId, 
                        header.otherBodyEntityId,
                        averagePointEnter,
                        averageNormalEnter, 
                        averageImpulseEnter
                    )
                    : default;
                
                contactStayArray[index] = countStay > 0 
                    ? new ContactInfo(
                        thisColliderStay,
                        otherColliderStay,
                        header.bodyEntityId, 
                        header.otherBodyEntityId,
                        averagePointStay,
                        averageNormalStay, 
                        averageImpulseStay
                    )
                    : default;
                
                contactExitArray[index] = countExit > 0 
                    ? new ContactInfo( 
                        thisColliderExit,
                        otherColliderExit,
                        header.bodyEntityId, 
                        header.otherBodyEntityId,
                        averagePointExit,
                        averageNormalExit, 
                        averageImpulseExit
                    )
                    : default;
            }
        }
    }
    
}