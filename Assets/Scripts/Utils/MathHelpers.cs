using UnityEngine;

namespace View
{
    public static class MathHelpers
    {
        public static float Cross2d(Vector2 v, Vector2 w) => v.x * w.y - v.y * w.x;

        public static Vector2? Intersect(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2)
        {
            var len1 = end1 - start1;
            var len2 = end2 - start2;
            var a = Cross2d(start2 - start1, len1);
            var b = Cross2d(len1, len2);

            if (b == 0 || a == 0)
                return null;

            var u = a / b;
            var t = Cross2d(start2 - start1, len2) / b;

            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
                return start1 + t * len1;
            else
                return null;
        }
    }
}