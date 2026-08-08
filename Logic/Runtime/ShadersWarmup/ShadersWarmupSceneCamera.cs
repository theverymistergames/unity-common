using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace MisterGames.Logic.ShadersWarmup {
 
    internal sealed class ShadersWarmupSceneCamera : MonoBehaviour {
    
        [SerializeField] private ShadersWarmupSceneContentCollector _shadersWarmupSceneContentCollector;
        [SerializeField] private Camera _camera;

        [Header("Pass")]
        [SerializeField] [Min(0.01f)] private float _speed = 30f;

        [SerializeField] private bool _playOnStart = true;
        [SerializeField] private bool _loop;

        [Header("Framing")]
        [SerializeField] [Min(1f)] private float _viewportPadding = 1.1f;

        [SerializeField] [Min(0f)] private float _zDistancePadding = 10f;
        [SerializeField] [Min(0.01f)] private float _noBoundsViewDistance = 10f;

        [Header("UI")]
        [SerializeField] [Min(1)] private int _uiFramesPerInstance = 2;

        [Header("Custom passes")]
        [SerializeField] [Min(1)] private int _customPassFramesPerInstance = 2;

        [Header("Volume profiles")]
        [SerializeField] private Volume _volume;

        [SerializeField] [Min(1)] private int _profileFramesPerInstance = 3;

        [Header("Quality tiers")]
        [SerializeField] private int[] _qualityTiers = Array.Empty<int>();

        [SerializeField] [Min(0)] private int _tierSettleFrames = 2;
        [SerializeField] private bool _restoreQualityTier = true;

        private readonly List<PassSegment> _passSegments = new();

        private readonly struct PassSegment {

            public readonly Vector3 start;
            public readonly Vector3 end;

            public PassSegment(Vector3 start, Vector3 end) {
                this.start = start;
                this.end = end;
            }
        }

        private void Start() {
            RunPass(destroyCancellationToken).Forget();
        }

        private async UniTask RunPass(CancellationToken ct) {
            if (!TryBuildPassSegments()) return;

            var qualityTiers = BuildQualityTiers();
            if (qualityTiers.Count == 0) return;

            if (_volume == null && _shadersWarmupSceneContentCollector.VolumeProfiles.Count > 0) {
                Debug.LogWarning($"{nameof(ShadersWarmupSceneCamera)}: collected " +
                                 $"{_shadersWarmupSceneContentCollector.VolumeProfiles.Count} volume profiles, " +
                                 $"but volume is null. Volumes will be skipped.");
            }
                
            transform.rotation = Quaternion.identity;
            int originalTier = QualitySettings.GetQualityLevel();
            var originalProfile = _volume != null ? _volume.sharedProfile : null;
            SetUiInstancesActive(false);
            SetCustomPassVolumesEnabled(false);

            try {
                do {
                    for (int i = 0; i < qualityTiers.Count; i++) {
                        await ApplyQualityTier(qualityTiers[i], ct);
                        await MoveThroughPassSegments(ct);
                        await WarmupUiInstances(ct);
                        await WarmupCustomPasses(ct);
                        await WarmupVolumeProfiles(ct);
                    }
                } while (_loop && !ct.IsCancellationRequested);
            }
            finally {
                if (_restoreQualityTier) QualitySettings.SetQualityLevel(originalTier);
                SetUiInstancesActive(false);
                SetCustomPassVolumesEnabled(false);
                if (_volume != null) _volume.sharedProfile = originalProfile;
            }

            ShaderWarmupService.Instance.NotifyShaderWarmupScenePassCompleted();
        }

        private async UniTask ApplyQualityTier(int tier, CancellationToken ct) {
            QualitySettings.SetQualityLevel(tier);

            Debug.Log($"{nameof(ShadersWarmupSceneCamera)}: pass for quality tier [{tier}] {QualitySettings.names[tier]}.");

            for (int i = 0; i < _tierSettleFrames && !ct.IsCancellationRequested; i++) {
                await UniTask.Yield();
            }
        }

        private List<int> BuildQualityTiers() {
            var tiers = new List<int>();
            int tierCount = QualitySettings.names.Length;

            if (_qualityTiers == null || _qualityTiers.Length == 0) {
                for (int i = 0; i < tierCount; i++) tiers.Add(i);
                return tiers;
            }

            for (int i = 0; i < _qualityTiers.Length; i++) {
                int tier = _qualityTiers[i];
                if (tier < 0 || tier >= tierCount) continue;

                if (!tiers.Contains(tier)) tiers.Add(tier);
            }

            return tiers;
        }

        private async UniTask WarmupUiInstances(CancellationToken ct) {
            var uiInstances = _shadersWarmupSceneContentCollector.UiInstances;

            for (int i = 0; i < uiInstances.Count && !ct.IsCancellationRequested; i++) {
                var instance = uiInstances[i];
                if (instance == null) continue;

                instance.SetActive(true);

                for (int frame = 0; frame < _uiFramesPerInstance && !ct.IsCancellationRequested; frame++) {
                    await UniTask.Yield();
                }

                if (instance != null) instance.SetActive(false);
            }
        }

        private async UniTask WarmupCustomPasses(CancellationToken ct) {
            var customPassVolumes = _shadersWarmupSceneContentCollector.CustomPassVolumes;

            for (int i = 0; i < customPassVolumes.Count && !ct.IsCancellationRequested; i++) {
                if (customPassVolumes[i] == null) continue;

                customPassVolumes[i].enabled = true;

                for (int frame = 0; frame < _customPassFramesPerInstance && !ct.IsCancellationRequested; frame++) {
                    await UniTask.Yield();
                }

                if (customPassVolumes[i] != null) customPassVolumes[i].enabled = false;
            }
        }

        private void SetCustomPassVolumesEnabled(bool enabled) {
            if (_shadersWarmupSceneContentCollector == null) return;

            var customPassVolumes = _shadersWarmupSceneContentCollector.CustomPassVolumes;

            for (int i = 0; i < customPassVolumes.Count; i++) {
                if (customPassVolumes[i] != null) customPassVolumes[i].enabled = enabled;
            }
        }

        private async UniTask WarmupVolumeProfiles(CancellationToken ct) {
            if (_volume == null) return;

            var volumeProfiles = _shadersWarmupSceneContentCollector.VolumeProfiles;

            for (int i = 0; i < volumeProfiles.Count && !ct.IsCancellationRequested; i++) {
                if (volumeProfiles[i] == null) continue;

                _volume.sharedProfile = volumeProfiles[i];

                for (int frame = 0; frame < _profileFramesPerInstance && !ct.IsCancellationRequested; frame++) {
                    await UniTask.Yield();
                }
            }
        }

        private void SetUiInstancesActive(bool active) {
            if (_shadersWarmupSceneContentCollector == null) return;

            var uiInstances = _shadersWarmupSceneContentCollector.UiInstances;
            for (int i = 0; i < uiInstances.Count; i++) {
                if (uiInstances[i] != null) uiInstances[i].SetActive(active);
            }
        }

        private async UniTask MoveThroughPassSegments(CancellationToken ct) {
            for (int i = 0; i < _passSegments.Count && !ct.IsCancellationRequested; i++) {
                var segment = _passSegments[i];

                // There is nothing to render between the segments, so the camera jumps to the next start
                // instead of flying over the empty space.
                transform.position = segment.start;

                while (!ct.IsCancellationRequested && transform.position != segment.end) {
                    transform.position = Vector3.MoveTowards(transform.position, segment.end, _speed * Time.deltaTime);
                    await UniTask.Yield();
                }
            }
        }

        private bool TryBuildPassSegments() {
            if (_shadersWarmupSceneContentCollector == null) {
                Debug.LogError($"{nameof(ShadersWarmupSceneCamera)}: {nameof(ShadersWarmupSceneContentCollector)} is null.");
                return false;
            }

            if (_camera == null) {
                Debug.LogError($"{nameof(ShadersWarmupSceneCamera)}: camera is null.");
                return false;
            }

            _passSegments.Clear();

            var columns = _shadersWarmupSceneContentCollector.GridColumns;
            float requiredFarClip = 0f;

            for (int i = 0; i < columns.Count; i++) {
                var column = columns[i];
                float distance = GetDistanceToFit(column.size.x);
                float cameraZ = column.min.z - distance;

                _passSegments.Add(new PassSegment(
                    new Vector3(column.center.x, column.min.y, cameraZ),
                    new Vector3(column.center.x, column.max.y, cameraZ)));

                requiredFarClip = Mathf.Max(requiredFarClip, column.max.z - cameraZ);
            }

            if (_shadersWarmupSceneContentCollector.HasNoBoundsRow) {
                var row = _shadersWarmupSceneContentCollector.NoBoundsRow;
                float cameraZ = row.min.z - _noBoundsViewDistance - _zDistancePadding;

                _passSegments.Add(new PassSegment(
                    new Vector3(row.min.x, row.center.y, cameraZ),
                    new Vector3(row.max.x, row.center.y, cameraZ)));

                requiredFarClip = Mathf.Max(requiredFarClip, row.max.z - cameraZ);
            }

            // A distant column must not be cut off by the far plane, otherwise a part of the grid is simply not drawn.
            _camera.farClipPlane = Mathf.Max(_camera.farClipPlane, requiredFarClip * _viewportPadding);

            return true;
        }

        private float GetDistanceToFit(float width) {
            float horizontalFov = Camera.VerticalToHorizontalFieldOfView(_camera.fieldOfView, _camera.aspect);
            float distance = width * 0.5f * _viewportPadding / Mathf.Tan(horizontalFov * 0.5f * Mathf.Deg2Rad);
            return Mathf.Max(distance, _camera.nearClipPlane * 2f) + _zDistancePadding;
        }
    }
    
}