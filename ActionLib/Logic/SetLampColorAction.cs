using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Attributes;
using MisterGames.Logic.Interactives;
using UnityEngine;

namespace MisterGames.ActionLib.Logic {
    
    [Serializable]
    public sealed class SetLampColorAction : IActorAction {
    
        public LampBehaviour lamp;

        [Header("Light")]
        public Operation lightColorOperation;
        [VisibleIf(nameof(lightColorOperation), 0)]
        [ColorUsage(showAlpha: true, hdr: true)]
        public Color lightColor;
        [Tooltip("-1 for all lights")]
        [Min(-1)] public int lightIndex = -1;
        
        [Header("Material")]
        public Operation materialColorOperation;
        [VisibleIf(nameof(materialColorOperation), 0)]
        [ColorUsage(showAlpha: true, hdr: true)]
        public Color materialColor;
        [Tooltip("-1 for all materials")]
        [Min(-1)] public int materialIndex = -1;

        public enum Operation {
            Set,
            Reset,
        }
        
        public UniTask Apply(IActor context, CancellationToken cancellationToken = default) {
            switch (lightColorOperation) {
                case Operation.Set:
                    if (lightIndex < 0) lamp.SetAllLightsColor(lightColor);
                    else lamp.SetLightColor(lightColor, lightIndex);
                    break;
                
                case Operation.Reset:
                    if (lightIndex < 0) lamp.ResetAllLightsColor();
                    else lamp.ResetLightColor(lightIndex);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            switch (materialColorOperation) {
                case Operation.Set:
                    if (materialIndex < 0) lamp.SetAllMaterialsColor(materialColor);
                    else lamp.SetMaterialColor(materialColor, materialIndex);
                    break;
                
                case Operation.Reset:
                    if (materialIndex < 0) lamp.ResetAllMaterialsColor();
                    else lamp.ResetMaterialColor(materialIndex);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return default;
        }
    }
    
}