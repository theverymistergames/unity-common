using UnityEngine;

namespace MisterGames.Actors {
    
    public interface IActor {
        GameObject GameObject { get; }
        Transform Transform { get; }
        
        T GetData<T>() where T : class, IActorData;
        void SetData<T>(T data) where T : class, IActorData;
        bool RemoveData<T>() where T : class, IActorData;
        
        T GetComponent<T>() where T : class;
        bool TryGetComponent<T>(out T component) where T : class;
        ComponentCollection<T> GetComponents<T>() where T : class;

        void DestroyActor(float time = 0f);
    }
    
}