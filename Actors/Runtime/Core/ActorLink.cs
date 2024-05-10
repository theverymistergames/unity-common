using UnityEngine;

namespace MisterGames.Actors {
    
    public sealed class ActorLink : MonoBehaviour, IActorComponent, IActor {

        public GameObject GameObject => _actor.GameObject;
        public Transform Transform => _actor.Transform;

        private IActor _actor;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        void IActorComponent.OnTerminate(IActor actor) {
            _actor = null;
        }

        public T GetData<T>() where T : class, IActorData {
            return _actor.GetData<T>();
        }

        public void SetData<T>(T data) where T : class, IActorData {
            _actor.SetData(data);
        }

        public bool RemoveData<T>() where T : class, IActorData {
            return _actor.RemoveData<T>();
        }

        public new T GetComponent<T>() where T : class {
            return _actor.GetComponent<T>();
        }

        public new bool TryGetComponent<T>(out T component) where T : class {
            return _actor.TryGetComponent(out component);
        }

        public new ComponentCollection<T> GetComponents<T>() where T : class {
            return _actor.GetComponents<T>();
        }

        public void DestroyActor(float time = 0) {
            _actor.DestroyActor(time);
        }
    }
    
}