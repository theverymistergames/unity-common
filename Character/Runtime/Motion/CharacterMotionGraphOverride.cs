using MisterGames.Actors.Actions;
using MisterGames.Character.Core;
using UnityEngine;

namespace MisterGames.Character.Motion {
    
    public sealed class CharacterMotionGraphOverride : MonoBehaviour {
        
        [SerializeField] private ActorAction _action;
        [SerializeField] private int _priority;

        private CharacterMotionGraphPipeline _motionGraph;
        
        private void OnEnable() {
            _motionGraph = CharacterSpawner.Instance.GetCharacter().GetComponent<CharacterMotionGraphPipeline>();
            _motionGraph?.SetGraphOverride(this, _action, _priority);
        }

        private void OnDisable() {
            _motionGraph?.RemoveGraphOverride(this);
        }
    }
    
}