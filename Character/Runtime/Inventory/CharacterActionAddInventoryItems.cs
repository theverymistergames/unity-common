using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.Inventory {

    [Serializable]
    public sealed class CharacterActionAddInventoryItems : IAsyncAction, IDependency {

        public InventoryItemStack[] items;

        private IInventory _inventory;

        public void OnSetupDependencies(IDependencyContainer container) {
            container.CreateBucket(this)
                .Add<CharacterAccess>();
        }

        public void OnResolveDependencies(IDependencyResolver resolver) {
            _inventory = resolver
                .Resolve<ICharacterAccess>()
                .GetPipeline<ICharacterInventoryPipeline>()
                .Inventory;
        }

        public UniTask Apply(object source, CancellationToken cancellationToken = default) {
            for (int i = 0; i < items.Length; i++) {
                var stack = items[i];
                _inventory?.AddItems(stack.asset, stack.data.count);
            }

            return default;
        }
    }

}
