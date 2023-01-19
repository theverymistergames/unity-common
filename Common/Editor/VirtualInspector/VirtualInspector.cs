using System;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.VirtualInspector {

    public sealed class VirtualInspector : ScriptableObject {

        [SerializeReference] private object _data;

        public Action<SerializedProperty> OnGUI { get; private set; }
        private Action<object> _onValidate;

        public static VirtualInspector Create(
            object data,
            Action<SerializedProperty> onGUI = null,
            Action<object> onValidate = null
        ) {
            var instance = CreateInstance<VirtualInspector>();

            instance.OnGUI = onGUI ?? DrawInline;
            instance._onValidate = onValidate;
            instance._data = data;

            return instance;
        }

        private static void DrawInline(SerializedProperty serializedProperty) {
            foreach (object child in serializedProperty) {
                EditorGUILayout.PropertyField((SerializedProperty) child, true);
            }
        }

        private void OnValidate() {
            _onValidate?.Invoke(_data);
        }
    }

}
