using MisterGames.Actors;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.Logic.Libs {
    
    public sealed class LabelLibraryActorReference : MonoBehaviour, IActorComponent {
        
        [SerializeField] private LabelValue<IActor> _label;
        
        void IActorComponent.OnAwake(IActor actor) {
            _label.TrySetData(actor);
        }

        void IActorComponent.OnDestroyed(IActor actor) {
            _label.TrySetData(null);
        }
    }
    
}