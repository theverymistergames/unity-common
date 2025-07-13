using System.Collections.Generic;
using MisterGames.Collisions.Core;
using MisterGames.Common.Tick;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MisterGames.Logic.Phys {

    public sealed class CollisionBatchGroup : MonoBehaviour, IUpdate {

        private struct ContactInfo {
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
        public delegate void ContactEvent(TriggerEventType evt, Rigidbody rigidbody, int surfaceMaterial, Vector3 point, Vector3 normal, Vector3 impulse);

        private readonly Dictionary<int, RigidbodyData> _rbIdToDataMap = new();

        private NativeArray<ContactInfo> _contactEnterArray;
        private NativeArray<ContactInfo> _contactStayArray;
        private NativeArray<ContactInfo> _contactExitArray;
        private int _contactInfoCount;
        private JobHandle _jobHandle;

        private void OnEnable() {
            Physics.ContactEvent += OnContactEvent;

            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            Physics.ContactEvent -= OnContactEvent;

            PlayerLoopStage.FixedUpdate.Unsubscribe(this);

            _jobHandle.Complete();
            
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

        void IUpdate.OnUpdate(float dt) {
            _jobHandle.Complete();

            for (int i = 0; i < _contactInfoCount; i++) {
                var info = _contactEnterArray[i];
                if (info.thisBodyID != 0 || info.otherBodyID != 0) {
                    if (_rbIdToDataMap.TryGetValue(info.thisBodyID, out var thisData)) {
                        OnContact.Invoke(TriggerEventType.Enter, thisData.rigidbody, thisData.surfaceMaterial, info.point, info.normal, info.impulse);
                    }

                    if (_rbIdToDataMap.TryGetValue(info.thisBodyID, out var otherData)) {
                        OnContact.Invoke(TriggerEventType.Enter, otherData.rigidbody, otherData.surfaceMaterial, info.point, info.normal, info.impulse);
                    }
                }
                
                info = _contactStayArray[i];
                if (info.thisBodyID != 0 || info.otherBodyID != 0) {
                    if (_rbIdToDataMap.TryGetValue(info.thisBodyID, out var thisData)) {
                        OnContact.Invoke(TriggerEventType.Stay, thisData.rigidbody, thisData.surfaceMaterial, info.point, info.normal, info.impulse);
                    }

                    if (_rbIdToDataMap.TryGetValue(info.thisBodyID, out var otherData)) {
                        OnContact.Invoke(TriggerEventType.Stay, otherData.rigidbody, otherData.surfaceMaterial, info.point, info.normal, info.impulse);
                    }
                }
                
                info = _contactExitArray[i];
                if (info.thisBodyID != 0 || info.otherBodyID != 0) {
                    if (_rbIdToDataMap.TryGetValue(info.thisBodyID, out var thisData)) {
                        OnContact.Invoke(TriggerEventType.Exit, thisData.rigidbody, thisData.surfaceMaterial, info.point, info.normal, info.impulse);
                    }

                    if (_rbIdToDataMap.TryGetValue(info.thisBodyID, out var otherData)) {
                        OnContact.Invoke(TriggerEventType.Exit, otherData.rigidbody, otherData.surfaceMaterial, info.point, info.normal, info.impulse);
                    }
                }
            }
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

            _jobHandle = job.Schedule(count, innerloopBatchCount: 256);
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

                for (int i = 0; i < headers[index].pairCount; i++) {
                    ref readonly var pair = ref headers[index].GetContactPair(i);

                    if (pair.isCollisionEnter) {
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
                        for (int k = 0; k < pair.contactCount; k++) {
                            ref readonly var contact = ref pair.GetContactPoint(k);
                            averageNormalStay += contact.normal;
                            averagePointStay += contact.position;
                            averageImpulseStay += contact.impulse;
                        }

                        countStay += pair.contactCount;
                    }
                    
                    for (int k = 0; k < pair.contactCount; k++) {
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
                        thisBodyID = headers[index].bodyInstanceID, 
                        otherBodyID = headers[index].otherBodyInstanceID, 
                        normal = averageNormalEnter, 
                        point = averagePointEnter,
                        impulse = averageImpulseEnter,
                    }
                    : default;
                
                contactStayArray[index] = countStay > 0 
                    ? new ContactInfo { 
                        thisBodyID = headers[index].bodyInstanceID, 
                        otherBodyID = headers[index].otherBodyInstanceID, 
                        normal = averageNormalStay, 
                        point = averagePointStay,
                        impulse = averageImpulseStay,
                    }
                    : default;
                
                contactExitArray[index] = countExit > 0 
                    ? new ContactInfo { 
                        thisBodyID = headers[index].bodyInstanceID, 
                        otherBodyID = headers[index].otherBodyInstanceID, 
                        normal = averageNormalExit, 
                        point = averagePointExit,
                        impulse = averageImpulseExit,
                    }
                    : default;
            }
        }
    }
    
}