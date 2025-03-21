﻿using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Capsule;
using MisterGames.Character.Phys;
using MisterGames.Character.Input;
using MisterGames.Collisions.Core;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public sealed class CharacterMotionGraphPipeline : MonoBehaviour, IActorComponent {

        [SerializeField] private ActorAction _action;

        private IActor _actor;
        private CharacterPosePipeline _pose;
        private CharacterInputPipeline _input;
        private CharacterGroundDetector _groundDetector;
        private CancellationTokenSource _enableCts;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
            _pose = actor.GetComponent<CharacterPosePipeline>();
            _input = actor.GetComponent<CharacterInputPipeline>();
            _groundDetector = actor.GetComponent<CharacterGroundDetector>();
        }

        private void OnEnable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = new CancellationTokenSource();

            _groundDetector.OnContact -= OnStartContactGround;
            _groundDetector.OnContact += OnStartContactGround;

            _groundDetector.OnLostContact -= OnStopContactGround;
            _groundDetector.OnLostContact += OnStopContactGround;

            _pose.OnPoseChanged -= OnPoseChanged;
            _pose.OnPoseChanged += OnPoseChanged;

            _input.OnRunPressed -= OnRunPressed;
            _input.OnRunPressed += OnRunPressed;

            _input.OnRunReleased -= OnRunReleased;
            _input.OnRunReleased += OnRunReleased;

            _action.Apply(_actor, _enableCts.Token).Forget();
        }

        private void OnDisable() {
            _enableCts?.Cancel();
            _enableCts?.Dispose();
            _enableCts = null;

            _groundDetector.OnContact -= OnStartContactGround;
            _groundDetector.OnLostContact -= OnStopContactGround;

            _pose.OnPoseChanged -= OnPoseChanged;

            _input.OnRunPressed -= OnRunPressed;
            _input.OnRunReleased -= OnRunReleased;
        }

        private void OnStartContactGround() {
            _action.Apply(_actor, _enableCts.Token).Forget();
        }

        private void OnStopContactGround() {
            _action.Apply(_actor, _enableCts.Token).Forget();
        }

        private void OnPoseChanged(CharacterPose newPose, CharacterPose oldPose) {
            _action.Apply(_actor, _enableCts.Token).Forget();
        }

        private void OnRunPressed() {
            _action.Apply(_actor, _enableCts.Token).Forget();
        }

        private void OnRunReleased() {
            _action.Apply(_actor, _enableCts.Token).Forget();
        }
    }

}
