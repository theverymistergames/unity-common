using Cysharp.Threading.Tasks;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.Logic.Libs {
    
    public sealed class LabelLibraryActionReference : MonoBehaviour {
        
        [SerializeField] private LabelValue<IActorAction> _label;
        [SubclassSelector]
        [SerializeReference] private IActorAction _action;

        private void Awake() {
            _label.TrySetData(_action);
        }

        private void OnDestroy() {
            _label.ClearData();
        }

#if UNITY_EDITOR
        [Button(mode: ButtonAttribute.Mode.Runtime)]
        private void LaunchManually() {
            _action?.Apply(null).Forget();
        }
#endif
    }
    
}