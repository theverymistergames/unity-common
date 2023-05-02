using UnityEditor;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Core {

    public sealed class BlueprintEditorStorage : Common.Data.ScriptableSingleton<BlueprintEditorStorage> {

        [SerializeField] [HideInInspector] private BlueprintAsset _lastEditedBlueprintAsset;

        public BlueprintAsset LastEditedBlueprintAsset => _lastEditedBlueprintAsset;

        public void NotifyOpenedBlueprintAsset(BlueprintAsset blueprintAsset) {
            _lastEditedBlueprintAsset = blueprintAsset;
            EditorUtility.SetDirty(this);
        }
    }

}
