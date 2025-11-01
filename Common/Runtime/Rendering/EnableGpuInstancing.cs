using System;
using MisterGames.Common.Data;
using MisterGames.Common.GameObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace MisterGames.Common.Rendering {
    
    [DefaultExecutionOrder(-100_000)]
    public sealed class EnableGpuInstancing : MonoBehaviour {
        
        [FormerlySerializedAs("_meshRenderer")] 
        [SerializeField] private Renderer _renderer;
        [SerializeField] private ApplyMode _applyMode = ApplyMode.OnlyFirstMaterial;
        [SerializeField] private ColorProperty[] _colors;
        [SerializeField] private GenericProperty<float>[] _floats;
        [SerializeField] private GenericProperty<Vector4>[] _vectors;
        
        private enum ApplyMode {
            OnlyFirstMaterial,
            AllMaterials,
        }
        
        [Serializable]
        private struct ColorProperty {
            public ShaderHashId property;
            [ColorUsage(showAlpha: true, hdr: true)] public Color value;
            public ColorMode mode;
        }
        
        [Serializable]
        private struct GenericProperty<T> {
            public ShaderHashId property;
            public T value;
        }

        private enum ColorMode {
            Color,
            Vector4,
        }
        
        private static MaterialPropertyBlock _sharedMaterialPropertyBlock;
        
        private void Awake() {
#if UNITY_EDITOR
            if (_renderer == null) Debug.LogError($"{nameof(EnableGpuInstancing)}: mesh renderer is null on {gameObject.GetPathInScene()}.");
#endif
            
            SetupPropertyBlock();
        }

        private void SetupPropertyBlock() {
#if UNITY_EDITOR
            if (!Application.isPlaying && _renderer == null) return;
            
            if (Application.isPlaying && _renderer == null) {
                Debug.LogError($"{nameof(EnableGpuInstancingGroup)}: renderer is null on {gameObject.GetPathInScene()}.");
                return;
            }
#endif
            
            _sharedMaterialPropertyBlock ??= new MaterialPropertyBlock();

            var block = _sharedMaterialPropertyBlock;

            if (HasOverrides()) {
                block = new MaterialPropertyBlock();
                SetupProperties(block);
            }

            switch (_applyMode) {
                case ApplyMode.OnlyFirstMaterial: {
                    _renderer.SetPropertyBlock(block, 0);

                    int count = _renderer.sharedMaterials.Length;
                    for (int j = 1; j < count; j++) {
                        _renderer.SetPropertyBlock(_sharedMaterialPropertyBlock, j);
                    }

                    break;
                }

                case ApplyMode.AllMaterials: {
                    int count = _renderer.sharedMaterials.Length;
                    for (int j = 0; j < count; j++) {
                        _renderer.SetPropertyBlock(block, j);
                    }
                    break; 
                }

                default:
                    throw new ArgumentOutOfRangeException();
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

                switch (data.mode) {
                    case ColorMode.Color:
                        block.SetColor(data.property, data.value);
                        break;
                    
                    case ColorMode.Vector4:
                        block.SetVector(data.property, data.value);
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
            _renderer = GetComponent<Renderer>();
        }
#endif
    }
}