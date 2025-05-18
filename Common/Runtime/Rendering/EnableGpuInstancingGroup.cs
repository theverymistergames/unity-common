using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Common.Rendering {
    
    public sealed class EnableGpuInstancingGroup : MonoBehaviour {
        
        [SerializeField] private MeshRenderer[] _meshRenderers;
        
        private static MaterialPropertyBlock _sharedMaterialPropertyBlock;
        
        private void Awake() {
            _sharedMaterialPropertyBlock ??= new MaterialPropertyBlock();
            
            for (int i = 0; i < _meshRenderers.Length; i++) {
                _meshRenderers[i].SetPropertyBlock(_sharedMaterialPropertyBlock);
            }
        }

#if UNITY_EDITOR
        [Button(mode: ButtonAttribute.Mode.Editor)]
        private void CollectChildMeshRenderers() {
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        }
        
        private void Reset() {
            CollectChildMeshRenderers();
        }
#endif
    }
}