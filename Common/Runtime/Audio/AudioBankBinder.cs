using MisterGames.Common.Service;
using UnityEngine;

namespace MisterGames.Common.Audio {
    
    public sealed class AudioBankBinder : MonoBehaviour {

        [SerializeField] private Mode _mode;
        [SerializeField] private AudioBankReference[] _banks;
        
        private enum Mode {
            AwakeDestroy,
            EnableDisable,
        }

        private void Awake() {
            if (_mode != Mode.AwakeDestroy || !Services.TryGet(out IAudioBankService service)) return;

            for (int i = 0; i < _banks.Length; i++) {
                service.Bind(this, _banks[i]);
            }
        }

        private void OnDestroy() {
            if (_mode != Mode.AwakeDestroy || !Services.TryGet(out IAudioBankService service)) return;

            for (int i = 0; i < _banks.Length; i++) {
                service.Unbind(this, _banks[i]);
            }
        }

        private void OnEnable() {
            if (_mode != Mode.EnableDisable || !Services.TryGet(out IAudioBankService service)) return;

            for (int i = 0; i < _banks.Length; i++) {
                service.Bind(this, _banks[i]);
            }
        }

        private void OnDisable() {
            if (_mode != Mode.EnableDisable || !Services.TryGet(out IAudioBankService service)) return;

            for (int i = 0; i < _banks.Length; i++) {
                service.Unbind(this, _banks[i]);
            }
        }
    }
    
}