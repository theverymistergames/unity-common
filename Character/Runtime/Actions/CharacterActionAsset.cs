using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Actions;
using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using MisterGames.Common.Dependencies;
using UnityEngine;

namespace MisterGames.Character.Actions {

    [CreateAssetMenu(fileName = nameof(CharacterActionAsset), menuName = "MisterGames/Character/" + nameof(CharacterActionAsset))]
    public sealed class CharacterActionAsset : ScriptableObject, ICharacterAction {

        [SerializeReference] [SubclassSelector] private ICharacterAction _action;

        public UniTask Apply(ICharacterAccess characterAccess, object source, CancellationToken cancellationToken = default) {
            return _action?.Apply(characterAccess, source, cancellationToken) ?? default;
        }
    }

}
