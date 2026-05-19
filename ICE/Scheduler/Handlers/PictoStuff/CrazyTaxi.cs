using Pictomancy;

namespace ICE.Scheduler.Handlers.PictoStuff;

internal static partial class PictoManager
{
    private const int N = 8;
    private static readonly float AngleOffset = MathF.PI / N; // 22.5° — flat top/bottom

    /// <summary>
    /// Draws a 3D tapered octagon arrow with swept-back wings.
    /// Arrow points along -Z, rotated around Y axis by <paramref name="rotation"/>.
    /// Use this for flat ground indicators where you already have a facing angle.
    /// </summary>
    public static void DrawCrazyArrow(PctDrawList drawList, Vector3 origin, float rotation, float backRadius, float tipRadius, float depth, float wingSpread, float notchFrac, uint color)
    {
        float zBack = depth / 2f;
        float zTip = -depth / 2f;
        float zNotch = zTip + depth * notchFrac;
        float rNotch = tipRadius + (backRadius - tipRadius) * notchFrac;

        var backVerts = new Vector3[N];
        var tipVerts = new Vector3[N];
        var notchVerts = new Vector3[N];

        for (int i = 0; i < N; i++)
        {
            float a = AngleOffset + (i / (float)N) * MathF.PI * 2f;
            float cos = MathF.Cos(a), sin = MathF.Sin(a);
            backVerts[i] = TransformYRot(origin, rotation, cos * backRadius, sin * backRadius, zBack);
            tipVerts[i] = TransformYRot(origin, rotation, cos * tipRadius, sin * tipRadius, zTip);
            notchVerts[i] = TransformYRot(origin, rotation, cos * rNotch, sin * rNotch, zNotch);
        }

        float rightFaceX = MathF.Cos(AngleOffset + 0f / N * MathF.PI * 2f);
        float leftFaceX = MathF.Cos(AngleOffset + 3f / N * MathF.PI * 2f);

        var wingApexRight = TransformYRot(origin, rotation, rightFaceX * rNotch + wingSpread, 0f, zNotch);
        var wingApexLeft = TransformYRot(origin, rotation, leftFaceX * rNotch - wingSpread, 0f, zNotch);

        DrawArrowFaces(drawList, backVerts, tipVerts, notchVerts, wingApexLeft, wingApexRight, color);
    }

    /// <summary>
    /// Draws a 3D tapered octagon arrow with swept-back wings, with the tip pointing
    /// directly toward <paramref name="target"/> from <paramref name="origin"/>.
    /// Handles full 3D orientation — tilts up/down and left/right as needed.
    /// </summary>
    public static void DrawCrazyArrowToward(PctDrawList drawList, Vector3 origin, Vector3 target, float backRadius, float tipRadius, float depth, float wingSpread, float notchFrac, uint color)
    {
        var (right, up, forward) = LookAtBasis(origin, target);

        float zBack = depth / 2f;
        float zTip = -depth / 2f;
        float zNotch = zTip + depth * notchFrac;
        float rNotch = tipRadius + (backRadius - tipRadius) * notchFrac;

        var backVerts = new Vector3[N];
        var tipVerts = new Vector3[N];
        var notchVerts = new Vector3[N];

        for (int i = 0; i < N; i++)
        {
            float a = AngleOffset + (i / (float)N) * MathF.PI * 2f;
            float cos = MathF.Cos(a), sin = MathF.Sin(a);
            backVerts[i] = TransformBasis(origin, right, up, forward, cos * backRadius, sin * backRadius, zBack);
            tipVerts[i] = TransformBasis(origin, right, up, forward, cos * tipRadius, sin * tipRadius, zTip);
            notchVerts[i] = TransformBasis(origin, right, up, forward, cos * rNotch, sin * rNotch, zNotch);
        }

        float rightFaceX = MathF.Cos(AngleOffset + 0f / N * MathF.PI * 2f);
        float leftFaceX = MathF.Cos(AngleOffset + 3f / N * MathF.PI * 2f);

        var wingApexRight = TransformBasis(origin, right, up, forward, rightFaceX * rNotch + wingSpread, 0f, zNotch);
        var wingApexLeft = TransformBasis(origin, right, up, forward, leftFaceX * rNotch - wingSpread, 0f, zNotch);

        DrawArrowFaces(drawList, backVerts, tipVerts, notchVerts, wingApexLeft, wingApexRight, color);
    }

