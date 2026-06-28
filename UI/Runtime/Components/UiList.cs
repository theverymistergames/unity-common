using System.Collections.Generic;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MisterGames.UI.Components {
    
    public sealed class UiList : MonoBehaviour {

        [Header("UI Components")]
        [SerializeField] private TMP_Text _elementTextField;
        
        [Header("Elements")]
        [SerializeField] [Min(0)] private int _selectedIndex;
        [SerializeField] private List<string> _elements = new();

        private void Awake() {
            SetSelectedIndex(_selectedIndex);
        }

        public IReadOnlyList<string> GetElements() {
            return _elements;
        }
        
        public void SetElements(IReadOnlyList<string> elements) {
            _elements.Clear();
            _elements.AddRange(elements);
            
            SetSelectedIndex(_selectedIndex);

#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(this);
#endif
        }

        public int GetSelectedIndex() {
            return _selectedIndex;
        }

        public void SetSelectedIndex(int index) {
            if (_elements == null || _elements.Count == 0) {
                _selectedIndex = 0;
                ApplyText(null);
                return;
            }
            
            _selectedIndex = Mathf.Clamp(index, 0, _elements.Count - 1);
            ApplyText(_elements[_selectedIndex]);
        }

        public void IncrementSelectedIndex() {
            SetSelectedIndex(_selectedIndex + 1);
        }

        public void DecrementSelectedIndex() {
            SetSelectedIndex(_selectedIndex - 1);
        }

        private void ApplyText(string text) {
#if UNITY_EDITOR
            if (!Application.isPlaying && !_applyTextInEditor) return;
#endif
            
            _elementTextField.SetText(text);

#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(_elementTextField);
#endif
        }
        
#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _applyTextInEditor = true;
        
        private void OnValidate() {
            SetSelectedIndex(_selectedIndex);
        }
#endif
    }
    
}