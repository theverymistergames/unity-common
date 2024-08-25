using System.Buffers;
using MisterGames.Common.Maths;
using UnityEngine;
using UnityEngine.AI;

namespace MisterGames.Common {
    
    public static class DebugExt
    {

        public static void DrawLabel(Vector3 origin, string text, int fontSize = 14, Color color = default) {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(origin, text, new GUIStyle { fontSize = fontSize, normal = { textColor = color }});      
#endif
        }

        public static void DrawLine(
            Vector3 start,
            Vector3 end,
            Color color,
            float duration = 0f,
            bool gizmo = false)
        {
            if (gizmo) {
                if (duration > 0f) Debug.Log($"{nameof(DebugExt)}: Draw gizmo Gizmo does not support duration > 0.");
                
                Gizmos.color = color;
                Gizmos.DrawLine(start, end);
                
                return;
            }
            
            Debug.DrawLine(start, end, color, duration);
        }

        public static void DrawRay(
            Vector3 start,
            Vector3 dir,
            Color color,
            float duration = 0f,
            bool gizmo = false)
        {
            DrawLine(start, start + dir, color, duration, gizmo);
        }

        public static void DrawLines(
            Vector3[] points,
            Color color,
            bool loop = false,
            int count = -1,
            float duration = 0f,
            bool gizmo = false)
        {
            if (count < 0) count = points.Length;
            if (count < 2) return;

            for (int i = 0; i < count - 1; i++)
            {
                var p = points[i];
                var n = points[i < count - 1 ? i + 1 : 0];

                DrawLine(p, n, color, duration, gizmo);
            }

            if (loop)
            {
                var p = points[count - 1];
                var n = points[0];

                DrawLine(p, n, color, duration, gizmo);
            }
        }

        public static void DrawBezier3Points(
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            Color color,
            float step = 0.05f,
            float duration = 0f,
            bool gizmo = false)
        {
            int count = step > 0f ? Mathf.CeilToInt(1f / step) + 1 : 0;
            var points = ArrayPool<Vector3>.Shared.Rent(count);

            for (int i = 0; i < count; i++)
            {
                float t = (float) i / (count - 1);
                points[i] = BezierExtensions.EvaluateBezier3Points(p0, p1, p2, t);
            }

            DrawLines(points, color, loop: false, count, duration, gizmo);

            ArrayPool<Vector3>.Shared.Return(points);
        }

        public static void DrawBezier4Points(
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            Vector3 p3,
            Color color,
            float step = 0.05f,
            float duration = 0f,
            bool gizmo = false)
        {
            int count = step > 0f ? Mathf.CeilToInt(1f / step) + 1 : 0;
            var points = ArrayPool<Vector3>.Shared.Rent(count);

            for (int i = 0; i < count; i++)
            {
                float t = (float) i / (count - 1);
                points[i] = BezierExtensions.EvaluateBezier4Points(p0, p1, p2, p3, t);
            }

            DrawLines(points, color, loop: false, count, duration, gizmo);

            ArrayPool<Vector3>.Shared.Return(points);
        }

        public static void DrawPointer(
            Vector3 position,
            Color color,
            float size = 0.1f,
            float duration = 0f,
            bool gizmo = false)
        {
            var points = ArrayPool<Vector3>.Shared.Rent(6);

            points[0] = position;
            points[1] = position + new Vector3(0f, size, size * 0.5f);
            points[2] = position + new Vector3(0f, size, -size * 0.5f);
            points[3] = position;
            points[4] = position + new Vector3(size * 0.5f, size, 0f);
            points[5] = position + new Vector3(-size * 0.5f, size, 0f);

            DrawLines(points, color, loop: true, count: 6, duration, gizmo);

            ArrayPool<Vector3>.Shared.Return(points);
        }

