using System;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Common.Collisions {

    public class MaterialDetector : MonoBehaviour {

        [SerializeField] private CollisionDetector _collisionDetector;
        [SerializeField] private MaterialData _defaultMaterialData;

        public event Action OnMaterialChanged = delegate {  };
        public Optional<MaterialData> Material { get; private set; }

        private readonly Optional<MaterialData> _empty = Optional<MaterialData>.Empty();
        private Optional<MaterialData> _default;
        
        private void Awake() {
            InitMaterialData();
        }
        
        private void OnEnable() {
            _collisionDetector.OnSurfaceChanged += HandleSurfaceChanged;
        }

        private void OnDisable() {
            _collisionDetector.OnSurfaceChanged -= HandleSurfaceChanged;
        }

        private void InitMaterialData() {
            _default = Optional<MaterialData>.WithValue(_defaultMaterialData);
            Material = _empty;
        }

        private void HandleSurfaceChanged() {
            Material = ExtractMaterial();
            OnMaterialChanged.Invoke();
        }

        private Optional<MaterialData> ExtractMaterial() {
            var info = _collisionDetector.CollisionInfo;
            if (!info.hasContact) return _empty;

            var holder = info.surface.GetComponentInParent<MaterialHolder>();
            return holder != null ? Optional<MaterialData>.WithValue(holder.materialData) : _default;
        }
        
    }

}