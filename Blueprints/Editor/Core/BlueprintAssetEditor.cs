using System;
using UnityEditor;

namespace MisterGames.Blueprints.Editor.Core {

    [CustomEditor(typeof(BlueprintAsset))]
    public sealed class BlueprintAssetEditor : UnityEditor.Editor {

        public event Action<SerializedProperty> OnNodeGUI = delegate {  };

        private int _filterNodeId = -1;

        public void FilterNode(int nodeId) {
            _filterNodeId = nodeId;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            var nodesProperty = serializedObject
                .FindProperty("_blueprintMeta")
                .FindPropertyRelative("_nodesMap")
                .FindPropertyRelative("_entries");

            for (int i = 0; i < nodesProperty.arraySize; i++) {
                var entryProperty = nodesProperty.GetArrayElementAtIndex(i);

                int nodeId = entryProperty.FindPropertyRelative("key").intValue;
                if (_filterNodeId != nodeId) continue;

                var nodeProperty = entryProperty
                    .FindPropertyRelative("value")
                    .FindPropertyRelative("_node");

                OnNodeGUI.Invoke(nodeProperty);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

}
