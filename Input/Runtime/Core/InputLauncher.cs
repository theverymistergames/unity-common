using MisterGames.Input.Bindings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MisterGames.Input.Core {

    [DefaultExecutionOrder(-100_000)]
    public sealed class InputLauncher : MonoBehaviour {

        [SerializeField] private InputBlockService _inputBlockService;
        
        private readonly InputMapper _inputMapper = new();
        private readonly InputBindingHelper _inputBindingHelper = new();
        private readonly InputUpdater _inputUpdater = new();
        
        public void Awake() {
#if UNITY_EDITOR
            InputServices.DisableInputInEditModeAndClearSources();
            InputServices.DisposeServices();
#endif
            
            _inputBindingHelper.Initialize();
            _inputMapper.Initialize(InputSystem.actions);
            _inputBlockService.Initialize(_inputMapper);
            
            InputServices.Mapper = _inputMapper;
            InputServices.Blocks = _inputBlockService;
            InputServices.BindingHelper = _inputBindingHelper;
            
            _inputUpdater.Initialize();
        }

        public void OnDestroy() {
            _inputUpdater.Dispose();
            
            _inputMapper.Dispose();
            _inputBlockService.Dispose();
            _inputBindingHelper.Dispose();
        }
    }

}
