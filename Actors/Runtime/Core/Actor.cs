using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MisterGames.Actors {
    
    public abstract class Actor : MonoBehaviour, IActor {
        
        public abstract IActor ParentActor { get; set; }
        public abstract GameObject GameObject { get; }
        public abstract Transform Transform { get; }
        public abstract ActorData DataSO { get; }
        public abstract CancellationToken EnableToken { get; }
        public abstract CancellationToken DestroyToken { get; }
        
        public abstract bool TryGetData<T>(out T data) where T : class, IActorData;
        public abstract T GetData<T>() where T : class, IActorData;
        public abstract void SetData(IActorData data);
        public abstract void SetData(IReadOnlyList<IActorData> data);
        public abstract void MuteData<T>() where T : class, IActorData;
        public abstract void SetDataOverride(object source, IActorData data);
        public abstract void SetDataOverrides(object source, IReadOnlyList<IActorData> data);
        public abstract void RemoveDataOverride(object source, IActorData data);
        public abstract void RemoveDataOverrides(object source, IReadOnlyList<IActorData> data);
        
        public new abstract T GetComponent<T>() where T : class;
        public new abstract bool TryGetComponent<T>(out T component) where T : class;
        public abstract T AddComponent<T>() where T : Component;
        public abstract T AddComponent<T>(T component) where T : Component;
        public abstract T GetOrAddComponent<T>() where T : Component;
        public new abstract ComponentCollection<T> GetComponents<T>() where T : class;
        public new abstract void GetComponents<T>(List<T> dest) where T : class;
        
        public abstract void DestroyActor(float time = 0);
    }
    
}