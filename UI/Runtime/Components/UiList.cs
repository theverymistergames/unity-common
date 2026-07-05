using System;
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
        [SerializeField] private UiButton _buttonDecrement;
        [SerializeField] private UiButton _buttonIncrement;
        [SerializeField] private bool _loop;
        
        [Header("Elements")]
        [SerializeField] [Min(0)] private int _selectedIndex;
        [SerializeField] private List<string> _elements = new();

        public event Action<int> OnSelectedIndexChanged = delegate { };
        
        private int _currentTextHash;

        private void OnEnable() {
            _buttonIncrement.OnClicked += IncrementSelectedIndex;
            _buttonDecrement.OnClicked += DecrementSelectedIndex;
            
            SetSelectedIndex(_selectedIndex, force: true);
        }

        private void OnDisable() {
            _buttonIncrement.OnClicked -= IncrementSelectedIndex;
            _buttonDecrement.OnClicked -= DecrementSelectedIndex;
            
            _buttonIncrement.Block(this, false);
            _buttonDecrement.Block(this, false);
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
            if (index == _selectedIndex) SelectIndex(index);
            
#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(this);
#endif
            
            return true;
        }

        public int GetSelectedIndex() {
            return _selectedIndex;
        }

        public void SelectIndex(int index) {
            SetSelectedIndex(index, force: false);
        }

        private void IncrementSelectedIndex() {
            int count = _elements?.Count ?? 0;
            int next = _loop && _selectedIndex >= count ? 0 : _selectedIndex + 1;
            
            SetSelectedIndex(next);
        }

        private void DecrementSelectedIndex() {
            int count = _elements?.Count ?? 0;
            int next = _loop && _selectedIndex <= 0 ? count - 1 : _selectedIndex - 1;
            
            SetSelectedIndex(next);
        }

        private void SetSelectedIndex(int index, bool force = false) {
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

            int nextHash = GetTextHash(nextText);
            
            if (nextIndex == _selectedIndex && nextHash == _currentTextHash && !force) return;
            
            _selectedIndex = nextIndex;
            _currentTextHash = nextHash;
            
            OnSelectedIndexChanged.Invoke(_selectedIndex);
            
            ApplyText(nextText);
            UpdateButtons();
            
#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(this);
#endif
        }

        private void ApplyText(string text) {
#if UNITY_EDITOR
            if (!Application.isPlaying && !_applyTextInEditor) return;
#endif
            
            if (_elementTextField == null) return;
            
            _elementTextField.SetText(text);

#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(_elementTextField);
#endif
        }

        private void UpdateButtons() {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            int count = _elements?.Count ?? 0;
            bool canShowDecrement = count > 1 && (_loop || _selectedIndex > 0);
            bool canShowIncrement = count > 1 && (_loop || _selectedIndex < count - 1);

            _buttonIncrement.Block(this, !canShowIncrement);
            _buttonDecrement.Block(this, !canShowDecrement);
        }

        private static int GetTextHash(string text) {
            return text == null ? 0 : Animator.StringToHash(text);
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