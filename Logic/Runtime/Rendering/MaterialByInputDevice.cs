using System;
using System.Collections.Generic;
using MisterGames.Common.Inputs;
using MisterGames.Common.Service;
using UnityEngine;
using DeviceType = MisterGames.Common.Inputs.DeviceType;

namespace MisterGames.Logic.Rendering {
    
    public sealed class MaterialByInputDevice : MonoBehaviour {
        
        [SerializeField] private Renderer _renderer;
        [SerializeField] private MaterialData[] _materialData;

        [Serializable]
        private struct MaterialData {
            public DeviceType deviceType;
            public Material material;
        }

        private readonly Dictionary<DeviceType, Material> _runtimeMaterials = new();

        private void OnDestroy() {
            foreach (var material in _runtimeMaterials.Values) {
                Destroy(material);
            }
        }

        private void OnEnable() {
            if (!Services.TryGet(out IDeviceService service)) return;
            
            service.OnDeviceChanged += OnDeviceChanged;
            OnDeviceChanged(service.CurrentDevice);
        }

        private void OnDisable() {
            if (!Services.TryGet(out IDeviceService service)) return;
            
            service.OnDeviceChanged -= OnDeviceChanged;
        }

        private void OnDeviceChanged(DeviceType deviceType) {
            _renderer.material = GetMaterial(deviceType); 
        }

        private Material GetMaterial(DeviceType deviceType) {
            if (_runtimeMaterials.TryGetValue(deviceType, out var material)) {
                return material;
            }
            
            for (int i = 0; i < _materialData.Length; i++) {
                ref var data = ref _materialData[i];
                if (data.deviceType != deviceType) continue;
                
                material = new Material(data.material);
                _runtimeMaterials[deviceType] = material;
                return material;
            }
            
            return null;
        }

#if UNITY_EDITOR
        private void Reset() {
            _renderer = GetComponent<Renderer>();
        }
#endif
    }
    
}