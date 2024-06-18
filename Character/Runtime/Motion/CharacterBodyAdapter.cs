using MisterGames.Actors;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Character.Motion {

    public sealed class CharacterBodyAdapter : MonoBehaviour, IActorComponent, ITransformAdapter {
        
        public Vector3 Position { get => _body.position; set => _body.position = value; }
        public Vector3 LocalPosition { get => _body.localPosition; set => _body.localPosition = value; }

        public Quaternion Rotation { get => _body.rotation; set => _body.rotation = value; }
        public Quaternion LocalRotation { get => _body.localRotation; set => _body.localRotation = value; }

        private Rigidbody _rigidbody;
        private Transform _body;

        void IActorComponent.OnAwake(IActor actor) {
            _rigidbody = actor.GetComponent<Rigidbody>();
            _body = _rigidbody.transform;
        }

        public void Move(Vector3 delta) {
            _rigidbody.MovePosition(_rigidbody.position + delta);
        }
        
        public void Rotate(Quaternion delta) {
            _rigidbody.MoveRotation(_rigidbody.rotation * delta);
        }
    }

}