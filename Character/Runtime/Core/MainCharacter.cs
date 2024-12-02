﻿using MisterGames.Actors;
using MisterGames.Character.Motion;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Character.Core {

    public sealed class MainCharacter : MonoBehaviour, IActorComponent {

        void IActorComponent.OnAwake(IActor actor) {
            if (actor == CharacterSystem.Instance.GetCharacter()) return;
            
            Debug.LogError($"{nameof(MainCharacter)}: found second {nameof(MainCharacter)} at {GameObjectExtensions.GetPathInScene(actor.Transform, includeSceneName: true)}, " +
                           $"it will be destroyed. Please remove second {nameof(MainCharacter)}, " +
                           $"only original {nameof(MainCharacter)} at {GameObjectExtensions.GetPathInScene(CharacterSystem.Instance.GetCharacter().Transform, includeSceneName: true)} should exist. " +
                           $"Original {nameof(MainCharacter)} will be teleported to the position of the second {nameof(MainCharacter)}.");
            
            actor.Transform.GetPositionAndRotation(out var pos, out var rot);
            CharacterSystem.Instance.GetCharacter().GetComponent<CharacterMotionPipeline>().Teleport(pos, rot);

            Destroy(actor.GameObject);
        }

    }

}
