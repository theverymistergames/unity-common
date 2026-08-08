using MisterGames.Common.GameObjects;
using UnityEngine;
using UnityEngine.UI;

namespace MisterGames.Logic.Shaders
{
    
    internal sealed class ShaderWarmupView : MonoBehaviour
    {
        [Header("View")]
        [SerializeField] private GameObject _view;
        [SerializeField] private Image _progressBar;

        [Header("Completion")]
        [SerializeField] private GameObject[] _enableAfterWarmup;
        
        private void Awake()
        {
            if (ShaderWarmupService.Instance.IsWarmupCompleted)
            {
                OnWarmupCompleted();
                return;
            }
            
            ShaderWarmupService.Instance.OnWarmupCompleted += OnWarmupCompleted;
            ShaderWarmupService.Instance.OnWarmupProgress += OnWarmupProgress;
        }

        private void OnDestroy()
        {
            ShaderWarmupService.Instance.OnWarmupCompleted -= OnWarmupCompleted;
            ShaderWarmupService.Instance.OnWarmupProgress -= OnWarmupProgress;
        }

        private void OnWarmupCompleted()
        {
            _view.SetActive(false);
            _enableAfterWarmup.SetActive(true);
        }

        private void OnWarmupProgress(float p)
        {
            _progressBar.fillAmount = p;
        }
    }
    
}