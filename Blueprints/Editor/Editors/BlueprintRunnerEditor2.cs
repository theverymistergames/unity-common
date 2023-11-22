using System.Text;
using log4net;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Editor.Windows;
using MisterGames.Blueprints.Meta;
using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Editors {

    [CustomEditor(typeof(BlueprintRunner2))]
    public sealed class BlueprintRunnerEditor2 : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            if (target is not BlueprintRunner2 runner) return;

            serializedObject.Update();

            var asset = runner.BlueprintAsset;

            DrawAssetPicker(runner);
            FetchSubgraphData(runner, asset);

            DrawRoot(runner, serializedObject);
            DrawCompileControls(runner);
            DrawRootBlackboard(serializedObject);
            DrawSubgraphs(runner, serializedObject);

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawAssetPicker(BlueprintRunner2 runner) {
            var value = EditorGUILayout.ObjectField(
                "Blueprint Asset",
                runner.BlueprintAsset,
                typeof(BlueprintAsset2),
                false
            );

            runner.BlueprintAsset = value as BlueprintAsset2;

            if (runner.BlueprintAsset != null && GUILayout.Button("Edit")) {
                BlueprintEditorWindow.Open(runner.BlueprintAsset, runner.GetBlueprintMeta(), runner.GetBlackboard());
            }
        }

        private static void DrawCompileControls(BlueprintRunner2 runner) {
            if (runner.RuntimeBlueprint == null) {
                if (runner.GetBlueprintMeta() != null && GUILayout.Button("Compile & Start Blueprint")) runner.RestartBlueprint();
                return;
            }

            if (GUILayout.Button("Interrupt Blueprint")) {
                runner.InterruptRuntimeBlueprint();
                return;
            }

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 2f);
            GUILayout.Label("External enter ports", EditorStyles.boldLabel);

            var rootPorts = runner.RuntimeBlueprint.rootPorts;

            foreach ((int sign, var port) in rootPorts) {
                if (port.IsData() || !port.IsInput()) continue;

                if (GUILayout.Button(port.Name)) {
                    var root = runner.RuntimeBlueprint.root;
                    runner.RuntimeBlueprint.Call(new NodeToken(root, root), sign);
                }
            }
        }

        private static void DrawRoot(BlueprintRunner2 runner, SerializedObject serializedObject) {
            var sourceAssetProperty = serializedObject.FindProperty("_blueprintAsset");
            var metaProperty = serializedObject.FindProperty("_rootOverrideMeta");
            var asset = sourceAssetProperty.objectReferenceValue as BlueprintAsset2;

            // Local override buttons
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Local Override");
            if (metaProperty.managedReferenceValue is BlueprintMeta2) {
                if (GUILayout.Button("Edit")) {
                    var meta = runner.GetBlueprintMeta();
                    var blackboard = asset != null ? asset.Blackboard : runner.GetBlackboard();
                    BlueprintEditorWindow.Open(asset, meta, blackboard, new SerializedObject(runner));
                }
                if (GUILayout.Button("Delete")) {
                    if (asset == null) runner.GetBlackboard()?.Clear();
                    metaProperty.managedReferenceValue = null;
                }
            }
            else {
                if (GUILayout.Button("Create")) {
                    metaProperty.managedReferenceValue = new BlueprintMeta2();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawRootBlackboard(SerializedObject serializedObject) {
            var blackboardProperty = serializedObject.FindProperty("_blackboard");
            EditorGUILayout.PropertyField(blackboardProperty);
        }

        private static void DrawSubgraphs(BlueprintRunner2 runner, SerializedObject serializedObject) {
            var subgraphTree = runner.SubgraphTree;

            int count = subgraphTree.Count;
            if (count == 0) return;

            var runnerSerializedObject = new SerializedObject(runner);

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 6f);

            foreach (var root in subgraphTree.Roots) {
                var it = subgraphTree.GetTree(root);
                while (true) {
                    ref var data = ref it.GetValue();
                    var id = it.GetKey();

                    // Node header
                    string label;
                    if (it.TryGetParent(out int p)) {
                        var sb = new StringBuilder($"{id.source}.{id.node}");

                        while (true) {
                            var nodeId = subgraphTree.GetKeyAt(p);
                            sb.Insert(0, $"{nodeId.source}.{nodeId.node} => ");
                            if (!subgraphTree.TryGetParent(p, out p)) break;
                        }

                        sb.Insert(0, "Node ");
                        label = sb.ToString();
                    }
                    else {
                        label = $"Node {id.source}.{id.node}";
                    }
                    GUILayout.Label(label);

                    // Read-only asset field next to node header
                    var rect = GUILayoutUtility.GetLastRect();
                    var headerRect = new Rect(
                        rect.x + EditorGUIUtility.labelWidth,
                        rect.y,
                        rect.width - EditorGUIUtility.labelWidth,
                        EditorGUIUtility.singleLineHeight
                    );
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.ObjectField(headerRect, "", data.asset, typeof(BlueprintAsset), false);
                    EditorGUI.EndDisabledGroup();

                    GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                    // Local override buttons
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Local Override");
                    if (data.metaOverride != null) {
                        if (GUILayout.Button("Edit")) {
                            BlueprintEditorWindow.Open(data.asset, data.metaOverride, serializedObject: runnerSerializedObject);
                        }
                        if (GUILayout.Button("Delete")) {
                            data.metaOverride = null;
                        }
                    }
                    else {
                        if (GUILayout.Button("Create")) {
                            data.metaOverride = new BlueprintMeta2();
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    // Blackboard field
                    var blackboardProperty = serializedObject.FindProperty($"_subgraphTree._nodes.Array.data[{it.Index}].value.blackboard");
                    EditorGUILayout.PropertyField(blackboardProperty);

                    if (!it.MovePreOrder()) break;

                    // Space between override entries
                    GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 6f);
                }
            }
        }

        private static void FetchSubgraphData(BlueprintRunner2 runner, BlueprintAsset2 oldAsset) {
            var meta = runner.GetBlueprintMeta();

            if (meta == null) {
                runner.SubgraphTree.Clear();
                return;
            }

            var asset = runner.BlueprintAsset;
            bool changed = false;

            if (asset != null) changed |= runner.GetBlackboard().MatchPropertiesWith(asset.Blackboard);

            if (asset != oldAsset) {
                runner.RootOverrideMeta = null;
                changed = true;
            }

            if (changed) EditorUtility.SetDirty(runner);

            var subgraphTree = runner.SubgraphTree;
            subgraphTree.AllowDefragmentation(false);

            var subgraphAssetMap = meta.SubgraphAssetMap;

            foreach (var nodeId in subgraphTree.Roots) {
                if (!subgraphAssetMap.ContainsKey(nodeId)) subgraphTree.RemoveNode(nodeId);
            }

            foreach (var (nodeId, subgraphAsset) in subgraphAssetMap) {
                FetchBlueprintAndItsSubgraphsRecursively(runner, subgraphAsset, subgraphTree, nodeId);
            }

            subgraphTree.AllowDefragmentation(true);
        }

        private static void FetchBlueprintAndItsSubgraphsRecursively(
            BlueprintRunner2 runner,
            BlueprintAsset2 asset,
            TreeMap<NodeId, SubgraphData> subgraphTree,
            NodeId id = default,
            int parentIndex = -1
        ) {
            if (asset == null) {
                subgraphTree.RemoveNode(id, parentIndex);
                return;
            }

            if (subgraphTree.TryGetNode(id, parentIndex, out parentIndex)) {
                ref var data = ref subgraphTree.GetValueAt(parentIndex);

                bool changed = false;

                if (data.asset != asset) {
                    data.asset = asset;
                    data.metaOverride = null;

                    changed = true;
                }

                changed |= data.blackboard.MatchPropertiesWith(asset.Blackboard);

                if (changed) EditorUtility.SetDirty(runner);
            }
            else {
                parentIndex = subgraphTree.GetOrAddNode(id, parentIndex);
                ref var data = ref subgraphTree.GetValueAt(parentIndex);

                data.asset = asset;
                data.blackboard = new Blackboard(asset.Blackboard);

                EditorUtility.SetDirty(runner);
            }

            var subgraphAssetMap = asset.BlueprintMeta.SubgraphAssetMap;

            for (int i = subgraphTree.GetChild(parentIndex); i >= 0; i = subgraphTree.GetNext(i)) {
                var nodeId = subgraphTree.GetKeyAt(i);
                if (!subgraphAssetMap.ContainsKey(nodeId)) subgraphTree.RemoveNode(nodeId, parentIndex);
            }

            foreach (var (nodeId, subgraphAsset) in subgraphAssetMap) {
                FetchBlueprintAndItsSubgraphsRecursively(runner, subgraphAsset, subgraphTree, nodeId, parentIndex);
            }
        }
    }

}
