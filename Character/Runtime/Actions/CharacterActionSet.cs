using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Actions {

    [CreateAssetMenu(fileName = nameof(CharacterActionSet), menuName = "MisterGames/Character/" + nameof(CharacterActionSet))]
    public sealed class CharacterActionSet :
        ScriptableObject,
        ICharacterAsyncAction,
        ICharacterAccessInitializable
    {
        [SerializeReference] [SubclassSelector] private ICharacterAction _action;
        [SerializeReference] [SubclassSelector] private ICharacterAsyncAction _asyncAction;

        public void Initialize(ICharacterAccess characterAccess) {
            if (_action is ICharacterAccessInitializable action) action.Initialize(characterAccess);
            if (_asyncAction is ICharacterAccessInitializable asyncAction) asyncAction.Initialize(characterAccess);
        }

        public void DeInitialize() {
            if (_action is ICharacterAccessInitializable action) action.DeInitialize();
            if (_asyncAction is ICharacterAccessInitializable asyncAction) asyncAction.DeInitialize();
        }

        public UniTask ApplyAsync(object source, ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            _action?.Apply(source, characterAccess);
            return _asyncAction?.ApplyAsync(source, characterAccess, cancellationToken) ?? default;
        }
    }

}
