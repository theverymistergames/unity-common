using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [CreateAssetMenu(fileName = "Blueprint2", menuName = "MisterGames/Blueprint2")]
    public sealed class BlueprintAsset2 : ScriptableObject {

        [SerializeField] private BlueprintFactoryStorage _factoryStorage;
        [SerializeField] private BlueprintNodeMetaStorage _blueprintMeta;

    }
}
