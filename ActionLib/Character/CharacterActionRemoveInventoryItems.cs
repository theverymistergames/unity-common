using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Character.Inventory;

namespace MisterGames.ActionLib.Character {

    [Serializable]
    public sealed class CharacterActionRemoveInventoryItems : IActorAction {

        public InventoryItemStack[] items;

        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            var inventory = context.GetComponent<ICharacterInventoryPipeline>().Inventory;

            for (int i = 0; i < items.Length; i++) {
                var stack = items[i];
                inventory?.RemoveItems(stack.asset, stack.count);
            }

            return default;
        }
    }

}
