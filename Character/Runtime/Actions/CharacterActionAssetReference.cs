using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;

namespace MisterGames.Character.Actions {

    [Serializable]
    public sealed class CharacterActionAssetReference : ICharacterAction {

        public CharacterActionAsset action;

        public UniTask Apply(ICharacterAccess context, CancellationToken cancellationToken = default) {
            return action == null ? default : action.Apply(context, cancellationToken);
        }
    }

}
