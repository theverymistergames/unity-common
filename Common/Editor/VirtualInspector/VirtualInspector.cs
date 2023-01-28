using System;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.VirtualInspector {

    public sealed class VirtualInspector : ScriptableObject {

        [SerializeReference] private object _data;

        private Action<SerializedProperty> _onGUI;
        private Action<object> _onValidate;

        public static VirtualInspector Create(
            object data,
            Action<SerializedProperty> onGUI,
            Action<object> onValidate = null
        ) {
            var instance = CreateInstance<VirtualInspector>();

            instance._onGUI = onGUI;
            instance._onValidate = onValidate;
            instance._data = data;
            instance.hideFlags = HideFlags.DontSave;

            return instance;
        }

        internal void Draw(SerializedProperty serializedProperty) {
            _onGUI.Invoke(serializedProperty);
        }

        private void OnValidate() {
            _onValidate?.Invoke(_data);
        }
    }

}
