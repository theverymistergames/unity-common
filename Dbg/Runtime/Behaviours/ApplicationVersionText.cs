using TMPro;
using UnityEngine;

namespace MisterGames.Dbg.Behaviours {
    
    public sealed class ApplicationVersionText : MonoBehaviour {
        
        [SerializeField] private TMP_Text _text;
        [SerializeField] private string _prefix = "v";
        
        private void Awake() {
            if (!NeedShow()) {
                if (_text != null) _text.gameObject.SetActive(false);
                return;
            }
            
            ApplyVersion();
        }

        private void ApplyVersion() {
            string v = GetVersionText();
            if (_text.text == v) return;
            
            _text.SetText(v);
        }

        private string GetVersionText() {
            return $"{_prefix} {Application.version}";
        }

        private static bool NeedShow() {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return true;
#endif
            
            return false;
        }

#if UNITY_EDITOR
        private void Reset() {
            _text = GetComponent<TMP_Text>();
            if (_text != null) ApplyVersion();
        }

        private void OnValidate() {
            if (_text != null) ApplyVersion();
        }  
#endif
    }
    
}