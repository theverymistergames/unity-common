using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Character.Motion;
using MisterGames.Collisions.Core;
using MisterGames.Collisions.Detectors;
using MisterGames.Common.Attributes;
using MisterGames.Common.Labels;
using MisterGames.Common.Maths;
using UnityEngine;

namespace MisterGames.Character.Phys {
    
    public sealed class CharacterWaterMaterialDetector : MaterialDetectorBase, IActorComponent {
        
        [SerializeField] private LabelValue _material;
        [SerializeField] [MinMaxSlider(0f, 1f)] private Vector2 _submergeWeightRemap;
        
        private readonly List<MaterialInfo> _materialList = new();
        private CharacterWaterProcessor _waterProcessor;

        void IActorComponent.OnAwake(IActor actor) {
            _waterProcessor = actor.GetComponent<CharacterWaterProcessor>();
        }

        public override IReadOnlyList<MaterialInfo> GetMaterials(Vector3 point, Vector3 normal) {
            _materialList.Clear();

            float submergeWeight = _waterProcessor.SubmergeWeight;
            float weight = InterpolationUtils.Remap01(_submergeWeightRemap.x, _submergeWeightRemap.y, submergeWeight);

            if (weight <= 0f) return _materialList;
            
            _materialList.Add(new MaterialInfo(_material.GetValue(), weight));
            
            return _materialList;
        }
    }
    
}