using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Common.Rendering {
    
    public sealed class EnableGpuInstancing : MonoBehaviour {
        
        [SerializeField] private MeshRenderer _meshRenderer;
        
        private static MaterialPropertyBlock _sharedMaterialPropertyBlock;
        
        private void Awake() {
#if UNITY_EDITOR
            if (_meshRenderer == null) Debug.LogError($"{nameof(EnableGpuInstancing)}: mesh renderer is null on {gameObject.GetPathInScene()}.");
#endif
            
            _sharedMaterialPropertyBlock ??= new MaterialPropertyBlock();
            _meshRenderer.SetPropertyBlock(_sharedMaterialPropertyBlock);
        }

#if UNITY_EDITOR
        private void Reset() {
            _meshRenderer = GetComponent<MeshRenderer>();
        }
#endif
    }
}