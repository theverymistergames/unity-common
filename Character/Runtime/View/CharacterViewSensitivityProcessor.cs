using System;
using MisterGames.Actors;
using MisterGames.Character.Core;
using MisterGames.Common.Inputs;
using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Character.View {
    
    public sealed class CharacterViewSensitivityProcessor : MonoBehaviour, IActorComponent, IViewProcessor {

        private CharacterViewPipeline _view;
        private CharacterViewSensitivityData _data;
        private ICharacterSettings _characterSettings;

        private float _mouseSensT = 0.5f;
        private float _gamepadSensT = 0.5f;
        private Vector2 _mouseMul = Vector2.one;
        private Vector2 _gamepadMul = Vector2.one;
        
        void IActorComponent.OnSetData(IActor actor) {
            _data = actor.GetData<CharacterViewSensitivityData>();
        }

        void IActorComponent.OnAwake(IActor actor) {
            _view = actor.GetComponent<CharacterViewPipeline>();
            _characterSettings = Services.Get<ICharacterSettings>();
        }

        private void OnEnable() {
            _view.AddProcessor(this);
            
            _characterSettings.AddValueChangeListener(_data.mouseSensitivityId.GetValue(), OnMouseSensChanged);
            _characterSettings.AddValueChangeListener(_data.gamepadSensitivityId.GetValue(), OnGamepadSensChanged);
            _characterSettings.AddValueChangeListener(_data.mouseInvertYId.GetValue(), OnMouseInvertYChanged);
            _characterSettings.AddValueChangeListener(_data.gamepadInvertYId.GetValue(), OnGamepadInvertYChanged);
        }

        private void OnDisable() {
            _view.RemoveProcessor(this);
            
            _characterSettings.RemoveValueChangeListener(_data.mouseSensitivityId.GetValue(), OnMouseSensChanged);
            _characterSettings.RemoveValueChangeListener(_data.gamepadSensitivityId.GetValue(), OnGamepadSensChanged);
            _characterSettings.RemoveValueChangeListener(_data.mouseInvertYId.GetValue(), OnMouseInvertYChanged);
            _characterSettings.RemoveValueChangeListener(_data.gamepadInvertYId.GetValue(), OnGamepadInvertYChanged);
        }

        private void OnMouseSensChanged(int key) {
            _mouseSensT = _characterSettings.Get(key, 0.5f);
        }

        private void OnGamepadSensChanged(int key) {
            _gamepadSensT = _characterSettings.Get(key, 0.5f);
        }

        private void OnMouseInvertYChanged(int key) {
            _mouseMul = _characterSettings.Get(key, false) ? new Vector2(1f, -1f) : new Vector2(1f, 1f);
        }

        private void OnGamepadInvertYChanged(int key) {
            _gamepadMul = _characterSettings.Get(key, false) ? new Vector2(1f, -1f) : new Vector2(1f, 1f);
        }

        Vector2 IViewProcessor.GetViewSensitivity(InputDeviceType deviceType) {
            return deviceType switch {
                InputDeviceType.KeyboardMouse => _data.mouseSensitivityBase * _mouseMul * Mathf.Lerp(_data.mouseSensitivity0, _data.mouseSensitivity1, _mouseSensT),
                InputDeviceType.Gamepad => _data.gamepadSensitivityBase * _gamepadMul * Mathf.Lerp(_data.gamepadSensitivity0, _data.gamepadSensitivity1, _gamepadSensT),
                _ => throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null)
            };
        }
    }
    
}