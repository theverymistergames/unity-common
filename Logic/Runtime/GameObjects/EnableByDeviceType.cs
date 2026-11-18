using System;
using MisterGames.Common.GameObjects;
using MisterGames.Common.Inputs;
using MisterGames.Common.Lists;
using MisterGames.Common.Service;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Logic.GameObjects {
    
    public sealed class EnableByDeviceType : MonoBehaviour {

        [SerializeField] private Option[] _options;
        
        [Serializable]
        private struct Option {
            public Mode mode;
            public InputDeviceType[] deviceTypes;
            public GamepadType[] gamepadTypes;
            public Object[] objects;
        }

        private enum Mode {
            EnableOnDeviceMatch,
            DisableOnDeviceMatch,
        }

        private IDeviceService _deviceService;
        
        private void Awake() {
            _deviceService = Services.Get<IDeviceService>();
            _deviceService.OnDeviceChanged += OnDeviceChanged;

            ApplyOptions(_deviceService.CurrentDevice, _deviceService.GamepadType);
        }

        private void OnDestroy() {
            _deviceService.OnDeviceChanged -= OnDeviceChanged;
        }

        private void OnDeviceChanged(InputDeviceType deviceType) {
            ApplyOptions(deviceType, _deviceService.GamepadType);
        }

        private void ApplyOptions(InputDeviceType deviceType, GamepadType gamepadType) {
            for (int i = 0; i < _options.Length; i++) {
                ref var option = ref _options[i];
                bool match = option.deviceTypes.Contains(deviceType) &&
                             (option.gamepadTypes is not { Length: > 0 } || option.gamepadTypes.Contains(gamepadType));
                option.objects.SetEnabled(match == option.mode is Mode.EnableOnDeviceMatch);
            }
        }
    }
    
}