        public static void DrawCircle(
            Vector3 position,
            Quaternion orientation,
            float radius,
            Color color,
            float angle = 360f,
            float step = 0.05f,
            float duration = 0f,
            bool gizmo = false)
        {
            var normal = orientation * Vector3.up;
            var start = orientation * Vector3.forward * radius;
            float inc = step * 360f;
            angle = Mathf.Clamp(angle, 0f, 360f);

            int count = inc > 0f ? Mathf.CeilToInt(angle / inc) + 1 : 0;
            var points = ArrayPool<Vector3>.Shared.Rent(count);

            for (int i = 0; i < count; i++)
            {
                float a = Mathf.Clamp(i * inc, 0, angle);
                var rot = Quaternion.AngleAxis(a, normal);
                points[i] = rot * start + position;
            }

            DrawLines(points, color, loop: angle >= 360f, count, duration, gizmo);

            ArrayPool<Vector3>.Shared.Return(points);
        }

        public static void DrawRect(
            Vector3 position,
            Quaternion orientation,
            Vector2 size,
            Color color,
            float duration = 0f,
            bool gizmo = false)
        {
            var v0 = position + orientation * Vector3.right * size.x * 0.5f + orientation * Vector3.up * size.y * 0.5f;
            var v1 = position + orientation * Vector3.right * size.x * 0.5f + orientation * Vector3.down * size.y * 0.5f;
            var v2 = position + orientation * Vector3.left * size.x * 0.5f + orientation * Vector3.down * size.y * 0.5f;
            var v3 = position + orientation * Vector3.left * size.x * 0.5f + orientation * Vector3.up * size.y * 0.5f;

            DrawLine(v0, v1, color, duration, gizmo);
            DrawLine(v1, v2, color, duration, gizmo);
            DrawLine(v2, v3, color, duration, gizmo);
            DrawLine(v3, v0, color, duration, gizmo);
        }

        public static void DrawSquare(
            Vector3 position,
            Quaternion orientation,
            float side,
            Color color,
            float duration = 0f,
            bool gizmo = false)
        {
            DrawRect(position, orientation, new Vector2(side, side), color, duration, gizmo);
        }

        public static void DrawBox(
            Vector3 position,
            Quaternion orientation,
            Vector3 size,
            Color color,
            float duration = 0f,
            bool gizmo = false)
        {
            var v0 = position + orientation * Vector3.up * size.y * 0.5f;
            var v1 = position + orientation * Vector3.down * size.y * 0.5f;

            var v2 = position + orientation * Vector3.forward * size.z * 0.5f;
            var v3 = position + orientation * Vector3.back * size.z * 0.5f;

            DrawRect(v0, orientation * Quaternion.Euler(90f, 0f, 0f), new Vector2(size.x, size.z), color, duration, gizmo);
            DrawRect(v1, orientation * Quaternion.Euler(90f, 0f, 0f), new Vector2(size.x, size.z), color, duration, gizmo);

            DrawRect(v2, orientation, new Vector2(size.x, size.y), color, duration, gizmo);
            DrawRect(v3, orientation, new Vector2(size.x, size.y), color, duration, gizmo);
        }

        public static void DrawCube(
            Vector3 position,
            Quaternion orientation,
            float size,
            Color color,
            float duration = 0f,
            bool gizmo = false)
        {
            DrawBox(position, orientation, new Vector3(size, size, size), color, duration, gizmo);
        }

        public static void DrawCylinder(
            Vector3 from,
            Vector3 to,
            float radius,
            Color color,
            float step = 0.05f,
            float duration = 0f,
            bool gizmo = false)
        {
            var normal = (to - from).normalized;
            var rot = Quaternion.FromToRotation(Vector3.up, normal);
            var forward = rot * Vector3.forward;
            var right = rot * Vector3.right;

            var fr = forward * radius;
            var rr = right * radius;

            var from0 = from + fr;
            var from1 = from - fr;
            var from2 = from + rr;
            var from3 = from - rr;

            var to0 = to + fr;
            var to1 = to - fr;
            var to2 = to + rr;
            var to3 = to - rr;

            var orient = Quaternion.LookRotation(forward, normal);

            DrawLine(from0, to0, color, duration, gizmo);
            DrawLine(from1, to1, color, duration, gizmo);
            DrawLine(from2, to2, color, duration, gizmo);
            DrawLine(from3, to3, color, duration, gizmo);

            DrawCircle(from, orient, radius, color, angle: 360f, step, duration, gizmo);
            DrawCircle(to, orient, radius, color, angle: 360f, step, duration, gizmo);
        }

