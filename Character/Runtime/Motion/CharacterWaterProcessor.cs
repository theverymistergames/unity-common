using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Character.Input;
using MisterGames.Character.View;
using MisterGames.Collisions.Rigidbodies;
using MisterGames.Common;
using MisterGames.Common.Attributes;
using MisterGames.Common.Layers;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public sealed class CharacterWaterProcessor : MonoBehaviour, IActorComponent, IMotionProcessor, IUpdate {
    
        [SerializeField] private CapsuleCollider _rootCollider;
        [SerializeField] private TriggerEmitter _triggerEmitter;
        [SerializeField] private LayerMask _layerMask;
        
        [Header("Submerge")]
        [SerializeField] private int _orientationPriority = 1;
        [SerializeField] [Min(0f)] private float _underwaterSpeed = 1f;
        [SerializeField] [Min(0f)] private float _underwaterForceUp = 5f;
        [SerializeField] private float _lowerPoint;
        [SerializeField] private float _topPoint;
        [SerializeField] [MinMaxSlider(0f, 1f)] private Vector2 _speedWeightRemap = new(0.2f, 0.8f);
        [SerializeField] [MinMaxSlider(0f, 1f)] private Vector2 _forceUpWeightRemap = new(0.2f, 0.8f);
        
        public float SubmergeWeight { get; private set; }
        public bool IsUnderwater => SubmergeWeight >= 1f;
        
        private readonly HashSet<Collider> _colliders = new();
        
        private CharacterInputPipeline _inputPipeline;
        private CharacterMotionPipeline _motionPipeline;
        private CharacterViewPipeline _viewPipeline;
        private CharacterGravity _characterGravity;
        private Transform _rootTransform;
        
        void IActorComponent.OnAwake(IActor actor) {
            _motionPipeline  = actor.GetComponent<CharacterMotionPipeline>();
            _viewPipeline = actor.GetComponent<CharacterViewPipeline>();
            _inputPipeline = actor.GetComponent<CharacterInputPipeline>();
            _characterGravity = actor.GetComponent<CharacterGravity>();
            
            _rootTransform = _rootCollider.transform;
        }

        private void OnEnable() {
            _motionPipeline.AddProcessor(this);
            
            _triggerEmitter.TriggerEnter += TriggerEnter;
            _triggerEmitter.TriggerExit += TriggerExit;
            
            PlayerLoopStage.FixedUpdate.Subscribe(this);
        }

        private void OnDisable() {
            _motionPipeline.RemoveProcessor(this);
            
            _triggerEmitter.TriggerEnter -= TriggerEnter;
            _triggerEmitter.TriggerExit -= TriggerExit;
            
            PlayerLoopStage.FixedUpdate.Unsubscribe(this);
        }

        private void TriggerEnter(Collider collider) {
            if (_layerMask.Contains(collider.gameObject.layer)) _colliders.Add(collider);
        }
        
        private void TriggerExit(Collider collider) {
            _colliders.Remove(collider);
        }

        void IUpdate.OnUpdate(float dt) {
            SubmergeWeight = GetSubmergeWeightMax();
        }

        bool IMotionProcessor.ProcessOrientation(ref Quaternion orientation, out int priority) {
            priority = _orientationPriority;
            if (RemapWeight(SubmergeWeight, _speedWeightRemap) < 1f) return false;
            
            orientation = _viewPipeline.HeadRotation;
            return true;
        }

        void IMotionProcessor.ProcessInputSpeed(ref float speed, float dt) {
            speed = Mathf.Lerp(speed, _underwaterSpeed, RemapWeight(SubmergeWeight, _speedWeightRemap));
        }

        void IMotionProcessor.ProcessInputForce(ref Vector3 inputForce, Vector3 desiredVelocity, float dt) {
            if (_inputPipeline.IsJumpPressed) {
                inputForce -= _characterGravity.GravityDirection * (_underwaterForceUp * RemapWeight(SubmergeWeight, _forceUpWeightRemap));
            }
        }

        private static float RemapWeight(float submergeWeight, Vector2 remap) {
            return InterpolationUtils.Remap01(remap.x, remap.y, submergeWeight);
        }

        private float GetSubmergeWeightMax() {
            if (_colliders.Count == 0) return 0f;

            GetCapsulePoints(out var lowerPoint, out var topPoint);
            float maxLevel = 0f;
            
            foreach (var collider in _colliders) {
                maxLevel = Mathf.Max(maxLevel, GetSubmergeWeight(collider, lowerPoint, topPoint));
            }
            
            return maxLevel;
        }

        private float GetSubmergeWeight(Collider collider, Vector3 lowerPoint, Vector3 upperPoint) {
            var up = _rootTransform.up;

            var closestLowerPoint = lowerPoint + up * Vector3.Dot(collider.ClosestPoint(lowerPoint) - lowerPoint, up);
            var closestUpperPoint = lowerPoint + up * Vector3.Dot(collider.ClosestPoint(upperPoint) - lowerPoint, up);

            return upperPoint == lowerPoint 
                ? Vector3.Dot(upperPoint - closestUpperPoint, up) >= 0f ? 0f : 1f 
                : Mathf.Clamp01((closestUpperPoint - closestLowerPoint).magnitude / (upperPoint - lowerPoint).magnitude);
        }
        
        private void GetCapsulePoints(out Vector3 lowerPoint, out Vector3 topPoint) {
            float height = _rootCollider.height;
            var center = _rootCollider.bounds.center;
            var up = _rootTransform.up;
            
            lowerPoint = center - (height * 0.5f + _lowerPoint) * up;
            topPoint = center + (height * 0.5f + _topPoint) * up;

            if (Vector3.Dot(topPoint - lowerPoint, up) < 0f) {
                lowerPoint = (lowerPoint + topPoint) * 0.5f;
                topPoint = lowerPoint;
            }
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo;
        
        private void OnDrawGizmos() {
            if (!_showDebugInfo || _rootCollider == null) return;

            if (_rootTransform == null) _rootTransform = _rootCollider.transform;
            
            GetCapsulePoints(out var lowerPoint, out var topPoint);
            var rot = _rootCollider.transform.rotation;

            var lowRemap = Vector3.Lerp(lowerPoint, topPoint, _speedWeightRemap.x);
            var topRemap = Vector3.Lerp(lowerPoint, topPoint, _speedWeightRemap.y);

            DebugExt.DrawCircle(topRemap, rot, 0.05f, Color.magenta, gizmo: true);
            DebugExt.DrawCircle(lowRemap, rot, 0.05f, Color.cyan, gizmo: true);

            DebugExt.DrawCrossedPoint(topPoint, rot, Color.magenta, radius: 0f, size: 0.2f, gizmo: true);
            DebugExt.DrawCrossedPoint(lowerPoint, rot, Color.cyan, radius: 0f, size: 0.2f, gizmo: true);
            
            DebugExt.DrawLabel(topPoint + Vector3.up * 0.1f, $"W = {SubmergeWeight:0.00}");
        }
#endif
    }
    
}