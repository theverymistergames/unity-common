using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace MisterGames.Logic.Rendering {
    
    [Serializable, VolumeComponentMenu("Post-processing/Custom/Mirror Screen")]
    public sealed class MirrorScreen : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int Amount = Shader.PropertyToID("_Amount");
        private const string ShaderName = "Hidden/Custom/MirrorScreen";

        [Tooltip("0 = normal, 1 = mirrored")]
        public ClampedFloatParameter amount = new(0f, 0f, 1f);

        private Material material;

        public override bool visibleInSceneView => false;

        public bool IsActive() =>
            active && material != null && amount.value > 0.001f;

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            var shader = Shader.Find(ShaderName);
            if (shader != null)
                material = new Material(shader);
        }

        public override void Render(
            CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (!IsActive()) return;

            material.SetTexture(MainTex, source);
            material.SetFloat(Amount, amount.value);
            HDUtils.DrawFullScreen(cmd, material, destination);
        }

        public override void Cleanup() => CoreUtils.Destroy(material);
    }
    
}