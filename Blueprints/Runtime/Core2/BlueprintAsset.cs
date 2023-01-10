using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [CreateAssetMenu(fileName = nameof(BlueprintAsset), menuName = "MisterGames/" + nameof(BlueprintAsset))]
    public sealed class BlueprintAsset : ScriptableObject {

        [SerializeField] [HideInInspector] private Blueprint _blueprint;
        public Blueprint Blueprint => _blueprint;

    }

}
