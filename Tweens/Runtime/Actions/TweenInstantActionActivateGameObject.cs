using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.Tweens.Actions {

    [Serializable]
    public sealed class TweenInstantActionActivateGameObject : ITweenInstantAction {

        [SerializeField] private GameObject _gameObject;
        [SerializeField] private OperationType _operation;

        public GameObject GameObject { get => _gameObject; set => _gameObject = value; }
        public OperationType Operation { get => _operation; set => _operation = value; }

        public enum OperationType {
            Enable,
            Disable,
            Toggle,
        }

        public void Initialize(MonoBehaviour owner) { }

        public void DeInitialize() { }

        public void InvokeAction() {
            switch (_operation) {
                case OperationType.Enable:
                    _gameObject.SetActive(true);
                    break;

                case OperationType.Disable:
                    _gameObject.SetActive(false);
                    break;

                case OperationType.Toggle:
                    _gameObject.SetActive(!_gameObject.activeSelf);
                    break;

                default:
                    throw new NotImplementedException($"Operation type {_operation} is not supported for {nameof(TweenInstantActionActivateGameObject)}");
            }
        }
    }

}
