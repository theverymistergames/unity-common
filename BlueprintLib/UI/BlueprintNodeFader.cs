using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Blueprints;
using MisterGames.Common.Async;
using MisterGames.Scenes.Loading;
using UnityEngine;

namespace MisterGames.BlueprintLib {

    [Serializable]
    [BlueprintNode(Name = "Fader", Category = "UI", Color = BlueprintColors.Node.Actions)]
    public sealed class BlueprintNodeFader : IBlueprintNode, IBlueprintEnter, IBlueprintOutput<float> {

        public void CreatePorts(IBlueprintMeta meta, NodeId id) {
            meta.AddPort(id, Port.Enter("Fade In"));
            meta.AddPort(id, Port.Enter("Fade Out"));
            meta.AddPort(id, Port.Input<float>("Duration"));
            meta.AddPort(id, Port.Input<AnimationCurve>("Curve"));
            meta.AddPort(id, Port.Exit("On Finish"));
            meta.AddPort(id, Port.Output<float>("Progress"));
        }

        private CancellationTokenSource _cts;
        
        public void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            AsyncExt.RecreateCts(ref _cts);
        }

        public void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {
            AsyncExt.DisposeCts(ref _cts);
        }

        public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
            switch (port) {
                case 0:
                    Fade(blueprint, token, FadeMode.FadeIn, _cts.Token).Forget();
                    break;
                
                case 1:
                    Fade(blueprint, token, FadeMode.FadeOut, _cts.Token).Forget();
                    break;
            }
        }

        public float GetPortValue(IBlueprint blueprint, NodeToken token, int port) {
            return port == 5 && Fader.Main is { } fader ? fader.Progress : 0f;
        }

        private async UniTask Fade(IBlueprint blueprint, NodeToken token, FadeMode mode, CancellationToken cancellationToken) {
            float duration = blueprint.Read(token, 2, defaultValue: -1f);
            var curve = blueprint.Read<AnimationCurve>(token, 3, defaultValue: null);

            if (Fader.Main is { } fader) {
                await fader.FadeAsync(mode, duration, curve);
            }
            
            if (cancellationToken.IsCancellationRequested) return;
            
            blueprint.Call(token, 4);
        }
    }

}
