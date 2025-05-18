using MisterGames.Common.Attributes;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Common.Rendering {
    
    [DefaultExecutionOrder(-100_000)]
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
            Undo.RecordObject(this, "CollectChildMeshRenderers");
            
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
            
            EditorUtility.SetDirty(this);
        }
        
        private void Reset() {
            CollectChildMeshRenderers();
        }
#endif
    }
}