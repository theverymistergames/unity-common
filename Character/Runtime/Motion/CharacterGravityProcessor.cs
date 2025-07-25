using MisterGames.Actors;
using MisterGames.Character.View;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public sealed class CharacterGravityProcessor : MonoBehaviour, IActorComponent, IMotionProcessor {
    
        [SerializeField] private int _orientationPriority;
        [SerializeField] [Min(0f)] private float _zeroGravityInputSpeed = 0.25f;
        
        private CharacterGravity _characterGravity;
        private CharacterMotionPipeline _motionPipeline;
        private CharacterViewPipeline _viewPipeline;
        
        void IActorComponent.OnAwake(IActor actor) {
            _characterGravity = actor.GetComponent<CharacterGravity>();
            _motionPipeline  = actor.GetComponent<CharacterMotionPipeline>();
            _viewPipeline = actor.GetComponent<CharacterViewPipeline>();
        }

        private void OnEnable() {
            _motionPipeline.AddProcessor(this);
        }

        private void OnDisable() {
            _motionPipeline.RemoveProcessor(this);
        }
        
        bool IMotionProcessor.ProcessOrientation(ref Quaternion orientation, out int priority) {
            priority = _orientationPriority;
            if (!_characterGravity.HasGravity) return false;
            
            var up = _motionPipeline.Up;
            orientation = Quaternion.LookRotation(Vector3.ProjectOnPlane(_viewPipeline.HeadRotation * Vector3.forward, up), up);
            
            return true;
        }

        void IMotionProcessor.ProcessInputSpeed(ref float speed, float dt) {
            if (_characterGravity.HasGravity || !_characterGravity.UseGravity) return;
            
            speed = _zeroGravityInputSpeed;
        }

        void IMotionProcessor.ProcessInputForce(ref Vector3 inputForce, Vector3 desiredVelocity, float dt) {
            
        }
    }
    
}