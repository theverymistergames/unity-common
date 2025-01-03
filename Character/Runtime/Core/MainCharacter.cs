using MisterGames.Actors;
using MisterGames.Character.Motion;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Character.Core {

    public sealed class MainCharacter : MonoBehaviour, IActorComponent {

        void IActorComponent.OnAwake(IActor actor) {
            if (actor == CharacterSystem.Instance.GetCharacter()) return;
            
#if UNITY_EDITOR 
            Debug.LogWarning(
#else
            Debug.LogError(
#endif
                $"{nameof(MainCharacter)}: found second {nameof(MainCharacter)} at {actor.Transform.GetPathInScene(includeSceneName: true)}, " +
                $"it will be destroyed. Please remove second {nameof(MainCharacter)}, " + 
                $"only original {nameof(MainCharacter)} at {CharacterSystem.Instance.GetCharacter().Transform.GetPathInScene(includeSceneName: true)} should exist. " + 
                $"Original {nameof(MainCharacter)} will be teleported to the position of the second {nameof(MainCharacter)}.");
            
            actor.Transform.GetPositionAndRotation(out var pos, out var rot);
            CharacterSystem.Instance.GetCharacter().GetComponent<CharacterMotionPipeline>().Teleport(pos, rot);

            Destroy(actor.GameObject);
        }

    }

}
