﻿using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Character.Core2 {

    public sealed class CharacterAccess : MonoBehaviour, ICharacterAccess {

        [SerializeField] private CharacterInput _input;
        [SerializeField] private CameraController _cameraController;

        [SerializeField] private CharacterViewAdapter _viewAdapter;
        [SerializeField] private CharacterMotionAdapter _motionAdapter;

        [SerializeField] private CharacterViewPipeline _viewPipeline;
        [SerializeField] private CharacterMotionPipeline _motionPipeline;

        [SerializeField] private CollisionDetectorBase _hitDetector;
        [SerializeField] private CollisionDetectorBase _ceilingDetector;
        [SerializeField] private CollisionDetectorBase _groundDetector;

        public ICharacterInput Input => _input;
        public CameraController CameraController => _cameraController;

        public ITransformAdapter ViewAdapter => _viewAdapter;
        public ITransformAdapter MotionAdapter => _motionAdapter;

        public ICharacterPipeline ViewPipeline => _viewPipeline;
        public ICharacterPipeline MotionPipeline => _motionPipeline;

        public ICollisionDetector HitDetector => _hitDetector;
        public ICollisionDetector CeilingDetector => _ceilingDetector;
        public ICollisionDetector GroundDetector => _groundDetector;
    }

}
