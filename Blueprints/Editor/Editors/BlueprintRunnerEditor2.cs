using System.Text;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Editor.Windows;
using MisterGames.Blueprints.Factory;
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
            var newAsset = EditorGUILayout.ObjectField(
                "Blueprint Asset",
                runner.BlueprintAsset,
                typeof(BlueprintAsset2),
                false
            ) as BlueprintAsset2;

            runner.BlueprintAsset = newAsset;

            if (newAsset != null && GUILayout.Button("Edit")) {
                GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                BlueprintEditorWindow.Open(newAsset);
            }

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }

        private static void DrawRoot(BlueprintRunner2 runner, SerializedObject serializedObject) {
            var metaProperty = serializedObject.FindProperty("_rootMetaOverride");
            var enabledProperty = serializedObject.FindProperty("_isRootOverrideEnabled");
            var asset = runner.BlueprintAsset;

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(!runner.IsRootMetaOverrideEnabled);
            GUILayout.Label("Local Override");
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(enabledProperty, GUIContent.none, GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight));

            if (metaProperty.managedReferenceValue is BlueprintMeta2) {
                if (GUILayout.Button("Edit")) {
                    BlueprintMeta2 meta;
                    Blackboard blackboard;
                    IBlueprintFactory factoryOverride;

                    if (asset == null) {
                        meta = runner.RootMetaOverride;
                        blackboard = runner.RootBlackboard;
                        factoryOverride = null;
                    }
                    else {
                        meta = asset.BlueprintMeta;
                        blackboard = asset.Blackboard;
                        factoryOverride = runner.RootMetaOverride?.Factory;
                    }

                    BlueprintEditorWindow.Open(asset, meta, factoryOverride, blackboard, new SerializedObject(runner));
                }

                if (GUILayout.Button("Delete")) {
                    if (asset == null) runner.RootBlackboard?.Clear();
                    runner.RootMetaOverride = null;
                    runner.IsRootMetaOverrideEnabled = false;
                }
            }
            else {
                if (GUILayout.Button("Create")) {
                    runner.RootMetaOverride = new BlueprintMeta2();
                    runner.IsRootMetaOverrideEnabled = true;
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }

        private static void DrawCompileControls(BlueprintRunner2 runner) {
            if (runner.RuntimeBlueprint == null) {
                if (runner.BlueprintAsset != null || runner.GetRootFactory() != null) {
                    if (GUILayout.Button("Compile & Start Blueprint")) runner.RestartBlueprint();
                    GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                }

                return;
            }

            if (GUILayout.Button("Interrupt Blueprint")) {
                runner.InterruptRuntimeBlueprint();
                GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                return;
            }

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 3f);
            GUILayout.Label("External enter ports", EditorStyles.boldLabel);

            var rootPorts = runner.RuntimeBlueprint.rootPorts;

            foreach ((int sign, var port) in rootPorts) {
                if (port.IsData() || !port.IsInput()) continue;

                GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                if (GUILayout.Button(port.Name)) {
                    var root = runner.RuntimeBlueprint.root;
                    runner.RuntimeBlueprint.Call(new NodeToken(root, root), sign);
                }
            }

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }

        private static void DrawRootBlackboard(SerializedObject serializedObject) {
            var blackboardProperty = serializedObject.FindProperty("_blackboard");
            EditorGUILayout.PropertyField(blackboardProperty);

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }

        private static void DrawSubgraphs(BlueprintRunner2 runner, SerializedObject serializedObject) {
            var subgraphTree = runner.SubgraphTree;

            int count = subgraphTree.Count;
            if (count == 0) return;

            var runnerSerializedObject = new SerializedObject(runner);

            foreach (var root in subgraphTree.Roots) {
                var it = subgraphTree.GetTree(root);
                while (true) {
                    ref var data = ref it.GetValue();
                    var id = it.GetKey();

                    // Horizontal divider
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    // Node header
                    string label;
                    if (it.TryGetParent(out int p)) {
                        var sb = new StringBuilder($"{id.source}.{id.node}");

                        while (true) {
                            var nodeId = subgraphTree.GetKeyAt(p);
                            sb.Insert(0, $"{nodeId.source}.{nodeId.node} => ");
                            if (!subgraphTree.TryGetParent(p, out p)) break;
                        }

                        sb.Insert(0, "Subgraph Node ");
                        label = sb.ToString();
                    }
                    else {
                        label = $"Subgraph Node {id.source}.{id.node}";
                    }
                    GUILayout.Label(label, EditorStyles.boldLabel);

                    GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField("Blueprint Asset", data.asset, typeof(BlueprintAsset), false);
                    EditorGUI.EndDisabledGroup();

                    GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                    if (data.asset != null) {
                        if (GUILayout.Button("Edit")) BlueprintEditorWindow.Open(data.asset);
                        GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                    }

                    // Local override buttons
                    EditorGUILayout.BeginHorizontal();

                    var property = serializedObject.FindProperty($"_subgraphTree._nodes.Array.data[{it.Index}].value");

                    // Enable override field
                    if (property?.FindPropertyRelative("isFactoryOverrideEnabled") is { } enabledProperty) {
                        EditorGUI.BeginDisabledGroup(!enabledProperty.boolValue);
                        GUILayout.Label("Local Override");
                        EditorGUI.EndDisabledGroup();

                        EditorGUILayout.PropertyField(enabledProperty, GUIContent.none, GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight));
                    }

                    if (data.factoryOverride != null) {
                        if (GUILayout.Button("Edit")) {
                            BlueprintEditorWindow.Open(
                                data.asset,
                                factoryOverride: data.factoryOverride,
                                serializedObject: runnerSerializedObject
                            );
                        }
                        if (GUILayout.Button("Delete")) {
                            data.factoryOverride = null;
                            data.isFactoryOverrideEnabled = false;
                        }
                    }
                    else {
                        if (GUILayout.Button("Create")) {
                            data.isFactoryOverrideEnabled = true;
                            data.factoryOverride = new BlueprintFactory();
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                    // Blackboard field
                    if (property?.FindPropertyRelative("blackboard") is { } blackboardProperty) {
                        EditorGUILayout.PropertyField(blackboardProperty);
                    }

                    if (it.IsInvalid() || !it.MovePreOrder()) break;
                }
            }
        }

        private static void FetchSubgraphData(BlueprintRunner2 runner, BlueprintAsset2 oldAsset) {
            var asset = runner.BlueprintAsset;
            var meta = asset != null ? asset.BlueprintMeta : runner.RootMetaOverride;
            var subgraphTree = runner.SubgraphTree;
            bool changed = false;

            if (asset != oldAsset) {
                runner.IsRootMetaOverrideEnabled = false;
                runner.RootMetaOverride = null;
                runner.RootBlackboard?.Clear();

                if (meta == null) subgraphTree.Clear();
                changed = true;
            }

            if (meta != null) {
                if (asset != null) {
                    changed |= runner.RootMetaOverride?.Factory.MatchNodesWith(asset.BlueprintMeta.Factory) ?? false;
                    changed |= runner.RootBlackboard?.MatchPropertiesWith(asset.Blackboard) ?? false;
                }

                subgraphTree.AllowDefragmentation(false);

                var subgraphAssetMap = meta.SubgraphAssetMap;
                subgraphTree.RemoveNodeIf(subgraphAssetMap, (m, id) => !m.ContainsKey(id));

                foreach (var (nodeId, subgraphAsset) in subgraphAssetMap) {
                    FetchBlueprintAndItsSubgraphsRecursively(runner, subgraphAsset, subgraphTree, nodeId);
                }

                subgraphTree.AllowDefragmentation(true);
            }

            if (changed) EditorUtility.SetDirty(runner);
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

            if (subgraphTree.TryGetNode(id, parentIndex, out int i)) {
                ref var data = ref subgraphTree.GetValueAt(i);

                bool changed = false;

                if (data.asset != asset) {
                    data.asset = asset;
                    data.factoryOverride = null;
                    data.blackboard.Clear();
                    data.isFactoryOverrideEnabled = false;

                    changed = true;
                }

                changed |= data.factoryOverride?.MatchNodesWith(data.asset.BlueprintMeta.Factory) ?? false;
                changed |= data.blackboard.MatchPropertiesWith(asset.Blackboard);

                if (changed) EditorUtility.SetDirty(runner);

                parentIndex = i;
            }
            else {
                int j = subgraphTree.GetOrAddNode(id, parentIndex);
                ref var data = ref subgraphTree.GetValueAt(j);

                data.asset = asset;
                data.blackboard = new Blackboard(asset.Blackboard);

                EditorUtility.SetDirty(runner);

                parentIndex = j;
            }

            var subgraphAssetMap = asset.BlueprintMeta.SubgraphAssetMap;
            subgraphTree.RemoveNodeIf(subgraphAssetMap, (m, n) => !m.ContainsKey(n), parentIndex);

            foreach (var (nodeId, subgraphAsset) in subgraphAssetMap) {
                FetchBlueprintAndItsSubgraphsRecursively(runner, subgraphAsset, subgraphTree, nodeId, parentIndex);
            }
        }
    }

}
