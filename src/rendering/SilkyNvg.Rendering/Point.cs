using SilkyNvg.Common;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SilkyNvg.Rendering
{
    internal class Point
    {

        public Vector2 Position { get; }

        public Vector2 Determinant { get; private set; }

        public float Length { get; private set; }

        public Vector2 MatrixDeterminant { get; private set; }

        public PointFlags Flags { get; internal set; }

        public Point(Vector2 position, PointFlags flags)
        {
            Position = position;
            Flags = flags;
        }

        public void SetDeterminant(Point other)
        {
            Vector2 determinant = other.Position - Position;
            Length = Maths.Normalize(ref determinant);
            Determinant = determinant;
        }

        public void RoundJoin(float lw, float rw, float lu, float ru, uint ncap, Point other, ICollection<Vertex> verts)
        {
            Vector2 dl0 = new(other.Determinant.Y, -other.Determinant.X);
            Vector2 dl1 = new(Determinant.Y, -Determinant.X);

            if (Flags.HasFlag(PointFlags.Left))
            {
                ChooseBevel(Flags.HasFlag(PointFlags.Innerbevel), other, this, lw, out Vector2 l0, out Vector2 l1);
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
                ChooseBevel(Flags.HasFlag(PointFlags.Innerbevel), other, this, -rw, out Vector2 r0, out Vector2 r1);
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
        }

        private void BevelJoinLeft(float lw, float rw, float lu, float ru, Point other, Vector2 dl0, Vector2 dl1, ICollection<Vertex> verts)
        {
            ChooseBevel(Flags.HasFlag(PointFlags.Innerbevel), other, this, lw, out Vector2 l0, out Vector2 l1);

            verts.Add(new(l0, lu, 1.0f));
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
                Vector2 r0 = Position - MatrixDeterminant * rw;

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

        private void BevelJoinRight(float lw, float rw, float lu, float ru, Point other, Vector2 dl0, Vector2 dl1, ICollection<Vertex> verts)
        {
            ChooseBevel(Flags.HasFlag(PointFlags.Innerbevel), other, this, -rw, out Vector2 r0, out Vector2 r1);

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
                Vector2 l0 = MatrixDeterminant * lw;

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

        public void BevelJoin(float lw, float rw, float lu, float ru, Point other, ICollection<Vertex> verts)
        {
            Vector2 dl0 = new(other.Determinant.Y, -other.Determinant.X);
            Vector2 dl1 = new(Determinant.Y, -Determinant.X);

            if (Flags.HasFlag(PointFlags.Left))
            {
                BevelJoinLeft(lw, rw, lu, ru, other, dl0, dl1, verts);
            }
            else
            {
                BevelJoinRight(lw, rw, lu, ru, other, dl0, dl1, verts);
            }
        }

        private void JoinBevelLeft(float lw, float rw, float lu, float ru, Vector2 dl0, Vector2 dl1, Point other, ICollection<Vertex> verts)
        {
            ChooseBevel(Flags.HasFlag(PointFlags.Innerbevel), other, this, lw, out Vector2 l0, out Vector2 l1);

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
                Vector2 r0 = Position - MatrixDeterminant * rw;

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

        private void JoinBevelRight(float lw, float rw, float lu, float ru, Vector2 dl0, Vector2 dl1, Point other, ICollection<Vertex> verts)
        {
            ChooseBevel(Flags.HasFlag(PointFlags.Innerbevel), other, this, -rw, out Vector2 r0, out Vector2 r1);

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
                Vector2 l0 = Position + MatrixDeterminant * lw;

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

        public void JoinBevel(float lw, float rw, float lu, float ru, Vector2 dl0, Vector2 dl1, Point other, ICollection<Vertex> verts)
        {
            if (Flags.HasFlag(PointFlags.Left))
            {
                JoinBevelLeft(lw, rw, lu, ru, dl0, dl1, other, verts);
            }
            else
            {
                JoinBevelRight(lw, rw, lu, ru, dl0, dl1, other, verts);
            }
        }

        public void Join(Point other, float iw, bool bevelOrRound, float miterLimit, ref uint nleft, ref uint bevelCount)
        {
            Vector2 dl0 = new(other.Determinant.Y, -other.Determinant.X);
            Vector2 dl1 = new(Determinant.Y, -Determinant.X);

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

            float limit = MathF.Max(1.01f, MathF.Min(other.Length, Length) * iw);
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

        public static bool Equals(Vector2 p0, Vector2 p1, float tol)
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

        public static void Vertex(Point p0, Point p1, float woff, ICollection<Vertex> verts)
        {
            if ((p1.Flags & PointFlags.Bevel) != 0)
            {
                Vector2 dl0 = new(p0.Determinant.Y, -p0.Determinant.X);
                Vector2 dl1 = new(p1.Determinant.Y, -p1.Determinant.X);

                if ((p1.Flags & PointFlags.Left) != 0)
                {
                    Vector2 l = p1.Position + p1.MatrixDeterminant * woff;
                    verts.Add(new(l, 0.5f, 1.0f));
                }
                else
                {
                    Vector2 l0 = (p1.Position + dl0) * woff;
                    Vector2 l1 = (p1.Position + dl1) * woff;
                    verts.Add(new(l0, 0.5f, 1.0f));
                    verts.Add(new(l1, 0.5f, 1.0f));
                }
            }
            else
            {
                verts.Add(new(p1.Position + (p1.MatrixDeterminant * woff), 0.5f, 1.0f));
            }
        }

        private static void ChooseBevel(bool bevel, Point p0, Point p1, float w, out Vector2 pos0, out Vector2 pos1)
        {
            if (bevel)
            {
                pos0 = new(p1.Position.X + p0.Determinant.Y * w, p1.Position.Y - p0.Determinant.X * w);
                pos1 = new(p1.Position.X + p1.Determinant.Y * w, p1.Position.Y - p1.Determinant.X * w);
            }
            else
            {
                pos0 = new(p1.Position.X + p1.MatrixDeterminant.X * w, p1.Position.Y + p1.MatrixDeterminant.Y * w);
                pos1 = new(p1.Position.X + p1.MatrixDeterminant.X * w, p1.Position.Y + p1.MatrixDeterminant.Y * w);
            }
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.Position - b.Position, a.Flags);
        }

    }
}
