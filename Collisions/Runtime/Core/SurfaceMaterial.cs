using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.Collisions.Core {
    
    public sealed class SurfaceMaterial : MonoBehaviour {
        
        [SerializeField] private LabelValue _material;

        public int MaterialId => _material.GetValue();
    }
    
}