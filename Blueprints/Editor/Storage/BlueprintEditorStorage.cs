using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Storage {

    public sealed class BlueprintEditorStorage : Common.Data.ScriptableSingleton<BlueprintEditorStorage> {

        [SerializeField] [HideInInspector] private BlueprintAsset2 _lastEditedBlueprintAsset;

        public BlueprintAsset2 LastEditedBlueprintAsset => _lastEditedBlueprintAsset;

        public void NotifyOpenedBlueprintAsset(BlueprintAsset2 blueprintAsset) {
            _lastEditedBlueprintAsset = blueprintAsset;
            EditorUtility.SetDirty(this);
        }
    }

}
