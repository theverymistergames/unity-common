using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using UnityEngine;

namespace MisterGames.ActionLib.Materials {

    [Serializable]
    public sealed class SetMaterialEmissionAction : IActorAction {

        public Renderer renderer;
        public Color color;
        public float intensity;

        private static readonly int EmissiveColor = Shader.PropertyToID("_EmissiveColor");
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            if (renderer.material == renderer.sharedMaterial) {
                renderer.material = new Material(renderer.sharedMaterial);
            }
            
            renderer.material.SetColor(EmissiveColor, color * intensity);
            return default;
        }
    }

}
