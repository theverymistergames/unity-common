using MisterGames.Common.Labels;
using UnityEngine;

namespace MisterGames.Character.Phys {
    
    public sealed class SurfaceMaterial : MonoBehaviour {
        
        [SerializeField] private LabelValue _material;

        public int MaterialId => _material.GetValue();
    }
    
}