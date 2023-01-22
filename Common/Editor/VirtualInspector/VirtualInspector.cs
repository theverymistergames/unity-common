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
            Action<SerializedProperty> onGUI = null,
            Action<object> onValidate = null
        ) {
            var instance = CreateInstance<VirtualInspector>();

            instance._onGUI = onGUI ?? DrawInline;
            instance._onValidate = onValidate;
            instance._data = data;
            instance.hideFlags = HideFlags.DontSave;

            return instance;
        }

        public void Draw(SerializedProperty serializedProperty) {
            _onGUI.Invoke(serializedProperty);
        }

        private static void DrawInline(SerializedProperty serializedProperty) {
            bool enterChildren = true;
            while (serializedProperty.NextVisible(enterChildren)) {
                enterChildren = false;
                EditorGUILayout.PropertyField(serializedProperty, true);
            }
        }

        private void OnValidate() {
            _onValidate?.Invoke(_data);
        }
    }

}
