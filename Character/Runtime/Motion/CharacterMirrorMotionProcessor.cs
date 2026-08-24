using MisterGames.Actors;
using MisterGames.Character.Core;
using MisterGames.Character.View;
using MisterGames.Common.Inputs;
using MisterGames.Common.Labels;
using MisterGames.Common.Maths;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public sealed class CharacterMirrorMotionProcessor : MonoBehaviour, IActorComponent, IMotionProcessor, IViewProcessor {

        [SerializeField] private LabelValue _mirrorSetting;
        [SerializeField] private Vector2 _motionMirror = new(-1f, 1f);
        [SerializeField] private Vector2 _viewMirror = new(-1f, 1f);

        private ICharacterSettings _characterSettings;
        private CharacterViewPipeline _viewPipeline;
        private CharacterMotionPipeline _motionPipeline;
        private bool _mirrorEnabled;
        
        void IActorComponent.OnAwake(IActor actor) {
            _characterSettings = Services.Get<ICharacterSettings>();
            _viewPipeline = actor.GetComponent<CharacterViewPipeline>();
            _motionPipeline = actor.GetComponent<CharacterMotionPipeline>();
        }

        private void OnEnable() {
            _viewPipeline.AddProcessor(this);
            _motionPipeline.AddProcessor(this);
            
            _characterSettings.AddValueChangeListener(_mirrorSetting.GetValue(), OnMirrorSettingChange);
            OnMirrorSettingChange(_mirrorSetting.GetValue());
        }

        private void OnDisable() {
            _viewPipeline.RemoveProcessor(this);
            _motionPipeline.RemoveProcessor(this);
            
            _characterSettings.RemoveValueChangeListener(_mirrorSetting.GetValue(), OnMirrorSettingChange);
        }

        private void OnMirrorSettingChange(int key) {
            _mirrorEnabled = _characterSettings.Get(key, defaultValue: false);
        }

        Vector2 IViewProcessor.GetViewSensitivity(InputDeviceType deviceType) {
            return IsMirrorEnabled() ? _viewMirror : new Vector2(1f, 1f);
        }

        void IMotionProcessor.ProcessInputVector(ref Vector2 input) {
            if (!IsMirrorEnabled()) return;
            
            input = input.Multiply(_motionMirror);
        }

        private bool IsMirrorEnabled() {
#if UNITY_EDITOR
            if (_enableMirrorDebug) return true;
#endif            
            
            return _mirrorEnabled;
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _enableMirrorDebug;
#endif
    }
    
}