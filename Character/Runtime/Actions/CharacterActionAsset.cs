using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Actions {

    [CreateAssetMenu(fileName = nameof(CharacterActionAsset), menuName = "MisterGames/Character/" + nameof(CharacterActionAsset))]
    public sealed class CharacterActionAsset : ScriptableObject, ICharacterAction {

        [SerializeReference] [SubclassSelector] private ICharacterAction _action;

        public UniTask Apply(ICharacterAccess context, CancellationToken cancellationToken = default) {
            return _action?.Apply(context, cancellationToken) ?? default;
        }
    }

}
