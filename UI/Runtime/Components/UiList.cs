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

        public void SetElementsCount(int count) {
            _elements ??= new List<string>(count);

            int oldCount = _elements.Count;
            if (oldCount == count) return;
            
            if (oldCount > count) {
                _elements.RemoveRange(count, _elements.Count - count);
            }
            else if (oldCount < count) {
                for (int i = oldCount; i < count; i++) {
                    _elements.Add(null);
                }
            }
      
            SetSelectedIndex(_selectedIndex);
            
#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(this);
#endif
        }

        public bool SetElement(int index, string text) {
            if (_elements == null || index < 0 || index >= _elements.Count) return false;
            
            _elements[index] = text;
            if (index == _selectedIndex) SetSelectedIndex(index);
            
#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(this);
#endif
            
            return true;
        }

        public int GetSelectedIndex() {
            return _selectedIndex;
        }

        public void SetSelectedIndex(int index) {
            int nextIndex;
            string nextText;

            if (_elements == null || _elements.Count == 0) {
                nextIndex = 0;
                nextText = null;
            }
            else {
                nextIndex = Mathf.Clamp(index, 0, _elements.Count - 1);
                nextText = _elements[nextIndex];
            }

            _selectedIndex = nextIndex;
            ApplyText(nextText);

#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(this);
#endif
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