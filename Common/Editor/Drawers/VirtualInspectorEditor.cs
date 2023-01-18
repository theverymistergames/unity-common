using UnityEditor;

namespace MisterGames.Common.Editor.Drawers {

    [CustomEditor(typeof(VirtualInspector))]
    public sealed class VirtualInspectorEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            if (serializedObject.targetObject == null) return;

            serializedObject.Update();

            var onGUI = ((VirtualInspector) serializedObject.targetObject).OnGUI;
            var dataProperty = serializedObject.FindProperty("_data");

            onGUI.Invoke(dataProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }

}
