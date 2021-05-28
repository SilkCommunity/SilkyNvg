using Silk.NET.Maths;
using System;
using System.Collections.Generic;

namespace SilkyNvg.Common
{
    internal class Point
    {

        public Vector2D<float> Position { get; }

        public Vector2D<float> Determinant { get; private set; }

        public float Length { get; private set; }

        public Vector2D<float> MatrixDeterminant { get; private set; }

        public PointFlags Flags { get; set; }

        public Point(Vector2D<float> position, PointFlags flags)
        {
            Position = position;
            Flags = flags;
        }

        public void SetDeterminant(Point other)
        {
            Vector2D<float> determinant = other.Position - Position;
            Length = Maths.Normalize(ref determinant);
            Determinant = determinant;
        }

        public Vector4D<float>[] RoundJoin(float lw, float rw, float lu, float ru, uint ncap, Point other)
        {
            List<Vector4D<float>> verts = new();

            Vector2D<float> dl0 = new(other.Position.Y, -other.Position.X);
            Vector2D<float> dl1 = new(Position.Y, -Position.X);

            if (Flags.HasFlag(PointFlags.Left))
            {
                ChooseBevel(Flags.HasFlag(PointFlags.Innerbevel), other, this, lw, out Vector2D<float> l0, out Vector2D<float> l1);
                float a0 = MathF.Atan2(-dl0.Y, -dl0.X);
                float a1 = MathF.Atan2(-dl1.Y, -dl1.X);
                if (a1 > a0)
                {
                    a1 -= MathF.PI * 2.0f;
                }

                verts.Add(new(l0, lu, 1.0f));
                verts.Add(new(Position - dl0 * rw, ru, 1.0f));

                uint n = (uint)Maths.Clamp((int)MathF.Ceiling(((a0 - a1) / MathF.PI) * ncap), 2, ncap);
                for (int i = 0; i < n; i++)
                {
                    float u = i / (float)(n - 1);
                    float a = a0 + u * (a1 - a0);
                    float rx = Position.X + (MathF.Cos(a) * rw);
                    float ry = Position.Y + (MathF.Sin(a) * rw);
                    verts.Add(new(Position, 0.5f, 1.0f));
                    verts.Add(new(rx, ry, ru, 1.0f));
                }

                verts.Add(new(l1, lu, 1.0f));
                verts.Add(new(Position - (dl1 * rw), ru, 1.0f));
            }
            else
            {
                ChooseBevel(Flags.HasFlag(PointFlags.Innerbevel), other, this, -rw, out Vector2D<float> r0, out Vector2D<float> r1);
                float a0 = MathF.Atan2(dl0.Y, dl0.X);
                float a1 = MathF.Atan2(dl1.Y, dl1.X);
                if (a1 < a0)
                {
                    a1 += MathF.PI * 2.0f;
                }

                verts.Add(new(Position + dl0 * rw, lu, 1.0f));
                verts.Add(new(r0, ru, 1.0f));

                uint n = (uint)Maths.Clamp((int)MathF.Ceiling(((a1 - a0) / MathF.PI) * ncap), 2, ncap);
                for (int i = 0; i < n; i++)
                {
                    float u = i / (float)(n - 1);
                    float a = a0 + u * (a1 - a0);
                    float lx = Position.X + MathF.Cos(a) * lw;
                    float ly = Position.Y + MathF.Sin(a) * lw;
                    verts.Add(new(lx, ly, lu, 1.0f));
                    verts.Add(new(Position, 0.5f, 1.0f));
                }

                verts.Add(new(Position + dl1 * rw, lu, 1.0f));
                verts.Add(new(r1, ru, 1.0f));
            }

            return verts.ToArray();
        }

        private void BevelJoinLeft(float lw, float rw, float lu, float ru, Point other, Vector2D<float> dl0, Vector2D<float> dl1, List<Vector4D<float>> verts)
        {
            ChooseBevel(Flags.HasFlag(PointFlags.Innerbevel), other, this, lw, out Vector2D<float> l0, out Vector2D<float> l1);

            verts.Add(new(l0, lu, 1.0f));
            verts.Add(new(Position - l0 * rw, ru, 1.0f));

            if (Flags.HasFlag(PointFlags.Bevel))
            {
                verts.Add(new(l0, lu, 1.0f));
                verts.Add(new(Position - dl0 * rw, ru, 1.0f));

                verts.Add(new(l1, lu, 1.0f));
                verts.Add(new(Position - dl1 * rw, ru, 1.0f));
            }
            else
            {
                Vector2D<float> r0 = MatrixDeterminant * rw;

                verts.Add(new(Position, 0.5f, 1.0f));
                verts.Add(new(Position - dl0 * rw, ru, 1.0f));

                verts.Add(new(r0, ru, 1.0f));
                verts.Add(new(r0, ru, 1.0f));

                verts.Add(new(Position, 0.5f, 1.0f));
                verts.Add(new(Position - dl1 * rw, ru, 1.0f));
            }

            verts.Add(new(l1, lu, 1.0f));
            verts.Add(new(Position - dl1 * rw, ru, 1.0f));
        }

