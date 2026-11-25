using MisterGames.Common.GameObjects;
using MisterGames.Common.Lists;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Logic.Rendering {
    
    public sealed class EnableByGraphicsTierHdrp : MonoBehaviour {
        
        [SerializeField] private Object[] _objects;
        [SerializeField] [Min(0)] private int[] _enableInTiers;
        
        private void Awake() {
            bool enable = _enableInTiers.Contains(QualitySettings.GetQualityLevel());
            _objects.SetEnabled(enable);
        }
    }
    
}