        public static void DrawSphere(
            Vector3 position,
            float radius,
            Color color,
            float step = 0.05f,
            float duration = 0f,
            bool gizmo = false)
        {
            var or0 = Quaternion.identity;
            var or1 = Quaternion.LookRotation(Vector3.up, Vector3.forward);
            var or2 = Quaternion.LookRotation(Vector3.up, Vector3.right);

            DrawCircle(position, or0, radius, color, angle: 360f, step, duration, gizmo);
            DrawCircle(position, or1, radius, color, angle: 360f, step, duration, gizmo);
            DrawCircle(position, or2, radius, color, angle: 360f, step, duration, gizmo);
        }

        public static void DrawCapsule(
            Vector3 from,
            Vector3 to,
            float radius,
            Color color,
            float step = 0.05f,
            float duration = 0f,
            bool gizmo = false)
        {
            var normal = (to - from).normalized;
            var rot = Quaternion.FromToRotation(Vector3.up, normal);
            var forward = rot * Vector3.forward;
            var right = rot * Vector3.right;

            var or0 = Quaternion.LookRotation(forward, right);
            var or1 = Quaternion.LookRotation(-right, forward);
            var or2 = Quaternion.LookRotation(forward, -right);
            var or3 = Quaternion.LookRotation(-right, -forward);

            DrawCylinder(from, to, radius, color, step, duration, gizmo);
            DrawCircle(from, or0, radius, color, angle: 180f, step, duration, gizmo);
            DrawCircle(from, or1, radius, color, angle: 180f, step, duration, gizmo);
            DrawCircle(to, or2, radius, color, angle: 180f, step, duration, gizmo);
            DrawCircle(to, or3, radius, color, angle: 180f, step, duration, gizmo);
        }

        public static void DrawSphereCast(
            Vector3 from,
            Vector3 to,
            float radius,
            Color color,
            float step = 0.05f,
            float duration = 0f,
            bool gizmo = false)
        {
            var normal = (to - from).normalized;
            var rot = Quaternion.FromToRotation(Vector3.up, normal);
            var forward = rot * Vector3.forward;
            var right = rot * Vector3.right;

            var or0 = Quaternion.LookRotation(-forward, right);
            var or1 = Quaternion.LookRotation(right, forward);
            var or2 = Quaternion.LookRotation(forward, -right);
            var or3 = Quaternion.LookRotation(-right, -forward);

            DrawCylinder(from, to, radius, color, step, duration, gizmo);
            DrawCircle(from, or0, radius, color, angle: 180f, step, duration, gizmo);
            DrawCircle(from, or1, radius, color, angle: 180f, step, duration, gizmo);
            DrawCircle(to, or2, radius, color, angle: 180f, step, duration, gizmo);
            DrawCircle(to, or3, radius, color, angle: 180f, step, duration, gizmo);
        }

