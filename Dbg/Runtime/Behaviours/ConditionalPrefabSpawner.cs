using UnityEngine;

namespace MisterGames.Dbg.Behaviours {
    
    public sealed class ConditionalPrefabSpawner : MonoBehaviour {
        
        [SerializeField] private GameObject _prefab;
        [SerializeField] private Transform _parent;
        [SerializeField] private bool _includeInEditor = true;
        [SerializeField] private bool _includeInDevBuild = true;
        [SerializeField] private bool _includeInReleaseBuild = false;

        private GameObject _prefabInstance;
        
        private void Awake() {
            if (NeedInstantiate()) _prefabInstance = Instantiate(_prefab, _parent);
        }

        private void OnDestroy() {
            if (NeedInstantiate()) Destroy(_prefabInstance);
        }

        private bool NeedInstantiate() {
#if UNITY_EDITOR
            return _includeInEditor;
#endif

#if DEVELOPMENT_BUILD
            return _includeInDevBuild;
#endif
            
            return _includeInReleaseBuild;
        }
    }
    
}