using MisterGames.Common.Labels;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Common.State {
    
    public sealed class EnableStateSource : MonoBehaviour {

        [SerializeField] private LabelValue _group;

        private void OnEnable() {
            Services.Get<IEnableStateService>()?.NotifyEnable(_group.GetValue(), true);
        }

        private void OnDisable() {
            Services.Get<IEnableStateService>()?.NotifyEnable(_group.GetValue(), false);
        }
    }
    
}