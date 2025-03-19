using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.Common.Localization {
    
    [DefaultExecutionOrder(-10000)]
    public sealed class LocalizationService : MonoBehaviour, ILocalizationService {
        
        [SerializeField] private LabelLibrary _localizationLib;
        [SerializeField] [Min(0)] private int _languagesArray;
        [LabelFilter("LocalizationLib")]
        [SerializeField] private LabelValue _defaultLanguage;
        
        public static ILocalizationService Instance { get; private set; }

        public int LocalizationId { get => _localizationId; set => SetLocalizationId(value); }
        private int _localizationId;
        
        private void Awake() {
            Instance = this;

            SetLocalizationId(_defaultLanguage.GetValue());
        }

        private void OnDestroy() {
            Instance = null;
        }

        private void SetLocalizationId(int hash) {
            int count = _localizationLib.GetLabelsCount(_languagesArray);

            for (int i = 0; i < count; i++) {
                int id = _localizationLib.GetLabelId(_languagesArray, i);
                if (hash != _localizationLib.GetValue(id)) continue;
                
                _localizationId = hash;
                Debug.Log($"LocalizationService: set localization {_localizationLib.GetLabel(id)}");
                break;
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (!Application.isPlaying) return;
            
            SetLocalizationId(_defaultLanguage.GetValue());
        }  
#endif
    }
    
}