using System;
using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Paths;
using SilkyNvg.Rendering;
using SilkyNvg.Utils;

namespace SilkyNvg
{
    public class Path2D()
    {
        
        private readonly List<Subpath> _subPaths = [];
        
        private bool _needNewSubPath = true;
        
        private bool HasSubPaths => _subPaths.Count > 0;
        
        private Subpath CurrentSubpath => _subPaths[^1];
        
        public Path2D(Path2D path)
            : this()
        {
            
        }

        public Path2D(string path)
            : this()
        {
            
        }

        public void AddPath(Path2D path, Matrix3x2 transform = default)
        {
            
        }

        internal void ClearSubPaths()
        {
            _subPaths.Clear();
        }

        internal void FillPath()
        {
            if (!HasSubPaths)
            {
                return;
            }
            
        }

        private void CreateNewSubpath(float startX, float startY)
        {
            _subPaths.Add(new Subpath(startX, startY));
            _needNewSubPath = false;
        }

        private void EnsureSubPathExists(float x, float y)
        {
            if (_needNewSubPath)
            {
                CreateNewSubpath(x, y);
            }
        }

        #region Path

        public void ClosePath()
        {
            if (!HasSubPaths)
            {
                return;
            }
            
            CurrentSubpath.Close();
            CreateNewSubpath(CurrentSubpath.Start.X, CurrentSubpath.Start.Y);
        }

        public void MoveTo(Vector2 p)
            => MoveTo(p.X, p.Y);

        public void MoveTo(float x, float y)
        {
            if (x.IsInfinityOrNan() || y.IsInfinityOrNan())
            {
                return;
            }
            CreateNewSubpath(x, y);
        }

        public void LineTo(Vector2 p)
            => LineTo(p.X, p.Y);

        public void LineTo(float x, float y)
        {
            if (x.IsInfinityOrNan() || y.IsInfinityOrNan())
            {
                return;
            }

            if (!HasSubPaths)
            {
                EnsureSubPathExists(x, y);
            }
            else
            {
                CurrentSubpath.AddLine(x, y);
            }
        }

        public void QuadraticCurveTo(Vector2 cp, Vector2 p)
            => QuadraticCurveTo(cp.X, cp.Y, p.X, p.Y);

        public void QuadraticCurveTo(float cpx, float cpy, float x, float y)
        {
            if (cpx.IsInfinityOrNan() || cpy.IsInfinityOrNan() || x.IsInfinityOrNan() || y.IsInfinityOrNan())
            {
                return;
            }
            
            EnsureSubPathExists(cpx, cpy);
            CurrentSubpath.AddQuadratic(cpx, cpy, x, y);
        }

        public void BezierCurveTo(Vector2 cp1, Vector2 cp2, Vector2 p)
            => BezierCurveTo(cp1.X, cp1.Y, cp2.X, cp2.Y, p.X, p.Y);

        public void BezierCurveTo(float cp1x, float cp1y, float cp2x, float cp2y, float x, float y)
        {
            if (cp1x.IsInfinityOrNan() || cp1y.IsInfinityOrNan() || cp2x.IsInfinityOrNan() || cp2y.IsInfinityOrNan()
                || x.IsInfinityOrNan() || y.IsInfinityOrNan())
            {
                return;
            }
            
            EnsureSubPathExists(cp1x, cp1y);
            CurrentSubpath.AddCubic(cp1x, cp1y, cp2x, cp2y, x, y);
        }

        public void ArcTo(Vector2 p1, Vector2 p2, float radius)
            => ArcTo(p1.X, p1.Y, p2.X, p2.Y, radius);

        public void ArcTo(float x1, float y1, float x2, float y2, float radius)
        {
            if (x1.IsInfinityOrNan() || y1.IsInfinityOrNan() || x2.IsInfinityOrNan() || y2.IsInfinityOrNan()
                || radius.IsInfinityOrNan())
            {
                return;
            }
            
            EnsureSubPathExists(x1, y1);

            if (radius < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius cannot be negative!");
            }

            CurrentSubpath.AddArcTo(x1, y1, x2, y2, radius);
        }

