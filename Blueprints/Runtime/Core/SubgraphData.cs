using System;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Factory;
using UnityEngine;

namespace MisterGames.Blueprints {

    [Serializable]
    public struct SubgraphData {


        public BlueprintAsset2 asset;
        public Blackboard blackboard;
        public bool isFactoryOverrideEnabled;
        [SerializeReference] public IBlueprintFactory factoryOverride;

        public override string ToString() {
            return $"{nameof(SubgraphData)}(asset {asset}, blackboard {blackboard}, factory override {factoryOverride}, enabled {isFactoryOverrideEnabled})";
        }
    }

}
