using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;

namespace MisterGames.Character.Actions {

    [Serializable]
    public sealed class CharacterActionAssetReference : ICharacterAction {

        public CharacterActionAsset asset;

        public UniTask Apply(ICharacterAccess context, CancellationToken cancellationToken = default) {
            return asset == null ? default : asset.Apply(context, cancellationToken);
        }
    }

}
