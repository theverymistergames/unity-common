using System;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Factory;
using UnityEngine;

namespace MisterGames.Blueprints {

    [Serializable]
    public struct SubgraphData {

        public BlueprintAsset2 asset;
        public Blackboard blackboard;

        [SerializeReference] public IBlueprintFactory factoryOverride;
    }

}
