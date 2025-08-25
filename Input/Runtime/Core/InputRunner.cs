using MisterGames.Input.Bindings;
using UnityEngine;

namespace MisterGames.Input.Core {

    [DefaultExecutionOrder(-100_000)]
    public sealed class InputRunner : MonoBehaviour {

        [SerializeField] private InputBlockService _inputBlockService;
        
        private readonly InputStorage _inputStorage = new();
        private readonly InputBindingHelper _inputBindingHelper = new();
        
        public void Awake() {
#if UNITY_EDITOR
            InputServices.DisableInputInEditModeAndClearSources();
#endif
            
            InputServices.Storage = _inputStorage;
            InputServices.Blocks = _inputBlockService;
            InputServices.BindingHelper = _inputBindingHelper;
            
            _inputBindingHelper.Initialize();
            _inputStorage.Initialize();
            _inputBlockService.Initialize(_inputStorage);
        }

        public void OnDestroy() {
            _inputStorage.Dispose();
            _inputBlockService.Dispose();
            
            InputServices.Storage = null;
            InputServices.Blocks = null;
            InputServices.BindingHelper = null;
        }
    }

}
