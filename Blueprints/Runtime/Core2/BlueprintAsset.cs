using System;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    /// <summary>
    /// Asset to store blueprint meta data that can be compiled into runtime blueprint instance.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(BlueprintAsset), menuName = "MisterGames/" + nameof(BlueprintAsset))]
    public sealed class BlueprintAsset : ScriptableObject {

        [SerializeField] [HideInInspector]
        private BlueprintMeta _blueprintMeta;

        public BlueprintMeta BlueprintMeta => _blueprintMeta;

        private readonly BlueprintCompiler _blueprintCompiler = new BlueprintCompiler();
        private bool _isCompilerPrepared;

        public RuntimeBlueprint Compile() {
            if (_isCompilerPrepared) return _blueprintCompiler.Compile();

            _blueprintCompiler.Prepare(_blueprintMeta);
            _isCompilerPrepared = true;

            return _blueprintCompiler.Compile();
        }

        private void OnValidate() {
            _blueprintMeta.OnValidate(this);
        }
    }

}
