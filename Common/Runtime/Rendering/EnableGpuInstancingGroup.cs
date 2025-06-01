using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Common.GameObjects;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.Common.Rendering {
    
    [DefaultExecutionOrder(-100_001)]
    public sealed class EnableGpuInstancingGroup : MonoBehaviour {
        
        [SerializeField] private Renderer[] _meshRenderers;
        
        [Header("Apply for all renderers")]
        [SerializeField] private ColorProperty[] _colors;
        [SerializeField] private GenericProperty<float>[] _floats;
        [SerializeField] private GenericProperty<Vector4>[] _vectors;
        
        [Serializable]
        private struct ColorProperty {
            public ShaderHashId property;
            [ColorUsage(showAlpha: true, hdr: true)] public Color value;
        }
        
        [Serializable]
        private struct GenericProperty<T> {
            public ShaderHashId property;
            public T value;
        }
        
        private static MaterialPropertyBlock _sharedMaterialPropertyBlock;
        
        private void Awake() {
            SetupPropertyBlock();
        }

        private void SetupPropertyBlock() {
            _sharedMaterialPropertyBlock ??= new MaterialPropertyBlock();
            
            var block = _sharedMaterialPropertyBlock;

            if (HasOverrides()) {
                block = new MaterialPropertyBlock();
                SetupProperties(block);
            }
            
            for (int i = 0; i < _meshRenderers.Length; i++) {
#if UNITY_EDITOR
                if (!Application.isPlaying && _meshRenderers[i] == null) continue;
                if (Application.isPlaying && _meshRenderers[i] == null) {
                    Debug.LogError($"{nameof(EnableGpuInstancingGroup)}: mesh renderer #{i} is null on {gameObject.GetPathInScene()}.");
                    continue;
                }
#endif
                
                _meshRenderers[i].SetPropertyBlock(block);
            }
        }
        
        private bool HasOverrides() {
            return _colors?.Length > 0 ||
                   _floats?.Length > 0 ||
                   _vectors?.Length > 0;
        }
        
        private void SetupProperties(MaterialPropertyBlock block) {
            for (int i = 0; i < _colors?.Length; i++) {
                ref var data = ref _colors[i];
                block.SetColor(data.property, data.value);
            }
             
            for (int i = 0; i < _floats?.Length; i++) {
                ref var data = ref _floats[i];
                block.SetFloat(data.property, data.value);
            }
            
            for (int i = 0; i < _vectors?.Length; i++) {
                ref var data = ref _vectors[i];
                block.SetVector(data.property, data.value);
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            SetupPropertyBlock();
        }

        [Button(mode: ButtonAttribute.Mode.Editor)]
        private void CollectChildMeshRenderers() {
            Undo.RecordObject(this, "CollectChildMeshRenderers");
            
            _meshRenderers = GetComponentsInChildren<Renderer>();
            
            EditorUtility.SetDirty(this);
        }
        
        private void Reset() {
            CollectChildMeshRenderers();
        }
#endif
    }
}