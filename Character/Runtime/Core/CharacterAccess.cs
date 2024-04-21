using MisterGames.Actors;
using MisterGames.Character.Motion;
using MisterGames.Character.View;
using MisterGames.Common.GameObjects;
using UnityEngine;

namespace MisterGames.Character.Core {

    public sealed class CharacterAccess : MonoBehaviour, IActorComponent, ICharacterAccess {

        [SerializeField] private CharacterHeadAdapter headAdapter;
        [SerializeField] private CharacterBodyAdapter _bodyAdapter;

        [SerializeField] private CharacterPipelineBase[] _pipelines;

        public ITransformAdapter HeadAdapter => headAdapter;
        public ITransformAdapter BodyAdapter => _bodyAdapter;

        void IActorComponent.OnAwakeActor(IActor actor) {
            CharacterAccessRegistry.Instance.Register(actor);
        }

        void IActorComponent.OnDestroyActor(IActor actor) {
            CharacterAccessRegistry.Instance.Unregister(actor);
        }

        public T GetPipeline<T>() where T : ICharacterPipeline {
            for (int i = 0; i < _pipelines.Length; i++) {
                if (_pipelines[i] is T t) return t;
            }

            return default;
        }
    }

}
