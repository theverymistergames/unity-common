using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

namespace MisterGames.Splines.Tools {
    
    /// <summary>
    /// Deform a mesh and place it along a spline, given various parameters.
    /// 
    /// This class intend to cover the most common situations of mesh bending. It can be used as-is in your project,
    /// or can serve as a source of inspiration to write your own procedural generator.
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class SplineMeshGenerator : MonoBehaviour {
        
        [Tooltip("Mesh to bend along the spline.")]
        [SerializeField]
        private Mesh _mesh;
        
        [Tooltip("Material to apply on the bent mesh.")]
        [SerializeField]
        private Material _material;
        
        [Tooltip("Physic material to apply on the bent mesh.")]
        [SerializeField]
        private PhysicsMaterial _physicMaterial;
        
        [Tooltip("Translation to apply on the mesh before bending it.")]
        [SerializeField]
        private Vector3 _translation;
        
        [Tooltip("Rotation to apply on the mesh before bending it.")]
        [SerializeField]
        private Vector3 _rotation = new Vector3(0f, 90f, 0f);
        
        [Tooltip("Scale to apply on the mesh before bending it.")]
        [SerializeField]
        private Vector3 _scale = Vector3.one;

        [Tooltip("If true, a mesh collider will be generated.")]
        [SerializeField]
        private bool _generateCollider = true;
        
        [Tooltip("The mode to use to fill the chosen interval with the bent mesh.")]
        [SerializeField]
        private MeshFillingMode _mode = MeshFillingMode.Repeat;

        internal SplineContainer splineContainer;

        private void OnEnable() { 
            splineContainer = GetComponent<SplineContainer>();
            TryGenerateMesh(splineContainer);
        }

        // TODO test with UnityEngine.Splines
        public void TryGenerateMesh(SplineContainer spline) {
            if (_mesh == null || _material == null) return;
            
            var meshGameObject = FindOrCreateMesh($"{name}_GeneratedMesh");

            meshGameObject.GetComponent<SplineMeshBender>().SetSpline(spline);
            meshGameObject.GetComponent<MeshCollider>().enabled = _generateCollider;
        }

        private GameObject FindOrCreateMesh(string objectName) {
            var childTransform = transform.Find(objectName);
            var res = childTransform == null
                ? new GameObject(
                    name,
                    typeof(MeshFilter),
                    typeof(MeshRenderer),
                    typeof(SplineMeshBender),
                    typeof(MeshCollider)
                ) {
                    transform = {
                        parent = transform,
                        localPosition = Vector3.zero,
                        localScale = Vector3.one,
                        localRotation = Quaternion.identity
                    },
                    isStatic = true
                }
                : childTransform.gameObject;

            res.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            res.GetComponent<MeshRenderer>().material = _material;
            res.GetComponent<MeshCollider>().material = _physicMaterial;
            
            var splineMeshBender = res.GetComponent<SplineMeshBender>();
            var sourceMesh = new MeshInfo(_mesh, _translation, Quaternion.Euler(_rotation), _scale);
            splineMeshBender.SetSourceMesh(sourceMesh, _mode);

            return res;
        }

        private void OnValidate() {
            StartCoroutine(WaitFrameAndTryGenerate());
        }

        private IEnumerator WaitFrameAndTryGenerate() {
            yield return null;
            if (enabled) TryGenerateMesh(splineContainer);
        }
    }
    
}
