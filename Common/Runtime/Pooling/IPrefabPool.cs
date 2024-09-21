using UnityEngine;

namespace MisterGames.Common.Pooling {
    
    public interface IPrefabPool {
        Transform ActiveSceneRoot { get; }
        Transform PoolRoot { get; }
        
        GameObject Get(GameObject prefab, bool active = true);
        GameObject Get(GameObject prefab, Transform parent, bool active = true, bool worldPositionStays = true);
        GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, bool active = true);
        GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, bool active = true);
        
        T Get<T>(GameObject prefab, bool active = true) where T : Component;
        T Get<T>(GameObject prefab, Transform parent, bool active = true, bool worldPositionStays = true) where T : Component;
        T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation, bool active = true) where T : Component;
        T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, bool active = true) where T : Component;
        
        T Get<T>(T prefab, bool active = true) where T : Component;
        T Get<T>(T prefab, Transform parent, bool active = true, bool worldPositionStays = true) where T : Component;
        T Get<T>(T prefab, Vector3 position, Quaternion rotation, bool active = true) where T : Component;
        T Get<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent, bool active = true) where T : Component;

        void Release(GameObject instance, float duration = 0f);
        void Release(Component component, float duration = 0f);
    }
    
}