        public static void DrawFrustum(
            Vector3 origin,
            Vector3 direction,
            Vector2 angles,
            Vector2 window,
            Color color,
            float duration = 0f,
            bool gizmo = false)
        {
            var orient = Quaternion.LookRotation(direction, Vector3.up);
            
            var startRotX = Quaternion.Euler(0f, -angles.x * 0.5f, 0f);
            var endRotX = Quaternion.Euler(0f, angles.x * 0.5f, 0f);
            var startRotY = Quaternion.Euler(angles.y * 0.5f, 0f, 0f);
            var endRotY = Quaternion.Euler(-angles.y * 0.5f, 0f, 0f);
            
            var startPosX = origin + orient * new Vector3(-window.x * 0.5f, 0f, 0f);
            var endPosX = origin + orient * new Vector3(window.x * 0.5f, 0f, 0f);
            var startPosY = orient * new Vector3(0f, -window.y * 0.5f, 0f);
            var endPosY = orient * new Vector3(0f, window.y * 0.5f, 0f);

            var f = Vector3.forward * direction.magnitude;
            var rayStartStart = orient * startRotX * startRotY * f;
            var rayStartEnd = orient * startRotX * endRotY * f;
            var rayEndStart = orient * endRotX * startRotY * f;
            var rayEndEnd = orient * endRotX * endRotY * f;
            
            DrawRay(startPosX + startPosY, rayStartStart, color, duration, gizmo);
            DrawRay(startPosX + endPosY, rayStartEnd, color, duration, gizmo);
            DrawRay(endPosX + startPosY, rayEndStart, color, duration, gizmo);
            DrawRay(endPosX + endPosY, rayEndEnd, color, duration, gizmo);
            
            DrawLine(startPosX + startPosY, startPosX + endPosY, color, duration, gizmo);
            DrawLine(startPosX + startPosY, endPosX + startPosY, color, duration, gizmo);
            DrawLine(endPosX + endPosY, startPosX + endPosY, color, duration, gizmo);
            DrawLine(endPosX + endPosY, endPosX + startPosY, color);
            
            DrawLine(startPosX + startPosY + rayStartStart, startPosX + endPosY + rayStartEnd, color, duration, gizmo);
            DrawLine(startPosX + startPosY + rayStartStart, endPosX + startPosY + rayEndStart, color, duration, gizmo);
            DrawLine(endPosX + endPosY + rayEndEnd, startPosX + endPosY + rayStartEnd, color, duration, gizmo);
            DrawLine(endPosX + endPosY + rayEndEnd, endPosX + startPosY + rayEndStart, color);
        }
        
        public static void DrawTrajectory(
            ThrowData data,
            Vector3 origin,
            float gravity,
            Color color,
            float step = 0.05f,
            float duration = 0f,
            bool gizmo = false)
        {
            for (float t = 0f; t < data.time; t += step)
            {
                var p = TrajectoryUtils.EvaluateTrajectory(origin, data.velocity, t, gravity);
                DrawSphere(p, 0.02f, color, step, duration, gizmo);
            }
        }
        
        public static void DrawTransform(
            Vector3 position,
            Quaternion rotation,
            Color color,
            float radius = 0.05f,
            float step = 0.05f,
            float duration = 0f,
            bool gizmo = false)
        {
            DrawSphere(position, radius, color, step, duration, gizmo);
            DrawRay(position, rotation * Vector3.forward * (radius * 3f), Color.blue, duration, gizmo);
            DrawRay(position, rotation * Vector3.up * (radius * 3f), Color.green, duration, gizmo);
            DrawRay(position, rotation * Vector3.right * (radius * 3f), Color.red, duration, gizmo);
        }
        
        public static void DrawTransform(
            Transform transform,
            Color color,
            float radius = 0.05f,
            float step = 0.05f,
            float duration = 0f,
            bool gizmo = false)
        {
            DrawTransform(transform.position, transform.rotation, color, radius, step, duration, gizmo);
        }
        
        public static void DrawNavMeshPath(
            NavMeshPath path,
            Color color,
            Color colorEnd,
            float radius = 0.1f,
            float step = 0.05f,
            float duration = 0f,
            bool gizmo = false)
        {
            if (path is not { corners: { Length: > 0 }}) return;
            
            int length = path.corners.Length;
            for (int i = 0; i < length; i++)
            {
                var p = path.corners[i];
                DrawSphere(p, radius, i < length - 1 ? color : colorEnd, step, duration, gizmo);
                if (i > 0) DrawLine(p, path.corners[i - 1], color, duration, gizmo);
            }
        }
    }
    
}