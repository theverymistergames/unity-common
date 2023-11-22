using System;
using MisterGames.Blackboards.Core;
using MisterGames.Blueprints.Meta;
using UnityEngine;

namespace MisterGames.Blueprints {

    [Serializable]
    public struct SubgraphData {

        public BlueprintAsset2 asset;
        public Blackboard blackboard;

        [SerializeReference] public BlueprintMeta2 metaOverride;
    }

}
