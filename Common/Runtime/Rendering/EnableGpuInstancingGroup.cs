using System;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using MisterGames.Common.GameObjects;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
#endif

namespace MisterGames.Common.Rendering {
    
    [DefaultExecutionOrder(-100_001)]
    public sealed class EnableGpuInstancingGroup : MonoBehaviour {
        
        [FormerlySerializedAs("_meshRenderers")]
        [SerializeField] private Renderer[] _renderers;
        
        [Header("Apply for all renderers")]
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

            switch (_applyMode) {
                case ApplyMode.OnlyFirstMaterial:
                    for (int i = 0; i < _renderers.Length; i++) {
#if UNITY_EDITOR
                        if (!Application.isPlaying && _renderers[i] == null) continue;
                        if (Application.isPlaying && _renderers[i] == null) {
                            Debug.LogError($"{nameof(EnableGpuInstancingGroup)}: renderer #{i} is null on {gameObject.GetPathInScene()}.");
                            continue;
                        }
#endif
                        
                        var renderer = _renderers[i];
                        renderer.SetPropertyBlock(block, 0);
                        
                        int count = renderer.sharedMaterials.Length;
                        for (int j = 1; j < count; j++) {
                            renderer.SetPropertyBlock(_sharedMaterialPropertyBlock, j);
                        }
                    }
                    break;
                
                case ApplyMode.AllMaterials:
                    for (int i = 0; i < _renderers.Length; i++) {
#if UNITY_EDITOR
                        if (!Application.isPlaying && _renderers[i] == null) continue;
                        if (Application.isPlaying && _renderers[i] == null) {
                            Debug.LogError($"{nameof(EnableGpuInstancingGroup)}: renderer #{i} is null on {gameObject.GetPathInScene()}.");
                            continue;
                        }
#endif
                        var renderer = _renderers[i];
                        int count = renderer.sharedMaterials.Length;
                        
                        for (int j = 0; j < count; j++) {
                            renderer.SetPropertyBlock(block, j);
                        }
                    }
                    break;
                
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
        [Header("Collect")]
        [SerializeField] private bool _excludeRenderersFromOtherGroups = true;
        
        private void OnValidate() {
            SetupPropertyBlock();
        }

        [Button(mode: ButtonAttribute.Mode.Editor)]
        private void CollectChildRenderers() {
            Undo.RecordObject(this, "CollectChildRenderers");

            HashSet<int> excluded = null;

            if (_excludeRenderersFromOtherGroups) {
                excluded = UnityEngine.Pool.HashSetPool<int>.Get();
                var groups = GetComponentsInChildren<EnableGpuInstancingGroup>()
                    .Where(x => x != this)
                    .ToArray();

                for (int i = 0; i < groups.Length; i++) {
                    var renderers = groups[i]._renderers;
                    for (int j = 0; j < renderers.Length; j++) {
                        if (renderers[j] != null) excluded.Add(renderers[j].GetInstanceID());
                    }
                }
            }

            _renderers = excluded != null
                ? GetComponentsInChildren<Renderer>().Where(x => !excluded.Contains(x.GetInstanceID())).ToArray()
                : GetComponentsInChildren<Renderer>();

            EditorUtility.SetDirty(this);
        }
        
        private void Reset() {
            CollectChildRenderers();
        }
#endif
    }
}