using System;
using System.Collections.Generic;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Logic.Rendering {
    
    public sealed class MaterialsActivator : MonoBehaviour {
        
        [SerializeField] private Preset[] _presets;
        
        [Serializable]
        private struct Preset {
            public Mode materialsInRenderers;
            public Mode presetMaterials;
            public Renderer[] renderers;
            public Optional<Material>[] materials;
        }

        private enum Mode {
            UseInstantiatedMaterials,
            UseSharedMaterials,
        }
        
        private readonly List<Material> _materialsBuffer = new();
        private readonly MultiValueDictionary<Renderer, Material> _originalMaterialsMap = new();
        private readonly Dictionary<(int presetIndex, int matertialIndex), Material> _materialsInstancesMap = new();

        private void Awake() {
            FetchOriginalMaterials();
        }

        private void OnDestroy() {
            foreach (var material in _materialsInstancesMap.Values) {
                Destroy(material);
            }
            
            _originalMaterialsMap.Clear();
            _materialsInstancesMap.Clear();
            _materialsBuffer.Clear();
        }

        private void OnEnable() {
            SetupPresets();
        }

        private void OnDisable() {
            RestoreOriginalMaterials();
        }

        private void FetchOriginalMaterials() {
            for (int i = 0; i < _presets?.Length; i++) {
                ref var preset = ref _presets[i];

                for (int j = 0; j < preset.renderers?.Length; j++) {
                    var renderer = preset.renderers[j];
                    _materialsBuffer.Clear();
                    renderer.GetMaterials(_materialsBuffer);

                    for (int k = 0; k < _materialsBuffer.Count; k++) {
                        _originalMaterialsMap.AddValue(renderer, _materialsBuffer[k]);
                    }
                }
            }
        }

        private void RestoreOriginalMaterials() {
            for (int i = 0; i < _presets?.Length; i++) {
                ref var preset = ref _presets[i];

                for (int j = 0; j < preset.renderers?.Length; j++) {
                    var renderer = preset.renderers[j];
                    int count = _originalMaterialsMap.GetCount(renderer);
                    
                    _materialsBuffer.Clear();
                    for (int k = 0; k < count; k++) {
                        _materialsBuffer.Add(_originalMaterialsMap.GetValueAt(renderer, k));
                    }

                    renderer.SetMaterials(_materialsBuffer);
                }
            }
        }
        
        private void SetupPresets() {
            for (int i = 0; i < _presets?.Length; i++) {
                ref var preset = ref _presets[i];
                for (int j = 0; j < preset.renderers?.Length; j++) {
                    SetupMaterials(i, preset.renderers[j], preset.materials, preset.materialsInRenderers, preset.presetMaterials);
                }
            }
        }
        
        private void SetupMaterials(int presetIndex, Renderer renderer, Optional<Material>[] materials, Mode rendererMode, Mode presetMode) {
            _materialsBuffer.Clear();

            switch (rendererMode) {
                case Mode.UseInstantiatedMaterials:
                    renderer.GetMaterials(_materialsBuffer);
                    break;
                
                case Mode.UseSharedMaterials:
                    renderer.GetSharedMaterials(_materialsBuffer);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(rendererMode), rendererMode, null);
            }
            
            for (int i = 0; i < materials?.Length; i++) {
                ref var material = ref materials[i];
                if (_materialsBuffer.Count < i + 1) _materialsBuffer.Add(null);
                if (!material.HasValue) continue;

                Material instance;

                switch (presetMode) {
                    case Mode.UseInstantiatedMaterials:
                        if (!_materialsInstancesMap.TryGetValue((presetIndex, i), out instance)) {
                            instance = new Material(material.Value);
                            _materialsInstancesMap[(presetIndex, i)] = instance;
                        }
                        break;
                    
                    case Mode.UseSharedMaterials:
                        instance = material.Value;
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException(nameof(presetMode), presetMode, null);
                }

                _materialsBuffer[i] = instance;
            }
            
            renderer.SetMaterials(_materialsBuffer);
        }
    }
    
}