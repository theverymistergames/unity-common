using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Common.Audio {

    [DefaultExecutionOrder(-100_000)]
    public sealed class AudioBankServiceLauncher : MonoBehaviour {

        [SerializeField] private AudioBanksSettings _settings;

        private AudioBankService _service;

        private void Awake() {
            _service = new AudioBankService(_settings);
            Services.Register<IAudioBankService>(_service);
        }

        private void OnDestroy() {
            Services.Unregister(_service);
            _service.Dispose();
        }
    }

}