        private void BevelJoinRight(float lw, float rw, float lu, float ru, Point other, Vector2D<float> dl0, Vector2D<float> dl1, List<Vector4D<float>> verts)
        {
            ChooseBevel(Flags.HasFlag(PointFlags.Innerbevel), other, this, -rw, out Vector2D<float> r0, out Vector2D<float> r1);

            verts.Add(new(Position + r0 * lw, lu, 1.0f));
            verts.Add(new(r0, ru, 1.0f));

            if (Flags.HasFlag(PointFlags.Bevel))
            {
                verts.Add(new(Position + dl0 * lw, lu, 1.0f));
                verts.Add(new(r0, ru, 1.0f));

                verts.Add(new(Position + dl1 * lw, lu, 1.0f));
                verts.Add(new(r1, ru, 1.0f));
            }
            else
            {
                Vector2D<float> l0 = MatrixDeterminant * lw;

                verts.Add(new(Position + dl0 * lw, lu, 1.0f));
                verts.Add(new(Position, 0.5f, 1.0f));

                verts.Add(new(l0, lu, 1.0f));
                verts.Add(new(l0, lu, 1.0f));

                verts.Add(new(Position + dl1 * lw, lu, 1.0f));
                verts.Add(new(Position, 0.5f, 1.0f));
            }

            verts.Add(new(Position + dl1 * lw, lu, 1.0f));
            verts.Add(new(r1, ru, 1.0f));
        }

        public Vector4D<float>[] BevelJoin(float lw, float rw, float lu, float ru, Point other)
        {
            List<Vector4D<float>> verts = new();

            Vector2D<float> dl0 = new(other.Position.Y, -other.Position.X);
            Vector2D<float> dl1 = new(Position.Y, -Position.X);

            if (Flags.HasFlag(PointFlags.Left))
            {
                BevelJoinLeft(lw, rw, lu, ru, other, dl0, dl1, verts);
            }
            else
            {
                BevelJoinRight(lw, rw, lu, ru, other, dl0, dl1, verts);
            }

            return verts.ToArray();
        }

        private Vector4D<float>[] JoinBevelLeft(float lw, float rw, float lu, float ru, Vector2D<float> dl0, Vector2D<float> dl1, Point other)
        {
            List<Vector4D<float>> verts = new();

            ChooseBevel(Flags.HasFlag(PointFlags.Innerbevel), other, this, lw, out Vector2D<float> l0, out Vector2D<float> l1);

            verts.Add(new(l0, lu, 1));
            verts.Add(new(Position - dl0 * rw, ru, 1.0f));

            if (Flags.HasFlag(PointFlags.Bevel))
            {
                verts.Add(new(l0, lu, 1.0f));
                verts.Add(new(Position - dl0 * rw, ru, 1.0f));

                verts.Add(new(l1, lu, 1.0f));
                verts.Add(new(Position - dl1 * rw, ru, 1.0f));
            }
            else
            {
                Vector2D<float> r0 = Position - MatrixDeterminant * rw;

                verts.Add(new(Position, 0.5f, 1.0f));
                verts.Add(new(Position - dl0 * rw, ru, 1.0f));

                verts.Add(new(r0, ru, 1.0f));
                verts.Add(new(r0, ru, 1.0f));

                verts.Add(new(Position, 0.5f, 1.0f));
                verts.Add(new(Position - dl1 * rw, ru, 1.0f));
            }

            verts.Add(new(l1, lu, 1.0f));
            verts.Add(new(Position - dl1 * rw, ru, 1.0f));

            return verts.ToArray();
        }

        private Vector4D<float>[] JoinBevelRight(float lw, float rw, float lu, float ru, Vector2D<float> dl0, Vector2D<float> dl1, Point other)
        {
            List<Vector4D<float>> verts = new();

            ChooseBevel(Flags.HasFlag(PointFlags.Innerbevel), other, this, -rw, out Vector2D<float> r0, out Vector2D<float> r1);

            verts.Add(new(Position + dl0 * lw, lu, 1.0f));
            verts.Add(new(r0, ru, 1.0f));

            if (Flags.HasFlag(PointFlags.Bevel))
            {
                verts.Add(new(Position + dl0 * lw, lu, 1.0f));
                verts.Add(new(r0, ru, 1.0f));

                verts.Add(new(Position + dl1 * lw, lu, 1.0f));
                verts.Add(new(r1, ru, 1.0f));
            }
            else
            {
                Vector2D<float> l0 = Position + MatrixDeterminant * lw;

                verts.Add(new(Position + dl0 * lw, lu, 1.0f));
                verts.Add(new(Position, 0.5f, 1.0f));

                verts.Add(new(l0, lu, 1.0f));
                verts.Add(new(l0, lu, 1.0f));

                verts.Add(new(Position + dl1 * lw, lu, 1.0f));
                verts.Add(new(Position, 0.5f, 1.0f));
            }

            verts.Add(new(Position + dl1 * lw, lu, 1.0f));
            verts.Add(new(r1, ru, 1.0f));

            return verts.ToArray();
        }

