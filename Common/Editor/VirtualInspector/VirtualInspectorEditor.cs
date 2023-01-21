using UnityEditor;

namespace MisterGames.Common.Editor.VirtualInspector {

    [CustomEditor(typeof(VirtualInspector))]
    public sealed class VirtualInspectorEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            if (serializedObject.targetObject == null) return;

            serializedObject.Update();

            var dataProperty = serializedObject.FindProperty("_data");
            ((VirtualInspector) serializedObject.targetObject).Draw(dataProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }

}
