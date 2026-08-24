using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace MisterGames.Logic.Rendering {
    
    [Serializable, VolumeComponentMenu("Post-processing/Custom/Screen Mirror")]
    public sealed class ScreenMirror : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int Amount = Shader.PropertyToID("_Amount");
        private const string ShaderName = "Hidden/Custom/MirrorScreen";

        [Tooltip("0 = normal, 1 = mirrored")]
        public ClampedFloatParameter amount = new(0f, 0f, 1f);

        private Material material;

        public override bool visibleInSceneView => false;

        private Material Material
        {
            get
            {
                if (material != null) return material;

                var shader = Shader.Find(ShaderName);
                if (shader == null) return null;

                material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                return material;
            }
        }

        // Cheap checks first, so Shader.Find is only retried while the effect is actually blended in.
        public bool IsActive() =>
            active && amount.value > 0.001f && Material != null;

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Render(
            CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (!IsActive()) return;

            var mat = Material;

            mat.SetTexture(MainTex, source);
            mat.SetFloat(Amount, amount.value);
            HDUtils.DrawFullScreen(cmd, mat, destination);
        }

        public override void Cleanup() => CoreUtils.Destroy(material);
    }
    
}