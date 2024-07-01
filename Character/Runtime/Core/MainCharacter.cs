using MisterGames.Actors;
using UnityEngine;

namespace MisterGames.Character.Core {

    public sealed class MainCharacter : MonoBehaviour, IActorComponent {

        void IActorComponent.OnAwake(IActor actor) {
            CharacterSystem.Instance.Register(actor);
        }

        void IActorComponent.OnTerminate(IActor actor) {
            CharacterSystem.Instance.Unregister(actor);
        }
    }

}
