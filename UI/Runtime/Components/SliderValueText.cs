using MisterGames.Common.Maths;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    [ExecuteInEditMode]
    public sealed class SliderValueText : MonoBehaviour {
        
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _textField;
        [SerializeField] private string _format = "0.0";
        [SerializeField] private string _surround = "{0}%";
        [SerializeField] [Min(0f)] private float _sliderValue0 = 0f;
        [SerializeField] [Min(0f)] private float _sliderValue1 = 1f;
        [SerializeField] private float _outputValue0 = 0f;
        [SerializeField] private float _outputValue1 = 100f;
        [SerializeField] [Min(0f)] private float _epsilon = 0.1f;

        private float _lastOutput;
        
        private void OnEnable() {
            _lastOutput = GetSliderOutput();
            SetTextValue(_lastOutput);
            
#if UNITY_EDITOR
            if (!Application.isPlaying && _slider == null) return; 
#endif
            
            _slider.onValueChanged.RemoveListener(OnValueChanged);
            _slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable() {
#if UNITY_EDITOR
            if (!Application.isPlaying && _slider == null) return; 
#endif
            
            _slider.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(float arg0) {
#if UNITY_EDITOR
            if (!Application.isPlaying && !_updateInEditor) return; 
#endif
            
            float output = GetSliderOutput();
            if (output.IsNearlyEqual(_lastOutput, _epsilon)) return;
            
            _lastOutput = output;
            SetTextValue(_lastOutput);
        }

        private void SetTextValue(float value) {
#if UNITY_EDITOR
            if (!Application.isPlaying && _textField == null) return;
#endif
            
            string v = value.ToString(_format);
            string text = string.IsNullOrWhiteSpace(_surround) ? v : string.Format(_surround, v);
            
#if UNITY_EDITOR
            if (!Application.isPlaying && _textField.text == text) return;
#endif
            
            _textField.SetText(text);

#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(_textField);
#endif
        }

        private float GetSliderOutput() {
#if UNITY_EDITOR
            if (!Application.isPlaying && _slider == null) return _outputValue0;
#endif
            
            float v = _slider.value;
            float t = Mathf.InverseLerp(_sliderValue0, _sliderValue1, v);
            return Mathf.Lerp(_outputValue0, _outputValue1, t).RoundToStep(_epsilon);
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _updateInEditor;

        private void OnValidate() {
            if (!Application.isPlaying && !_updateInEditor) return;
            
            _lastOutput = GetSliderOutput();
            SetTextValue(_lastOutput);
        }
#endif
    }
    
}