using System.Collections.Generic;
using MisterGames.Blueprints.Compile;
using MisterGames.Blueprints.Runtime;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    internal static class BlueprintTweenConverter2 {

        public static ITween AsTween(LinkIterator links) {
            if (!links.MoveNext()) return null;

            var tween = AsTween(links.Read<IBlueprintNodeTween2>());
            if (!links.MoveNext()) return tween;

            var parallelTween = new TweenParallel { tweens = new List<ITween>() };
            if (tween != null) parallelTween.tweens.Add(tween);

            tween = AsTween(links.Read<IBlueprintNodeTween2>());
            if (tween != null) parallelTween.tweens.Add(tween);

            while (links.MoveNext()) {
                if (AsTween(links.Read<IBlueprintNodeTween2>()) is {} t) parallelTween.tweens.Add(t);
            }

            return parallelTween;
        }

        private static ITween AsTween(IBlueprintNodeTween2 node) {
            if (node == null) return null;

            while (true) {
                var links = node.NextLinks;
                if (!links.MoveNext()) return node.Tween;

                var nextNode = links.Read<IBlueprintNodeTween2>();

                if (!links.MoveNext()) {
                    if (nextNode != null) {
                        if (node.Tween != null) return AsTween(nextNode, new List<ITween> { node.Tween });

                        node = nextNode;
                        continue;
                    }

                    return node.Tween;
                }

                var parallelTween = new TweenParallel { tweens = new List<ITween>() };
                var n = nextNode;
                nextNode = null;

                var tween = AsTween(n);
                if (tween != null) {
                    parallelTween.tweens.Add(tween);
                    nextNode = n;
                }

                n = links.Read<IBlueprintNodeTween2>();
                tween = AsTween(n);
                if (tween != null) {
                    parallelTween.tweens.Add(tween);
                    nextNode ??= n;
                }

                while (links.MoveNext()) {
                    n = links.Read<IBlueprintNodeTween2>();
                    if (n == null) continue;

                    tween = AsTween(n);
                    if (tween == null) continue;

                    parallelTween.tweens.Add(tween);
                    nextNode ??= n;
                }

                int count = parallelTween.tweens.Count;
                if (count == 0) return node.Tween;

                if (count == 1) {
                    if (nextNode != null) {
                        if (node.Tween == null) {
                            node = nextNode;
                            continue;
                        }

                        return AsTween(nextNode, new List<ITween> { node.Tween });
                    }

                    return node.Tween;
                }

                return node.Tween == null ? parallelTween : AsTween(new List<ITween> { node.Tween, parallelTween });
            }
        }

        private static ITween AsTween(IBlueprintNodeTween2 node, List<ITween> tweens) {
            while (true) {
                if (node.Tween != null) tweens.Add(node.Tween);

                var links = node.NextLinks;
                if (!links.MoveNext()) return AsTween(tweens);

                var nextNode = links.Read<IBlueprintNodeTween2>();

                if (!links.MoveNext()) {
                    if (nextNode != null) {
                        node = nextNode;
                        continue;
                    }

                    return AsTween(tweens);
                }

                var parallelTween = new TweenParallel { tweens = new List<ITween>() };
                var n = nextNode;
                nextNode = null;

                var tween = AsTween(n);
                if (tween != null) {
                    parallelTween.tweens.Add(tween);
                    nextNode = n;
                }

                n = links.Read<IBlueprintNodeTween2>();
                tween = AsTween(n);
                if (tween != null) {
                    parallelTween.tweens.Add(tween);
                    nextNode ??= n;
                }

                while (links.MoveNext()) {
                    n = links.Read<IBlueprintNodeTween2>();
                    if (n == null) continue;

                    tween = AsTween(n);
                    if (tween == null) continue;

                    parallelTween.tweens.Add(tween);
                    nextNode ??= n;
                }

                int count = parallelTween.tweens.Count;
                if (count == 0) return AsTween(tweens);

                if (count == 1) {
                    if (nextNode != null) {
                        node = nextNode;
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