        public Vector4D<float>[] JoinBevel(float lw, float rw, float lu, float ru, Vector2D<float> dl0, Vector2D<float> dl1, Point other)
        {
            if (Flags.HasFlag(PointFlags.Left))
            {
                return JoinBevelLeft(lw, rw, lu, ru, dl0, dl1, other);
            }
            else
            {
                return JoinBevelRight(lw, rw, lu, ru, dl0, dl1, other);
            }
        }

        public void Join(Point other, float iw, bool bevelOrRound, float miterLimit, ref uint nleft, ref uint bevelCount)
        {
            Vector2D<float> dl0 = new(other.Determinant.Y, -other.Determinant.X);
            Vector2D<float> dl1 = new(Determinant.Y, -Determinant.X);

            MatrixDeterminant = new((dl0.X + dl1.X) * 0.5f, (dl0.Y + dl1.Y) * 0.5f);

            float dmr2 = (MatrixDeterminant.X * MatrixDeterminant.X) + (MatrixDeterminant.Y * MatrixDeterminant.Y);
            if (dmr2 > 0.000001f)
            {
                float scale = 1.0f / dmr2;
                scale = MathF.Min(scale, 600.0f);
                MatrixDeterminant *= scale;
            }

            Flags = ((Flags & PointFlags.Corner) != 0) ? PointFlags.Corner : 0;

            float cross = (Determinant.X * other.Determinant.Y) - (other.Determinant.X * Determinant.Y);
            if (cross > 0.0f)
            {
                nleft++;
                Flags |= PointFlags.Left;
            }

            float limit = MathF.Max(1.0f, MathF.Min(other.Length, Length) * iw);
            if ((dmr2 * limit * limit) < 1.0f)
            {
                Flags |= PointFlags.Innerbevel;
            }

            if ((Flags & PointFlags.Corner) != 0)
            {
                if ((dmr2 * miterLimit * miterLimit) < 1.0f || bevelOrRound)
                {
                    Flags |= PointFlags.Bevel;
                }
            }

            if ((Flags & (PointFlags.Bevel | PointFlags.Innerbevel)) != 0)
            {
                bevelCount++;
            }
        }

        public static bool Equals(Vector2D<float> p0, Vector2D<float> p1, float tol)
        {
            return Maths.PtEquals(p0, p1, tol);
        }

        public static bool Equals(Point p0, Point p1, float tol)
        {
            return Maths.PtEquals(p0.Position, p1.Position, tol);
        }

        public static float PolyArea(IList<Point> points)
        {
            float area = 0;
            for (int i = 2; i < points.Count; i++)
            {
                Point a = points[0];
                Point b = points[i - 1];
                Point c = points[i];
                area += Maths.Triarea2(a.Position, b.Position, c.Position);
            }

            return area * 0.5f;
        }

        public static Vector2D<float>[] Vertex(Point p0, Point p1, float woff)
        {
            if ((p1.Flags & PointFlags.Bevel) != 0)
            {
                Vector2D<float> dl0 = new(p0.Determinant.Y, -p0.Determinant.X);
                Vector2D<float> dl1 = new(p1.Determinant.Y, -p1.Determinant.X);

                if ((p1.Flags & PointFlags.Left) != 0)
                {
                    Vector2D<float> l = p1.Position + p1.MatrixDeterminant * woff;
                    return new Vector2D<float>[] { l };
                }
                else
                {
                    Vector2D<float> l0 = (p1.Position + dl0) * woff;
                    Vector2D<float> l1 = (p1.Position + dl1) * woff;
                    return new Vector2D<float>[]
                    {
                        l0,
                        l1
                    };
                }
            }
            else
            {
                return new Vector2D<float>[] { p1.Position + (p1.MatrixDeterminant * woff) };
            }
        }

        private static void ChooseBevel(bool bevel, Point p0, Point p1, float w, out Vector2D<float> pos0, out Vector2D<float> pos1)
        {
            if (bevel)
            {
                pos0 = new(p1.Position.X + p0.Determinant.Y * w, p1.Position.Y - p0.Determinant.X * w);
                pos1 = new(p1.Position.X + p0.Determinant.Y * w, p1.Position.Y - p0.Determinant.X * w);
            }
            else
            {
                pos0 = new(p1.Position.X + p0.MatrixDeterminant.Y * w, p1.Position.Y + p0.MatrixDeterminant.X * w);
                pos1 = new(p1.Position.X + p0.MatrixDeterminant.Y * w, p1.Position.Y + p0.MatrixDeterminant.X * w);
            }
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.Position - b.Position, a.Flags);
        }

    }
}
