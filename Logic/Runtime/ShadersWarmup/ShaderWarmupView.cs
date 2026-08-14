using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Localization;
using MisterGames.Common.Maths;
using MisterGames.Scenes.Loading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.Logic.ShadersWarmup {

    [DefaultExecutionOrder(-90_000)]
    internal sealed class ShaderWarmupView : MonoBehaviour {
        
        [Header("View")]
        [SerializeField] private GameObject _view;
        
        [Header("Header")]
        [SerializeField] private TMP_Text _header;
        [SerializeField] private LocalizationKey _headerKey;
        
        [Header("Progress")]
        [SerializeField] private Image _progressBar;
        [SerializeField] private float _outputValue0 = 0f;
        [SerializeField] private float _outputValue1 = 100f;
        [SerializeField] private string _progressSurround = "{0}%";
        [SerializeField] private string _progressFormat = "0";
        [SerializeField] [Min(0f)] private float _epsilon = 0.1f;

        private ShaderWarmupSettings _settings;
        private CancellationTokenSource _cts;
        private float _lastProgress;

        private void Awake() {
            _settings = ShaderWarmupService.Instance.GetSettings();
            
            AsyncExt.RecreateCts(ref _cts);
            _view.SetActive(false);
            
            if (ShaderWarmupService.Instance.IsWarmupCompleted) {
                OnWarmupCompleted();
                return;
            }

            ShaderWarmupService.Instance.OnWarmupStarted += OnWarmupStarted;
            ShaderWarmupService.Instance.OnWarmupCompleted += OnWarmupCompleted;
            ShaderWarmupService.Instance.OnWarmupProgress += OnWarmupProgress;
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _cts);
            
            ShaderWarmupService.Instance.OnWarmupStarted -= OnWarmupStarted;
            ShaderWarmupService.Instance.OnWarmupCompleted -= OnWarmupCompleted;
            ShaderWarmupService.Instance.OnWarmupProgress -= OnWarmupProgress;
        }

        private void OnWarmupStarted() {
            ApplyProgress(0f, force: true);
            
            AsyncExt.RecreateCts(ref _cts);
            EnableViewDelayed(enable: true, _settings.EnableViewDelay, _settings.EnableViewFader, _cts.Token).Forget();
        }

        private void OnWarmupCompleted() {
            AsyncExt.RecreateCts(ref _cts);
            EnableViewDelayed(enable: false, _settings.DisableViewDelay, _settings.DisableViewFader, _cts.Token).Forget();
        }

        private void OnWarmupProgress(float p) {
            ApplyProgress(p, force: false);
        }

        private async UniTask EnableViewDelayed(bool enable, float delay, float fader, CancellationToken ct) {
            await AsyncExt.DelayUnscaled(delay, cancellationToken: ct);
            if (ct.IsCancellationRequested) return;

            if (enable) {
                _view.SetActive(true);
                await Fader.Main.FadeOutAsync(fader);
                return;
            }
            
            await Fader.Main.FadeInAsync(fader);
            if (ct.IsCancellationRequested) return;
            
            _view.SetActive(false);
        }

        private void ApplyProgress(float p, bool force) {
            _progressBar.fillAmount = p;

            p = Mathf.Lerp(_outputValue0, _outputValue1, p).RoundToStep(_epsilon);
            
            if (_lastProgress.IsNearlyEqual(p, _epsilon) && !force) return;

            _lastProgress = p;
            
            string v = p.ToString(_progressFormat);
            string text = string.IsNullOrWhiteSpace(_progressSurround) ? v : string.Format(_progressSurround, v);
            _header.SetText(string.Format(_headerKey.GetValue(), text));
        }
    }
    
}