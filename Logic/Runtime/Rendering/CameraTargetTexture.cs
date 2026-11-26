using System;
using MisterGames.Common.Inputs;
using MisterGames.Common.Lists;
using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Logic.Rendering {

    public sealed class CameraTargetTexture : MonoBehaviour {

        [SerializeField] private Camera _camera;
        [SerializeField] private RenderTexture _renderTexture;
        [SerializeField] private Vector2 _defaultSize;
        [SerializeField] private Tier[] _tiers;
        
        [Serializable]
        private struct Tier {
            public int[] tiers;
            [Range(0f, 1f)] public float sizeMul;
        }

        private void OnEnable() {
            QualitySettings.activeQualityLevelChanged += OnQualityLevelChanged;

            ApplySize();
            _camera.targetTexture = _renderTexture;
        }

        private void OnDisable() {
            QualitySettings.activeQualityLevelChanged -= OnQualityLevelChanged;

            _camera.targetTexture = null;
        }

        private void OnQualityLevelChanged(int previousLevel, int currentLevel) {
            ApplySize();
        }

        private void ApplySize() {
            SetSize(_defaultSize * GetTierSizeMul(QualitySettings.GetQualityLevel()));
        }

        private float GetTierSizeMul(int qualityLevel) {
            for (int i = 0; i < _tiers.Length; i++) {
                ref var tier = ref _tiers[i];
                if (tier.tiers.Contains(qualityLevel)) return tier.sizeMul;
            }

            return 1f;
        }

        private void SetSize(Vector2 size) {
            size.x = size.x <= 0 ? _defaultSize.x : size.x;
            size.y = size.y <= 0 ? _defaultSize.y : size.y;
            
            var sizeInt = new Vector2Int(Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y));
            
            if (_renderTexture.width == sizeInt.x && _renderTexture.height == sizeInt.y) return;

            bool isAssigned = _camera.targetTexture == _renderTexture;
            if (isAssigned) _camera.targetTexture = null;

            _renderTexture.Release();
            _renderTexture.width = sizeInt.x;
            _renderTexture.height = sizeInt.y;
            _renderTexture.Create();

            if (isAssigned) _camera.targetTexture = _renderTexture;
        }
    }

}
