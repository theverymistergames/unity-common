using MisterGames.Common.GameObjects;
using MisterGames.Common.Lists;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Logic.Rendering {
    
    public sealed class EnableByGraphicsTierHdrp : MonoBehaviour {
        
        [SerializeField] private Object[] _objects;
        [SerializeField] [Min(0)] private int[] _enableInTiers;
        
        private void Awake() {
            QualitySettings.activeQualityLevelChanged += OnQualityLevelChanged;
            Apply(QualitySettings.GetQualityLevel());
        }

        private void OnDestroy() {
            QualitySettings.activeQualityLevelChanged -= OnQualityLevelChanged;
        }

        private void OnQualityLevelChanged(int previousLevel, int currentLevel) {
            Apply(currentLevel);
        }

        private void Apply(int qualityLevel) {
            _objects.SetEnabled(_enableInTiers.Contains(qualityLevel));
        }
    }
    
}