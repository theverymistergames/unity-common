﻿using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Character.View {

    public class CharacterHeadAdapter : MonoBehaviour, ITransformAdapter {

        [SerializeField] private CameraController _cameraController;

        public Vector3 Position {
            get => _cameraController.Position;
            set => _cameraController.SetPositionOffset(this, value - _cameraController.Position);
        }

        public Quaternion Rotation {
            get => _cameraController.Rotation;
            set => _cameraController.SetRotationOffset(this, value * Quaternion.Inverse(_cameraController.Rotation));
        }

        private void OnEnable() {
            _cameraController.RegisterInteractor(this);
        }

        private void OnDisable() {
            _cameraController.UnregisterInteractor(this);
        }

        public void Move(Vector3 delta) {
            _cameraController.AddPositionOffset(this, delta);
        }

        public void Rotate(Quaternion delta) {
            _cameraController.AddRotationOffset(this, delta);
        }
    }

}
