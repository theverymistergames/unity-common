using System;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Editor.Windows;
using MisterGames.Blueprints.Meta;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Storage {

    public sealed class BlueprintEditorStorage : Common.Data.ScriptableSingleton<BlueprintEditorStorage> {

        [SerializeField] [HideInInspector] private BlueprintEditorData _lastData;

        [Serializable]
        private struct BlueprintEditorData {
            public BlueprintAsset2 asset;
            public UnityEngine.Object target;
            public NodeId[] path;
        }

        public void OpenLast() {
            var target = _lastData.target != null ? _lastData.target : _lastData.asset;

            if (target is BlueprintAsset2 asset) {
                BlueprintEditorWindow.Open(asset);
                return;
            }

            if (target is BlueprintRunner2 runner) {
                var serializedObject = new SerializedObject(target);
                asset = _lastData.asset;
                BlueprintMeta2 meta;
                Blackboard blackboard;

                if (_lastData.path is { Length: > 0 } path) {
                    runner.TryFindSubgraph(path, out meta, out blackboard);
                }
                else {
                    meta = runner.GetBlueprintMeta();
                    blackboard = asset == null ? runner.GetBlackboard() : asset.Blackboard;
                }

                BlueprintEditorWindow.Open(asset, meta, blackboard, serializedObject);
            }
        }

        public void NotifyOpenedBlueprintAsset(BlueprintAsset2 asset, UnityEngine.Object target = null, NodeId[] path = null) {
            _lastData.asset = asset;
            _lastData.target = target;
            _lastData.path = path;

            EditorUtility.SetDirty(this);
        }
    }

}
