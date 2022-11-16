using System;
using MisterGames.Collisions.Core;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Collisions.Materials {

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
            _collisionDetector.OnTransformChanged += HandleTransformChanged;
        }

        private void OnDisable() {
            _collisionDetector.OnTransformChanged -= HandleTransformChanged;
        }

        private void InitMaterialData() {
            _default = Optional<MaterialData>.WithValue(_defaultMaterialData);
            Material = _empty;
        }

        private void HandleTransformChanged() {
            Material = ExtractMaterial();
            OnMaterialChanged.Invoke();
        }

        private Optional<MaterialData> ExtractMaterial() {
            var info = _collisionDetector.CollisionInfo;
            if (!info.hasContact) return _empty;

            var holder = info.transform.GetComponentInParent<MaterialHolder>();
            return holder != null ? Optional<MaterialData>.WithValue(holder.materialData) : _default;
        }
        
    }

}
