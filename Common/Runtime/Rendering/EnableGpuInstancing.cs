using System;
using MisterGames.Common.Data;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Common.Rendering {
    
    [DefaultExecutionOrder(-100_000)]
    public sealed class EnableGpuInstancing : MonoBehaviour {
        
        [SerializeField] private Renderer _meshRenderer;
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
#if UNITY_EDITOR
            if (_meshRenderer == null) Debug.LogError($"{nameof(EnableGpuInstancing)}: mesh renderer is null on {gameObject.GetPathInScene()}.");
#endif
            
            SetupPropertyBlock();
        }

        private void SetupPropertyBlock() {
#if UNITY_EDITOR
            if (!Application.isPlaying && _meshRenderer == null) return;
#endif
            
            _sharedMaterialPropertyBlock ??= new MaterialPropertyBlock();

            var block = _sharedMaterialPropertyBlock;

            if (HasOverrides()) {
                block = new MaterialPropertyBlock();
                SetupProperties(block);
            }
            
            _meshRenderer.SetPropertyBlock(block);
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

        private void Reset() {
            _meshRenderer = GetComponent<Renderer>();
        }
#endif
    }
}