using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Character.View {

    public sealed class CharacterHeadAdapter : MonoBehaviour, ITransformAdapter {

        [SerializeField] private Transform _head;

        public Vector3 Position { get => _head.position; set => _head.position = value; }
        public Quaternion Rotation { get => _head.rotation; set => _head.rotation = value; }

        public void Move(Vector3 delta) {
            _head.localPosition += delta;
        }

        public void Rotate(Quaternion delta) {
            _head.localRotation *= delta;
        }
    }

}
