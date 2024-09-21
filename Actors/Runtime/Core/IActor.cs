using System.Collections.Generic;
using UnityEngine;

namespace MisterGames.Actors {
    
    public interface IActor {
        
        GameObject GameObject { get; }
        Transform Transform { get; }
        ActorData DataSO { get; }
        
        bool TryGetData<T>(out T data) where T : class, IActorData;
        T GetData<T>() where T : class, IActorData;
        
        void SetData(IActorData data);
        void SetData(IReadOnlyList<IActorData> data);
        bool RemoveData<T>() where T : class, IActorData;
        
        void SetDataOverride(object source, IActorData data);
        void SetDataOverrides(object source, IReadOnlyList<IActorData> data);
        void RemoveDataOverride(object source, IActorData data);
        void RemoveDataOverrides(object source, IReadOnlyList<IActorData> data);
        
        T GetComponent<T>() where T : class;
        bool TryGetComponent<T>(out T component) where T : class;
        T AddComponent<T>() where T : Component;
        T AddComponent<T>(T component) where T : Component;
        T GetOrAddComponent<T>() where T : Component;
        ComponentCollection<T> GetComponents<T>() where T : class;
        void GetComponents<T>(List<T> dest) where T : class;
        
        void DestroyActor(float time = 0f);
    }
    
}