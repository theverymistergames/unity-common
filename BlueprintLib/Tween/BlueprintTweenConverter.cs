using System.Collections.Generic;
using MisterGames.Blueprints.Compile;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    internal static class BlueprintTweenConverter {

        public static ITween AsTween(List<RuntimeLink> links) {
            int count = links.Count;
            if (count == 0) return null;
            if (count == 1) return AsTween(links[0].Get<IBlueprintNodeTween>());

            var parallelTween = new TweenParallel { tweens = new List<ITween>(count) };
            for (int i = 0; i < count; i++) {
                if (AsTween(links[i].Get<IBlueprintNodeTween>()) is {} t) parallelTween.tweens.Add(t);
            }

            return parallelTween;
        }

        private static ITween AsTween(IBlueprintNodeTween node) {
            if (node == null) return null;

            while (true) {
                var links = node.NextLinks;
                int count = links.Count;

                if (count == 0) return node.Tween;

                if (count == 1) {
                    if (links[0].Get<IBlueprintNodeTween>() is {} n) {
                        if (node.Tween != null) return AsTween(n, new List<ITween> { node.Tween });

                        node = n;
                        continue;
                    }

                    return node.Tween;
                }

                int firstValidLinkIndex = -1;
                var parallelTween = new TweenParallel {tweens = new List<ITween>(count)};
                for (int i = 0; i < count; i++) {
                    if (AsTween(links[i].Get<IBlueprintNodeTween>()) is { } t) {
                        parallelTween.tweens.Add(t);
                        if (firstValidLinkIndex < 0) firstValidLinkIndex = i;
                    }
                }

                count = parallelTween.tweens.Count;
                if (count == 0) return node.Tween;

                if (count == 1) {
                    if (firstValidLinkIndex > 0 && links[firstValidLinkIndex].Get<IBlueprintNodeTween>() is {} n) {
                        if (node.Tween == null) {
                            node = n;
                            continue;
                        }

                        return AsTween(n, new List<ITween> { node.Tween });
                    }

                    return node.Tween;
                }

                return node.Tween == null ? parallelTween : AsTween(new List<ITween> { node.Tween, parallelTween });
            }
        }

        private static ITween AsTween(IBlueprintNodeTween node, List<ITween> tweens) {
            while (true) {
                if (node.Tween != null) tweens.Add(node.Tween);

                var links = node.NextLinks;
                int count = links.Count;
                if (count == 0) return AsTween(tweens);

                if (count == 1) {
                    if (links[0].Get<IBlueprintNodeTween>() is {} n) {
                        node = n;
                        continue;
                    }

                    return AsTween(tweens);
                }

                int firstValidLinkIndex = -1;
                var parallelTween = new TweenParallel { tweens = new List<ITween>(count) };
                for (int i = 0; i < count; i++) {
                    if (AsTween(links[i].Get<IBlueprintNodeTween>()) is { } t) {
                        parallelTween.tweens.Add(t);
                        if (firstValidLinkIndex < 0) firstValidLinkIndex = i;
                    }
                }

                count = parallelTween.tweens.Count;
                if (count == 0) return AsTween(tweens);

                if (count == 1) {
                    if (firstValidLinkIndex > 0 && links[firstValidLinkIndex].Get<IBlueprintNodeTween>() is {} n) {
                        node = n;
                        continue;
                    }

                    return AsTween(tweens);
                }

                tweens.Add(parallelTween);
                return AsTween(tweens);
            }
        }

        private static ITween AsTween(List<ITween> tweens) => tweens.Count switch {
            0 => null,
            1 => tweens[0],
            _ => new TweenSequence { tweens = tweens }
        };
    }



}
