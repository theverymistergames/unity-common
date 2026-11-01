using MisterGames.Common.Inputs;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Input.Components {
    
    public sealed class GamepadHidColorAnimationBehaviour : MonoBehaviour {

        [SerializeField] private GamepadHidColorAnimationAsset _animation;
        
        private IGamepadHidColors _gamepadHidColors;

        private void Awake() {
            _gamepadHidColors = Services.Get<IDeviceService>().GamepadHidColors;
        }

        private void OnEnable() {
            Play();
        }

        private void OnDisable() {
            Stop();
        }
        
        private void Play() {
            _gamepadHidColors?.PlayAnimation(this, _animation);
        }

        private void Stop() {
            _gamepadHidColors?.StopAnimation(this);
        }
    }
    
}
