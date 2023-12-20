﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;

namespace MisterGames.Character.Inventory {

    [Serializable]
    public sealed class CharacterActionRemoveInventoryItems : ICharacterAction {

        public InventoryItemStack[] items;

        public UniTask Apply(ICharacterAccess characterAccess, object source, CancellationToken cancellationToken = default) {
            var inventory = characterAccess.GetPipeline<ICharacterInventoryPipeline>().Inventory;

            for (int i = 0; i < items.Length; i++) {
                var stack = items[i];
                inventory?.RemoveItems(stack.asset, stack.count);
            }

            return default;
        }
    }

}
