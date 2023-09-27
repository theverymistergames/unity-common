using System;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [CreateAssetMenu(fileName = "Blueprint2", menuName = "MisterGames/Blueprint2")]
    public sealed class BlueprintAsset2 : ScriptableObject {

        [SerializeField] private BlueprintFactoryStorage dataArray;
        [SerializeField] private BlueprintMeta2 _blueprintMeta;

    }

    [Serializable]
    public sealed class BlueprintMeta2 {

        [SerializeField] private BlueprintNodeMeta2[] _nodeMetaArray;
        [SerializeReference] private IBlueprintFactory[] _nodeDataArrays;

        public void AddNode(BlueprintNodeMeta2 nodeMeta) {

        }

    }
}
