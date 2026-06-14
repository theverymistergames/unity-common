using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MisterGames.Actors {
    
    public sealed class ActorLink : Actor, IActorComponent {
        
        public override IActor ParentActor {
            get => _actor?.ParentActor;
            set { if (_actor != null) _actor.ParentActor = value; }
        }

        public override GameObject GameObject => _actor?.GameObject;
        public override Transform Transform => _actor?.Transform;
        public override ActorData DataSO => _actor?.DataSO;
        public override CancellationToken EnableToken => _actor?.EnableToken ?? new CancellationToken(canceled: true);
        public override CancellationToken DestroyToken => _actor?.DestroyToken ?? new CancellationToken(canceled: true);
        
        private IActor _actor;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        void IActorComponent.OnDestroyed(IActor actor) {
            _actor = null;
        }
        
        public override bool TryGetData<T>(out T data) {
            if (_actor?.TryGetData(out data) ?? false) return true;

            data = null;
            return false;
        }

        public override T GetData<T>() {
            return _actor?.GetData<T>();
        }

        public override void SetData(IActorData data) {
            _actor?.SetData(data);
        }

        public override void SetData(IReadOnlyList<IActorData> data) {
            _actor?.SetData(data);
        }

        public override void MuteData<T>() { 
            _actor.MuteData<T>();
        }

        public override void SetDataOverride(object source, IActorData data) {
            _actor?.SetDataOverride(source, data);
        }

        public override void SetDataOverrides(object source, IReadOnlyList<IActorData> data) {
            _actor?.SetDataOverrides(source, data);
        }

        public override void RemoveDataOverride(object source, IActorData data) {
            _actor?.RemoveDataOverride(source, data);
        }

        public override void RemoveDataOverrides(object source, IReadOnlyList<IActorData> data) {
            _actor?.RemoveDataOverrides(source, data);
        }

        public override T GetComponent<T>() where T : class {
            return _actor.GetComponent<T>();
        }

        public override bool TryGetComponent<T>(out T component) {
            return _actor.TryGetComponent(out component);
        }
        
        public override T AddComponent<T>() {
            return _actor?.AddComponent<T>();
        }

        public override T AddComponent<T>(T component) {
            return _actor?.AddComponent(component);
        }

        public override T GetOrAddComponent<T>() {
            return _actor?.GetOrAddComponent<T>();
        }

        public override ComponentCollection<T> GetComponents<T>() {
            return _actor.GetComponents<T>();
        }

        public override void GetComponents<T>(List<T> dest) {
            _actor?.GetComponents(dest);
        }

        public override void DestroyActor(float time = 0) {
            _actor.DestroyActor(time);
        }
    }
    
}