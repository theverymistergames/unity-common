using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace MisterGames.Logic.Rendering {

    public sealed class PortalView : MonoBehaviour {

        [SerializeField] private Camera _camera;
        [SerializeField] private Renderer _renderer;
        [SerializeField] [Min(0)] private int _materialIndex;
        [SerializeField] private string _textureProperty = "_PortalTex";

        [Header("Texture")]
        [SerializeField] private Vector2Int _resolution = new(1024, 1024);
        [SerializeField] private GraphicsFormat _colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
        [SerializeField] private FilterMode _filterMode = FilterMode.Bilinear;

        [Header("Update")]
        [SerializeField] private UpdateMode _updateMode = UpdateMode.EveryFrame;
        [SerializeField] [Min(1)] private int _onDemandFrames = 1;

        public enum UpdateMode {
            EveryFrame,
            OnDemand,
        }

        public RenderTexture Texture => _texture;
        public UpdateMode Mode { get => _updateMode; set => SetUpdateMode(value); }

        private RenderTexture _texture;
        private MaterialPropertyBlock _propertyBlock;
        private int _framesLeft;
        private bool _isSubscribed;

        private void Awake() {
            CreateTexture();
            ApplyTexture();
        }

        private void OnDestroy() {
            Unsubscribe();

            if (_camera != null && _camera.targetTexture == _texture) _camera.targetTexture = null;

            if (_texture != null) {
                _texture.Release();
                Destroy(_texture);
                _texture = null;
            }
        }

        private void OnEnable() {
            if (_updateMode == UpdateMode.EveryFrame) {
                _camera.enabled = true;
                return;
            }

            RequestRender();
        }

        private void OnDisable() {
            _framesLeft = 0;
            Unsubscribe();

            if (_camera != null) _camera.enabled = false;
        }

        public void RequestRender() {
            RequestRender(_onDemandFrames);
        }

        public void RequestRender(int frames) {
            if (!isActiveAndEnabled || _updateMode != UpdateMode.OnDemand) return;

            _framesLeft = Mathf.Max(_framesLeft, frames);
            _camera.enabled = _framesLeft > 0;

            if (_framesLeft > 0) Subscribe();
        }

        private void SetUpdateMode(UpdateMode mode) {
            if (_updateMode == mode) return;

            _updateMode = mode;
            if (!isActiveAndEnabled) return;

            if (mode == UpdateMode.EveryFrame) {
                _framesLeft = 0;
                Unsubscribe();
                _camera.enabled = true;
                return;
            }

            _camera.enabled = false;
            RequestRender();
        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera) {
            if (camera != _camera || --_framesLeft > 0) return;

            _framesLeft = 0;
            _camera.enabled = false;
            Unsubscribe();
        }

        private void Subscribe() {
            if (_isSubscribed) return;

            _isSubscribed = true;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }

        private void Unsubscribe() {
            if (!_isSubscribed) return;

            _isSubscribed = false;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }

        private void CreateTexture() {
            var descriptor = new RenderTextureDescriptor(
                Mathf.Max(1, _resolution.x),
                Mathf.Max(1, _resolution.y),
                _colorFormat,
                GraphicsFormat.D32_SFloat
            );

            _texture = new RenderTexture(descriptor) {
                name = $"{nameof(PortalView)}_{name}",
                filterMode = _filterMode,
                wrapMode = TextureWrapMode.Clamp,
            };

            _texture.Create();

            _camera.targetTexture = _texture;
            _camera.enabled = false;
        }

        private void ApplyTexture() {
            if (_renderer == null) return;

            _propertyBlock ??= new MaterialPropertyBlock();

            _renderer.GetPropertyBlock(_propertyBlock, _materialIndex);
            _propertyBlock.SetTexture(_textureProperty, _texture);
            _renderer.SetPropertyBlock(_propertyBlock, _materialIndex);
        }
    }

}
