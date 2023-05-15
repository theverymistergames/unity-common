using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Character.Core;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Character.Actions {

    [CreateAssetMenu(fileName = nameof(CharacterActionsAsset), menuName = "MisterGames/Character/" + nameof(CharacterActionsAsset))]
    public sealed class CharacterActionsAsset : ScriptableObject, ICharacterAction, ICharacterAccessInitializable {

        [SerializeReference] [SubclassSelector] private ICharacterAction _action;

        public void Initialize(ICharacterAccess characterAccess) {
            if (_action is ICharacterAccessInitializable action) action.Initialize(characterAccess);
        }

        public void DeInitialize() {
            if (_action is ICharacterAccessInitializable action) action.DeInitialize();
        }

        public UniTask Apply(object source, ICharacterAccess characterAccess, CancellationToken cancellationToken = default) {
            return _action?.Apply(source, characterAccess, cancellationToken) ?? default;
        }
    }

}
