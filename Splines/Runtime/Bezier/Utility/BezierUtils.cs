using MisterGames.Bezier.Objects;

namespace MisterGames.Bezier.Utility {

    public static class BezierUtils {

        public static void AlignControlPointsVertically(this BezierPath bezierPath) {
            var count = bezierPath.NumPoints;
            for (var i = 0; i < count; i++) {
                var isAnchor = i % 3 == 0;
                if (isAnchor) continue;

                var nextIsAnchor = (i + 1) % 3 == 0;
                var closestAnchorIndex = nextIsAnchor ? i + 1 : i - 1;
                if (closestAnchorIndex < 0 || closestAnchorIndex > count - 1) continue;
                
                var point = bezierPath[i];
                point.y = bezierPath[closestAnchorIndex].y;
                
                bezierPath.MovePoint(i, point, true);
            }
            
            bezierPath.NotifyPathModified();
        }

    }

}