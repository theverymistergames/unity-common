using System.Collections.Generic;
using MisterGames.Common.Attributes;
using UnityEngine;
using UnityEngine.Rendering;

namespace MisterGames.Logic.Rendering {

    public sealed class PortalCameraFollower : MonoBehaviour {
        
        [SerializeReference] [SubclassSelector] private ICameraProvider _viewCameraProvider;
        [SerializeField] private Camera _portalCamera;

        [Header("Portals")]
        [SerializeField] private Transform _entrance;
        [SerializeField] private Transform _exit;
        [SerializeField] private bool _flip = true;

        [Header("Projection")]
        [SerializeField] private bool _copyFieldOfView = true;
        [SerializeField] private bool _copyAspect = true;
        [SerializeField] private bool _obliqueClipping;
        [SerializeField] private float _clipPlaneOffset = 0.05f;

        private static readonly Quaternion FlipRotation = Quaternion.Euler(0f, 180f, 0f);

        private void OnEnable() {
            RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
        }

        private void OnDisable() {
            RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;

            if (_portalCamera != null) _portalCamera.ResetProjectionMatrix();
        }

        private void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras) {
            if (_viewCameraProvider?.GetCamera() is not { } camera) return;

            UpdateCamera(camera);
        }

        private void UpdateCamera(Camera viewer) {
            var viewerTransform = viewer.transform;
            var flip = _flip ? FlipRotation : Quaternion.identity;

            var localPosition = flip * _entrance.InverseTransformPoint(viewerTransform.position);
            var localRotation = flip * Quaternion.Inverse(_entrance.rotation) * viewerTransform.rotation;

            _portalCamera.transform.SetPositionAndRotation(
                _exit.TransformPoint(localPosition),
                _exit.rotation * localRotation
            );

            if (_copyFieldOfView) _portalCamera.fieldOfView = viewer.fieldOfView;
            if (_copyAspect) _portalCamera.aspect = viewer.aspect;

            if (_obliqueClipping) ApplyObliqueClipping();
            else _portalCamera.ResetProjectionMatrix();
        }

        private void ApplyObliqueClipping() {
            _portalCamera.ResetProjectionMatrix();

            var cameraTransform = _portalCamera.transform;
            var normal = _exit.forward;

            if (Vector3.Dot(normal, _exit.position - cameraTransform.position) < 0f) normal = -normal;

            var worldToCamera = _portalCamera.worldToCameraMatrix;
            var cameraNormal = worldToCamera.MultiplyVector(normal);
            var cameraPoint = worldToCamera.MultiplyPoint(_exit.position - normal * _clipPlaneOffset);

            var clipPlane = new Vector4(cameraNormal.x, cameraNormal.y, cameraNormal.z, -Vector3.Dot(cameraPoint, cameraNormal));
            _portalCamera.projectionMatrix = _portalCamera.CalculateObliqueMatrix(clipPlane);
        }
    }

}
