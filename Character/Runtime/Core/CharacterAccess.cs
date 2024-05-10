using MisterGames.Actors;
using UnityEngine;

namespace MisterGames.Character.Core {

    public sealed class CharacterAccess : MonoBehaviour, IActorComponent {

        void IActorComponent.OnAwake(IActor actor) {
            CharacterAccessRegistry.Instance.Register(actor);
        }

        void IActorComponent.OnTerminate(IActor actor) {
            CharacterAccessRegistry.Instance.Unregister(actor);
        }
    }

}
