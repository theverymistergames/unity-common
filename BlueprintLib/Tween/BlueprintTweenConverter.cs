using System.Collections.Generic;
using MisterGames.Blueprints.Runtime;
using MisterGames.Tweens;
using MisterGames.Tweens.Core;

namespace MisterGames.BlueprintLib {

    internal static class BlueprintTweenConverter {

        public static ITween AsTween(LinkIterator links) {
            if (!links.MoveNext()) return null;

            var tween = AsTween(links.Read<IBlueprintNodeTween>());
            if (!links.MoveNext()) return tween;

            var parallelTween = new TweenParallel { tweens = new List<ITween>() };
            if (tween != null) parallelTween.tweens.Add(tween);

            tween = AsTween(links.Read<IBlueprintNodeTween>());
            if (tween != null) parallelTween.tweens.Add(tween);

            while (links.MoveNext()) {
                if (AsTween(links.Read<IBlueprintNodeTween>()) is {} t) parallelTween.tweens.Add(t);
            }

            return parallelTween;
        }

        private static ITween AsTween(IBlueprintNodeTween node) {
            if (node == null) return null;

            while (true) {
                var links = node.NextLinks;
                if (!links.MoveNext()) return node.Tween;

                var nextNode = links.Read<IBlueprintNodeTween>();

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

                n = links.Read<IBlueprintNodeTween>();
                tween = AsTween(n);
                if (tween != null) {
                    parallelTween.tweens.Add(tween);
                    nextNode ??= n;
                }

                while (links.MoveNext()) {
                    n = links.Read<IBlueprintNodeTween>();
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

        private static ITween AsTween(IBlueprintNodeTween node, List<ITween> tweens) {
            while (true) {
                if (node.Tween != null) tweens.Add(node.Tween);

                var links = node.NextLinks;
                if (!links.MoveNext()) return AsTween(tweens);

                var nextNode = links.Read<IBlueprintNodeTween>();

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

                n = links.Read<IBlueprintNodeTween>();
                tween = AsTween(n);
                if (tween != null) {
                    parallelTween.tweens.Add(tween);
                    nextNode ??= n;
                }

                while (links.MoveNext()) {
                    n = links.Read<IBlueprintNodeTween>();
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

}
