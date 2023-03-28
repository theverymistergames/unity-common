using System;
using System.Collections;
using MisterGames.Bezier.Objects;
using MisterGames.Bezier.Utility;
using UnityEngine;

namespace MisterGames.Bezier.Generation {
    
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
        private PhysicMaterial _physicMaterial;
        
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
        
        [Tooltip("The mode to use to fill the choosen interval with the bent mesh.")]
        [SerializeField]
        private SplineMeshBender.FillingMode _mode = SplineMeshBender.FillingMode.Repeat;

        private SplineCreator _splineCreator;

        private void OnEnable() { 
            _splineCreator = GetComponent<SplineCreator>();
            _splineCreator.pathUpdated += Generate; 

            Generate();
        }

        private void OnDisable() {
            _splineCreator.pathUpdated -= Generate; 
        }

        private void OnValidate() {
            StartCoroutine(ExecuteInNextFrame(() => {
                if (enabled) Generate();
            }));
        }

        private static IEnumerator ExecuteInNextFrame(Action action) {
            yield return null;
            action.Invoke();
        }

        private void Generate() {
            if (_mesh == null || _material == null) return;
            
            var go = FindOrCreate("GeneratedMesh");
            go.GetComponent<SplineMeshBender>().SetInterval(_splineCreator.path); 
            go.GetComponent<MeshCollider>().enabled = _generateCollider;
        }

        private GameObject FindOrCreate(string objectName) {
            var childTransform = transform.Find(objectName);
            GameObject res;
            
            if (childTransform == null) {
                res = UoUtility.Create(objectName,
                    gameObject,
                    typeof(MeshFilter),
                    typeof(MeshRenderer),
                    typeof(SplineMeshBender),
                    typeof(MeshCollider));
                
                res.isStatic = true;
            } 
            else {
                res = childTransform.gameObject;
            }
            
            res.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            res.GetComponent<MeshRenderer>().material = _material;
            res.GetComponent<MeshCollider>().material = _physicMaterial;
            
            var mb = res.GetComponent<SplineMeshBender>();
            var sourceMesh = new MeshInfo {
                mesh = _mesh,
                translation = _translation,
                rotation = Quaternion.Euler(_rotation),
                scale = _scale
            }.Create();
            
            mb.SetSourceMesh(sourceMesh, _mode);
            return res;
        }
        
    }
    
}