        public void Arc(Vector2 origin, float radius, float startAngle, float endAngle, bool counterclockwise = false)
            => Ellipse(origin, radius, radius, 0, startAngle, endAngle, counterclockwise);
        
        public void Arc(float x, float y, float radius, float startAngle, float endAngle, bool counterclockwise = false)
            => Arc(new Vector2(x, y), radius, startAngle, endAngle, counterclockwise);

        public void Ellipse(Vector2 origin, float radiusX, float radiusY, float rotation, float startAngle,
            float endAngle, bool counterclockwise = false)
            => Ellipse(origin.X, origin.Y, radiusX, radiusY, rotation, startAngle, endAngle, counterclockwise);

        public void Ellipse(float x, float y, float radiusX, float radiusY, float rotation, float startAngle,
            float endAngle, bool counterclockwise = false)
        {
            if (x.IsInfinityOrNan() || y.IsInfinityOrNan() || radiusX.IsInfinityOrNan() || radiusY.IsInfinityOrNan() ||
                rotation.IsInfinityOrNan() || startAngle.IsInfinityOrNan() || endAngle.IsInfinityOrNan())
            {
                return;
            }

            if (radiusX < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radiusX), "Radius cannot be negative!");
            }

            if (radiusY < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radiusY), "Radius cannot be negative!");
            }
            
            // Always be counterclockwise (mathematically positive direction)
            float cwStartAngle = startAngle;
            float cwEndAngle = endAngle;
            
            if (counterclockwise)
            {
                // NOTE: No minus-sign here!! This is because html canvas is stupid
                // and measures angles in clockwise direction for some stupid reason.
                cwStartAngle = endAngle;
                cwEndAngle = startAngle;
            }

            cwStartAngle = Maths.NormaliseAngle(cwStartAngle);
            cwEndAngle = Maths.NormaliseAngle(cwEndAngle);

            if (cwEndAngle <= cwStartAngle)
                cwEndAngle += MathF.Tau;

            // We need to calculate this here since the start point is required for creating a new subpath.
            // This shouldn't be too expensive though, especially since we automatically cache the starting point.
            var ellipseTransform = Matrix3x2.CreateRotation(-rotation);
            ellipseTransform.Translation = new Vector2(x, y);
            Vector2 startPoint = Vector2.Transform(Maths.PointOnEllipse(radiusX, radiusY, cwStartAngle), ellipseTransform);
            
            if (HasSubPaths)
            {
                CurrentSubpath.AddLine(startPoint.X, startPoint.Y);
            }
            else
            {
                CreateNewSubpath(startPoint.X, startPoint.Y);
            }
            
            CurrentSubpath.AddEllipse(x, y, radiusX, radiusY, cwStartAngle, cwEndAngle, rotation);
        }

        public void Rect(Vector2 p, Vector2 s)
            => Rect(p.X, p.Y, s.X, s.Y);

        public void Rect(float x, float y, float w, float h)
        {
            if (x.IsInfinityOrNan() || y.IsInfinityOrNan() || w.IsInfinityOrNan() || h.IsInfinityOrNan())
            {
                return;
            }
            
            CreateNewSubpath(x, y);
            
            CurrentSubpath.AddLine(x + w, y);
            CurrentSubpath.AddLine(x + w, y + h);
            CurrentSubpath.AddLine(x, y + h);
            CurrentSubpath.Close();
            
            CreateNewSubpath(x, y);
        }

        public void RoundRect(Vector2 p, Vector2 s, params Vector2[] radii)
            => RoundRect(p.X, p.Y, s.X, s.Y, radii);

        public void RoundRect(Vector2 p, Vector2 s, params float[] radii)
            => RoundRect(p.X, p.Y, s.X, s.Y, radii);

        public void RoundRect(float x, float y, float w, float h, params float[] radii)
        {
            var temp = new Vector2[radii.Length];
            for (int i = 0; i < radii.Length; i++)
            {
                temp[i] = new Vector2(radii[i], radii[i]);
            }

            RoundRect(x, y, w, h, temp);
        }

        public void RoundRect(float x, float y, float w, float h, params Vector2[] radii)
        {
            if (x.IsInfinityOrNan() || y.IsInfinityOrNan() || w.IsInfinityOrNan() || h.IsInfinityOrNan())
            {
                return;
            }

            if (radii.Length is < 1 or > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(radii), "Radii must be a list of 1, 2, 3 or 4 radii.");
            }

            Span<Vector2> normalizedRadii = stackalloc Vector2[radii.Length];
            for (int i = 0; i < radii.Length; i++)
            {
                if (radii[i].IsInfinityOrNan())
                {
                    return;
                }

                if (radii[i].X < 0 || radii[i].Y < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(radii), "Radius cannot be negative!");
                }

                normalizedRadii[i] = radii[i];
            }

            Vector2 upperLeft, upperRight, lowerRight, lowerLeft;
            switch (normalizedRadii.Length)
            {
                case 4:
                    upperLeft = normalizedRadii[0];
                    upperRight = normalizedRadii[1];
                    lowerRight = normalizedRadii[2];
                    lowerLeft = normalizedRadii[3];
                    break;
                case 3:
                    upperLeft = normalizedRadii[0];
                    upperRight = lowerLeft = normalizedRadii[1];
                    lowerRight = normalizedRadii[2];
                    break;
                case 2:
                    upperLeft = lowerRight = normalizedRadii[0];
                    upperRight = lowerLeft = normalizedRadii[1];
                    break;
                case 1:
                    upperLeft = upperRight = lowerRight = lowerLeft = normalizedRadii[0];
                    break;
                default:
                    upperLeft = upperRight = lowerRight = lowerLeft = Vector2.Zero;
                    break;
            }

            float top = upperLeft.X + upperRight.X;
            float right = upperRight.Y + lowerRight.Y;
            float bottom = lowerRight.X + lowerLeft.X;
            float left = upperLeft.Y + lowerLeft.Y;

            float scale = MathF.Min(MathF.Min(MathF.Abs(w / top), MathF.Abs(h / right)), MathF.Min(MathF.Abs(w / bottom),
                MathF.Abs(h / left)));
            if (scale < 1)
            {
                upperLeft *= scale;
                upperRight *= scale;
                lowerLeft *= scale;
                lowerRight *= scale;
            }
            
            /*
             * It is not officially stated in the specification for HTML Canvas, that negative values for w or h
             * flip the rectangle. This seems to be common practice however, therefore we implement it here.
             */
            Vector2 temp;
            if (w < 0)
            {
                x = x + w;
                w = -w;

                temp = upperLeft;
                upperLeft = upperRight;
                upperRight = temp;
                temp = lowerLeft;
                lowerLeft = lowerRight;
                lowerRight = temp;
            }

            if (h < 0)
            {
                y = y + h;
                y = -y;

                temp = upperLeft;
                upperLeft = lowerLeft;
                lowerLeft = temp;
                temp = upperRight;
                upperRight = lowerRight;
                lowerRight = temp;
            }

            CreateNewSubpath(x + upperLeft.X, y);
            CurrentSubpath.AddLine(x + w - upperRight.X, y);
            CurrentSubpath.AddEllipse(
                x + w - upperRight.X, y + upperRight.Y,
                upperRight.X, upperRight.Y,
                MathF.PI * 1.5f, MathF.Tau, 0
            );
            CurrentSubpath.AddLine(x + w, y + h - lowerRight.Y);
            CurrentSubpath.AddEllipse(
                x + w - lowerRight.X,
                y + h - lowerRight.Y,
                lowerRight.X, lowerRight.Y,
                0, MathF.PI * 0.5f, 0
            );
            CurrentSubpath.AddLine(x + lowerLeft.X, y + h);
            CurrentSubpath.AddEllipse(
                x + lowerLeft.X,
                y + h - lowerLeft.Y,
                lowerLeft.X, lowerLeft.Y,
                MathF.PI * 0.5f, MathF.PI, 0
            );
            CurrentSubpath.AddLine(x, y + upperLeft.Y);
            CurrentSubpath.AddEllipse(
                x + upperLeft.X,
                y + upperLeft.Y,
                upperLeft.X, upperLeft.Y,
                MathF.PI, MathF.PI * 1.5f, 0
            );
            CurrentSubpath.Close();
        }

        #endregion

    }
}