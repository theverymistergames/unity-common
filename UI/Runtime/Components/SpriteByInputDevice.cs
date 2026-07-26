using System;
using MisterGames.Common.Inputs;
using MisterGames.Common.Service;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    public sealed class SpriteByInputDevice : MonoBehaviour {
        
        [SerializeField] private Image _image;
        [SerializeField] private SpriteData[] _spriteData;

        [Serializable]
        private struct SpriteData {
            public InputDeviceType deviceType;
            public Sprite sprite;
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

        private void OnDeviceChanged(InputDeviceType deviceType) {
            _image.sprite = GetSprite(deviceType);
        }

        private Sprite GetSprite(InputDeviceType deviceType) {
            for (int i = 0; i < _spriteData.Length; i++) {
                ref var spriteData = ref _spriteData[i];
                if (spriteData.deviceType != deviceType) continue;
                
                return spriteData.sprite;
            }
            
            return null;
        }
    }
    
}