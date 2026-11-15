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

        public event ContactEvent OnContact = delegate { };
        public delegate void ContactEvent(TriggerEventType evt, EntityId rigidbodyId, int rbMaterial, EntityId colliderId, Vector3 point, Vector3 normal, Vector3 impulse);

        private readonly Dictionary<EntityId, bool> _colliderIdToValidLayerMap = new();
        private NativeHashMap<EntityId, BodyData> _bodyIdToDataMap;
        private NativeList<ContactData> _contacts;

        private void Awake() {
            _bodyIdToDataMap = new NativeHashMap<EntityId, BodyData>(2, Allocator.Persistent);
            _contacts = new NativeList<ContactData>(Allocator.Persistent);
        }

        private void OnEnable() {
            Physics.ContactEvent += OnContactEvent;
        }

        private void OnDisable() {
            Physics.ContactEvent -= OnContactEvent;
        }

        private void OnDestroy() {
            if (_bodyIdToDataMap.IsCreated) _bodyIdToDataMap.Dispose();
            if (_contacts.IsCreated) _contacts.Dispose();
        }

        public void Register(Rigidbody rigidbody, int surfaceMaterial = 0) {
            _bodyIdToDataMap[rigidbody.GetEntityId()] = new BodyData(surfaceMaterial, _layerMask.Contains(rigidbody.gameObject.layer));
        }

        public void Unregister(Rigidbody rigidbody) {
            _bodyIdToDataMap.Remove(rigidbody.GetEntityId());
        }

        private void OnContactEvent(PhysicsScene scene, NativeArray<ContactPairHeader>.ReadOnly headers) {
            _contacts.Clear();

            new CollectContactsJob {
                headers = headers,
                bodyIdToDataMap = _bodyIdToDataMap,
                contacts = _contacts,
            }.Run();

            for (int i = 0; i < _contacts.Length; i++) {
                var contact = _contacts[i];
                if (contact.checkColliderLayer && !IsValidColliderLayer(contact.colliderId)) continue;

                OnContact.Invoke(contact.evt, contact.bodyId, contact.bodyMaterial, contact.colliderId, contact.point, contact.normal, contact.impulse);
            }
        }

        private bool IsValidColliderLayer(EntityId colliderId) {
            if (!_colliderIdToValidLayerMap.TryGetValue(colliderId, out bool isValid) &&
                CollisionUtils.GetColliderByEntityId(colliderId) is var collider && collider != null)
            {
                isValid = _layerMask.Contains(collider.gameObject.layer);
                _colliderIdToValidLayerMap[colliderId] = isValid;
            }

            return isValid;
        }

        private readonly struct BodyData {

            public readonly int material;
            public readonly bool isValidLayer;

            public BodyData(int material, bool isValidLayer) {
                this.material = material;
                this.isValidLayer = isValidLayer;
            }
        }

        private readonly struct ContactData {

            public readonly TriggerEventType evt;
            public readonly EntityId bodyId;
            public readonly int bodyMaterial;
            public readonly EntityId colliderId;
            public readonly bool checkColliderLayer;
            public readonly float3 point;
            public readonly float3 normal;
            public readonly float3 impulse;

            public ContactData(
                TriggerEventType evt, EntityId bodyId, int bodyMaterial, EntityId colliderId, bool checkColliderLayer,
                float3 point, float3 normal, float3 impulse)
            {
                this.evt = evt;
                this.bodyId = bodyId;
                this.bodyMaterial = bodyMaterial;
                this.colliderId = colliderId;
                this.checkColliderLayer = checkColliderLayer;
                this.point = point;
                this.normal = normal;
                this.impulse = impulse;
            }
        }

        private struct ContactSum {

            public EntityId colliderId;
            public EntityId otherColliderId;
            public float3 point;
            public float3 normal;
            public float3 impulse;
            public int contactCount;
            public bool hasPair;

            public void Add(in ContactPair pair) {
                colliderId = pair.colliderEntityId;
                otherColliderId = pair.otherColliderEntityId;
                contactCount += pair.contactCount;
                hasPair = true;

                for (int i = 0; i < pair.contactCount; i++) {
                    ref readonly var contact = ref pair.GetContactPoint(i);
                    point += (float3) contact.position;
                    normal += (float3) contact.normal;
                    impulse += (float3) contact.impulse;
                }
            }
        }

        [BurstCompile]
        private struct CollectContactsJob : IJob {

            [ReadOnly] public NativeArray<ContactPairHeader>.ReadOnly headers;
            [ReadOnly] public NativeHashMap<EntityId, BodyData> bodyIdToDataMap;
            public NativeList<ContactData> contacts;

            public void Execute() {
                for (int i = 0; i < headers.Length; i++) {
                    Collect(i);
                }
            }

            private void Collect(int index) {
                var header = headers[index];

                bool hasThisBody = bodyIdToDataMap.TryGetValue(header.bodyEntityId, out var thisBody);
                bool hasOtherBody = bodyIdToDataMap.TryGetValue(header.otherBodyEntityId, out var otherBody);

                if (!hasThisBody && !hasOtherBody) return;

                var enter = default(ContactSum);
                var stay = default(ContactSum);
                var exit = default(ContactSum);

                for (int i = 0; i < header.pairCount; i++) {
                    ref readonly var pair = ref header.GetContactPair(i);

                    if (pair.isCollisionEnter) enter.Add(in pair);
                    else if (pair.isCollisionStay) stay.Add(in pair);
                    else if (pair.isCollisionExit) exit.Add(in pair);
                }

                if (enter.contactCount > 0) Write(TriggerEventType.Enter, in enter, in header, hasThisBody, in thisBody, hasOtherBody, in otherBody);
                if (stay.contactCount > 0) Write(TriggerEventType.Stay, in stay, in header, hasThisBody, in thisBody, hasOtherBody, in otherBody);
                if (exit.hasPair) Write(TriggerEventType.Exit, in exit, in header, hasThisBody, in thisBody, hasOtherBody, in otherBody);
            }

            private void Write(
                TriggerEventType evt, in ContactSum sum, in ContactPairHeader header,
                bool hasThisBody, in BodyData thisBody, bool hasOtherBody, in BodyData otherBody)
            {
                int count = math.max(sum.contactCount, 1);
                var point = sum.point / count;
                var normal = sum.normal / count;
                var impulse = sum.impulse / count;

                if (hasThisBody && sum.otherColliderId != EntityId.None && (!hasOtherBody || otherBody.isValidLayer)) {
                    contacts.Add(new ContactData(evt, header.bodyEntityId, thisBody.material, sum.otherColliderId, !hasOtherBody, point, normal, impulse));
                }

                if (hasOtherBody && sum.colliderId != EntityId.None && (!hasThisBody || thisBody.isValidLayer)) {
                    contacts.Add(new ContactData(evt, header.otherBodyEntityId, otherBody.material, sum.colliderId, !hasThisBody, point, normal, impulse));
                }
            }
        }
    }

}
