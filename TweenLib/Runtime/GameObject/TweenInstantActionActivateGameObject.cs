using System;
using MisterGames.Tweens.Core;
using UnityEngine;

namespace MisterGames.TweenLib {

    [Serializable]
    public sealed class TweenInstantActionActivateGameObject : ITweenInstantAction {

        public GameObject gameObject;
        public OperationType operation;

        public enum OperationType {
            Enable,
            Disable,
            Toggle,
        }

        public void Initialize(MonoBehaviour owner) { }

        public void DeInitialize() { }

        public void InvokeAction() {
            switch (operation) {
                case OperationType.Enable:
                    gameObject.SetActive(true);
                    break;

                case OperationType.Disable:
                    gameObject.SetActive(false);
                    break;

                case OperationType.Toggle:
                    gameObject.SetActive(!gameObject.activeSelf);
                    break;

                default:
                    throw new NotImplementedException($"Operation type {operation} is not supported for {nameof(TweenInstantActionActivateGameObject)}");
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
        }
    }

}
