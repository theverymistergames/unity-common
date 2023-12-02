using System;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Editor.Windows;
using MisterGames.Blueprints.Factory;
using MisterGames.Blueprints.Meta;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Storage {

    public sealed class BlueprintEditorStorage : Common.Data.ScriptableSingleton<BlueprintEditorStorage> {

        [SerializeField] [HideInInspector] private BlueprintEditorData _lastData;

        [Serializable]
        private struct BlueprintEditorData {
            public BlueprintAsset asset;
            public UnityEngine.Object target;
            public NodeId[] path;
        }

        public void OpenLast() {
            var target = _lastData.target != null ? _lastData.target : _lastData.asset;

            if (target is BlueprintAsset asset) {
                BlueprintEditorWindow.Open(asset);
                return;
            }

            if (target is BlueprintRunner runner) {
                var serializedObject = new SerializedObject(target);

                asset = _lastData.asset;
                Blackboard blackboard = null;
                BlueprintMeta meta = null;
                IBlueprintFactory factoryOverride = null;

                if (runner.TryFindSubgraph(_lastData.path, out var data)) {
                    blackboard = data.asset.Blackboard;
                    meta = data.asset.BlueprintMeta;
                    factoryOverride = data.factoryOverride;
                }
                else {
                    if (asset == null) {
                        blackboard = runner.RootBlackboard;
                        meta = runner.RootMetaOverride;
                    }
                    else {
                        blackboard = asset.Blackboard;
                        meta = asset.BlueprintMeta;
                        factoryOverride = runner.RootMetaOverride?.Factory;
                    }
                }

                BlueprintEditorWindow.Open(asset, meta, factoryOverride, blackboard, serializedObject);
            }
        }

        public void NotifyOpenedBlueprintAsset(BlueprintAsset asset, UnityEngine.Object target = null, NodeId[] path = null) {
            _lastData.asset = asset;
            _lastData.target = target;
            _lastData.path = path;

            EditorUtility.SetDirty(this);
        }
    }

}
