using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Actors {
    
    public sealed class ActorLink : MonoBehaviour, IActorComponent, IActor {

        public GameObject GameObject => _actor?.GameObject;
        public Transform Transform => _actor?.Transform;
        public ActorData DataSO => _actor?.DataSO;
        
        private IActor _actor;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        void IActorComponent.OnDestroyed(IActor actor) {
            _actor = null;
        }
        
        public bool TryGetData<T>(out T data) where T : class, IActorData {
            if (_actor?.TryGetData(out data) ?? false) return true;

            data = default;
            return false;
        }

        public T GetData<T>() where T : class, IActorData {
            return _actor?.GetData<T>();
        }

        public void SetData(IActorData data) {
            _actor?.SetData(data);
        }

        public void SetData(IReadOnlyList<IActorData> data) {
            _actor?.SetData(data);
        }

        public bool RemoveData<T>() where T : class, IActorData {
            return _actor.RemoveData<T>();
        }

        public void SetDataOverride(object source, IActorData data) {
            _actor?.SetDataOverride(source, data);
        }

        public void SetDataOverrides(object source, IReadOnlyList<IActorData> data) {
            _actor?.SetDataOverrides(source, data);
        }

        public void RemoveDataOverride(object source, IActorData data) {
            _actor?.RemoveDataOverride(source, data);
        }

        public void RemoveDataOverrides(object source, IReadOnlyList<IActorData> data) {
            _actor?.RemoveDataOverrides(source, data);
        }

        public new T GetComponent<T>() where T : class {
            return _actor.GetComponent<T>();
        }

        public new bool TryGetComponent<T>(out T component) where T : class {
            return _actor.TryGetComponent(out component);
        }
        
        public T AddComponent<T>() where T : Component {
            return _actor?.AddComponent<T>();
        }

        public T AddComponent<T>(T component) where T : Component {
            return _actor?.AddComponent<T>(component);
        }

        public T GetOrAddComponent<T>() where T : Component {
            return _actor?.GetOrAddComponent<T>();
        }

        public new ComponentCollection<T> GetComponents<T>() where T : class {
            return _actor.GetComponents<T>();
        }

        public new void GetComponents<T>(List<T> dest) where T : class {
            _actor?.GetComponents(dest);
        }

        public void DestroyActor(float time = 0) {
            _actor.DestroyActor(time);
        }
    }
    
}