    private static void DrawArrowFaces(PctDrawList drawList, Vector3[] backVerts, Vector3[] tipVerts, Vector3[] notchVerts, Vector3 wingApexLeft, Vector3 wingApexRight, uint color)
    {
        // Back cap
        for (int i = 1; i < N - 1; i++)
            AddTri(drawList, backVerts[0], backVerts[i], backVerts[i + 1], color);

        // Shaft notch→back
        for (int i = 0; i < N; i++)
        {
            int j = (i + 1) % N;
            AddTri(drawList, backVerts[i], backVerts[j], notchVerts[j], color);
            AddTri(drawList, backVerts[i], notchVerts[j], notchVerts[i], color);
        }

        // Shaft tip→notch (all 8, wing faces intentionally overlap)
        for (int i = 0; i < N; i++)
        {
            int j = (i + 1) % N;
            AddTri(drawList, tipVerts[i], tipVerts[j], notchVerts[j], color);
            AddTri(drawList, tipVerts[i], notchVerts[j], notchVerts[i], color);
        }

        // Tip cap
        for (int i = 1; i < N - 1; i++)
            AddTri(drawList, tipVerts[0], tipVerts[i], tipVerts[i + 1], color);

        // Right wing (verts 0 and 7)
        AddTri(drawList, wingApexRight, tipVerts[0], tipVerts[7], color);
        AddTri(drawList, wingApexRight, notchVerts[7], notchVerts[0], color);
        AddTri(drawList, wingApexRight, tipVerts[0], notchVerts[0], color);
        AddTri(drawList, wingApexRight, notchVerts[7], tipVerts[7], color);

        // Left wing (verts 3 and 4)
        AddTri(drawList, wingApexLeft, tipVerts[3], tipVerts[4], color);
        AddTri(drawList, wingApexLeft, notchVerts[4], notchVerts[3], color);
        AddTri(drawList, wingApexLeft, tipVerts[3], notchVerts[3], color);
        AddTri(drawList, wingApexLeft, notchVerts[4], tipVerts[4], color);
    }

    /// <summary>Draws a triangle double-sided (both winding orders).</summary>
    private static void AddTri(PctDrawList drawList, Vector3 a, Vector3 b, Vector3 c, uint color)
    {
        drawList.AddTriangleFilled(a, b, c, color); // CW
        drawList.AddTriangleFilled(a, c, b, color); // CCW
    }

    /// <summary>
    /// Original Y-axis-only transform. localZ points along -Z before rotation.
    /// </summary>
    private static Vector3 TransformYRot(Vector3 origin, float rotation, float localX, float localY, float localZ)
    {
        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);
        return origin + new Vector3(
            localX * cos - localZ * sin,
            localY,
            localX * sin + localZ * cos
        );
    }

    /// <summary>
    /// Full 3D basis transform. localX maps to right, localY to up, localZ to -forward
    /// (negated so that positive localZ = back of arrow, negative localZ = tip).
    /// </summary>
    private static Vector3 TransformBasis(Vector3 origin, Vector3 right, Vector3 up, Vector3 forward, float localX, float localY, float localZ)
    {
        return origin + right * localX + up * localY + (-forward) * localZ;
    }

    /// <summary>
    /// Builds an orthonormal right/up/forward basis pointing from origin toward target.
    /// Falls back gracefully when the direction is nearly straight up or down.
    /// </summary>
    private static (Vector3 right, Vector3 up, Vector3 forward) LookAtBasis(Vector3 origin, Vector3 target)
    {
        var forward = Vector3.Normalize(target - origin);

        // If pointing nearly straight up/down, world-up is unreliable — use world-Z instead
        var worldUp = MathF.Abs(forward.Y) > 0.999f ? Vector3.UnitZ : Vector3.UnitY;

        var right = Vector3.Normalize(Vector3.Cross(worldUp, forward));
        var up = Vector3.Normalize(Vector3.Cross(forward, right));

        return (right, up, forward);
    }
}
