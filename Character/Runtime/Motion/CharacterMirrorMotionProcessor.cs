using MisterGames.Actors;
using MisterGames.Character.View;
using MisterGames.Common.Inputs;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public sealed class CharacterMirrorMotionProcessor : MonoBehaviour, IActorComponent, IMotionProcessor, IViewProcessor {

        [SerializeField] private Vector2 _motionMirror = new(-1f, 1f);
        [SerializeField] private Vector2 _viewMirror = new(-1f, 1f);
        
        private CharacterViewPipeline _viewPipeline;
        private CharacterMotionPipeline _motionPipeline;
        
        void IActorComponent.OnAwake(IActor actor) {
            _viewPipeline = actor.GetComponent<CharacterViewPipeline>();
            _motionPipeline = actor.GetComponent<CharacterMotionPipeline>();
        }

        private void OnEnable() {
            _viewPipeline.AddProcessor(this);
            _motionPipeline.AddProcessor(this);
        }

        private void OnDisable() {
            _viewPipeline.RemoveProcessor(this);
            _motionPipeline.RemoveProcessor(this);
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
            
            return false;
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _enableMirrorDebug;
#endif
    }
    
}