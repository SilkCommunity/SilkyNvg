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
        
        private readonly List<SubPath> _subPaths = [];
        
        private bool _needNewSubPath = true;
        
        private bool HasSubPaths => _subPaths.Count > 0;
        
        private SubPath CurrentSubPath => _subPaths[^1];
        
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

        internal void FillPath(GeometryBuilder geometryBuilder)
        {
            if (!HasSubPaths)
            {
                return;
            }

            Vector2 start = _subPaths[0].Start;
            geometryBuilder.BeginPath(start);
            foreach (var subPath in _subPaths)
            {
                subPath.BuildFillGeometry(geometryBuilder);
            }
            geometryBuilder.EndPath();
        }

        private void CreateNewSubpath(Vector2 start)
        {
            _subPaths.Add(new SubPath(start));
            _needNewSubPath = false;
        }

        private void EnsureSubPathExists(Vector2 p)
        {
            if (_needNewSubPath)
            {
                CreateNewSubpath(p);
            }
        }

        #region Path

        public void ClosePath()
        {
            if (!HasSubPaths)
            {
                return;
            }
            
            CurrentSubPath.Close();
            CreateNewSubpath(CurrentSubPath.Start);
        }

        public void MoveTo(Vector2 p)
        {
            if (p.IsInfinityOrNan())
            {
                return;
            }
            CreateNewSubpath(p);
        }

        public void MoveTo(float x, float y)
            => MoveTo(new Vector2(x, y));

        public void LineTo(Vector2 p)
        {
            if (p.IsInfinityOrNan())
            {
                return;
            }

            if (!HasSubPaths)
            {
                EnsureSubPathExists(p);
            }
            else
            {
                CurrentSubPath.AddLine(p);
            }
        }
        
        public void LineTo(float x, float y)
            => LineTo(new Vector2(x, y));

        public void QuadraticCurveTo(Vector2 cp, Vector2 p)
        {
            if (cp.IsInfinityOrNan() || p.IsInfinityOrNan())
            {
                return;
            }
            
            EnsureSubPathExists(cp);
            CurrentSubPath.AddQuadratic(cp, p);
        }

        public void QuadraticCurveTo(float cpx, float cpy, float x, float y)
            => QuadraticCurveTo(new Vector2(cpx, cpy), new Vector2(x, y));

        public void BezierCurveTo(Vector2 cp1, Vector2 cp2, Vector2 p)
        {
            if (cp1.IsInfinityOrNan() || cp2.IsInfinityOrNan() || p.IsInfinityOrNan())
            {
                return;
            }
            
            EnsureSubPathExists(cp1);
            CurrentSubPath.AddCubic(cp1, cp2, p);
        }
        
        public void BezierCurveTo(float cp1x, float cp1y, float cp2x, float cp2y, float x, float y)
            => BezierCurveTo(new Vector2(cp1x, cp1y), new Vector2(cp2x, cp2y), new Vector2(x, y));

        public void ArcTo(Vector2 p1, Vector2 p2, float radius)
        {
            if (p1.IsInfinityOrNan() || p2.IsInfinityOrNan() || radius.IsInfinityOrNan())
            {
                return;
            }
            
            EnsureSubPathExists(p1);

            if (radius < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius cannot be negative!");
            }

            CurrentSubPath.AddArcTo(p1, p2, radius);
        }
        
        public void ArcTo(float x1, float y1, float x2, float y2, float radius)
            => ArcTo(new Vector2(x1, y1), new Vector2(x2, y2), radius);

        public void Arc(Vector2 origin, float radius, float startAngle, float endAngle, bool counterclockwise = false)
            => Ellipse(origin, radius, radius, 0, startAngle, endAngle, counterclockwise);
        
        public void Arc(float x, float y, float radius, float startAngle, float endAngle, bool counterclockwise = false)
            => Arc(new Vector2(x, y), radius, startAngle, endAngle, counterclockwise);
        
        public void Ellipse(Vector2 origin, float radiusX, float radiusY, float rotation, float startAngle,
            float endAngle, bool counterclockwise = false)
        {
            if (origin.IsInfinityOrNan() || radiusX.IsInfinityOrNan() || radiusY.IsInfinityOrNan() ||
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

            var rotTransform = Matrix3x2.CreateRotation(-rotation);
            Vector2 startPoint = Maths.PointOnEllipse(origin, radiusX, radiusY, rotTransform, cwStartAngle);
            Vector2 endPoint = Maths.PointOnEllipse(origin, radiusX, radiusY, rotTransform, cwEndAngle);
            
            if (HasSubPaths)
            {
                CurrentSubPath.AddLine(startPoint);
            }
            else
            {
                CreateNewSubpath(startPoint);
            }
            
            CurrentSubPath.AddEllipse(origin, radiusX, radiusY, cwStartAngle, cwEndAngle, rotation, startPoint,
                endPoint);
        }

        public void Ellipse(float x, float y, float radiusX, float radiusY, float rotation, float startAngle,
            float endAngle, bool counterclockwise = false)
            => Ellipse(new Vector2(x, y), radiusX, radiusY, rotation, startAngle, endAngle, counterclockwise);

        public void Rect(Vector2 p, Vector2 s)
        {
            if (p.IsInfinityOrNan() || s.IsInfinityOrNan())
            {
                return;
            }
            
            CreateNewSubpath(p);
            CurrentSubPath.AddLine(p);
            CurrentSubPath.AddLine(p + s.X * Vector2.UnitX);
            CurrentSubPath.AddLine(p + s);
            CurrentSubPath.AddLine(p + s.Y * Vector2.UnitY);
            CurrentSubPath.Close();
            
            CreateNewSubpath(p);
        }

        public void Rect(float x, float y, float w, float h)
            => Rect(new Vector2(x, y), new Vector2(w, h));

        public void RoundRect(Vector2 p, Vector2 s, params Vector2[] radii)
        {
            if (p.IsInfinityOrNan() || s.IsInfinityOrNan())
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

            float scale = MathF.Min(MathF.Min(MathF.Abs(s.X / top), MathF.Abs(s.Y / right)), MathF.Min(MathF.Abs(s.X / bottom),
                MathF.Abs(s.Y / left)));
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
            if (s.X < 0)
            {
                p = p with { X = p.X + s.X };
                s.X = -s.X;

                temp = upperLeft;
                upperLeft = upperRight;
                upperRight = temp;
                temp = lowerLeft;
                lowerLeft = lowerRight;
                lowerRight = temp;
            }

            if (s.Y < 0)
            {
                p = p with { Y = p.Y + s.Y };
                s.Y = -s.Y;

                temp = upperLeft;
                upperLeft = lowerLeft;
                lowerLeft = temp;
                temp = upperRight;
                upperRight = lowerRight;
                lowerRight = temp;
            }

            CreateNewSubpath(p with { X = p.X + upperLeft.X });
            CurrentSubPath.AddLine(p with { X = p.X + s.X - upperRight.X });
            CurrentSubPath.AddEllipse(
                new Vector2(
                    p.X + s.X - upperRight.X,
                    p.Y + upperRight.Y
                ),
                upperRight.X, upperRight.Y, MathF.PI * 1.5f, MathF.Tau, 0,
                p with { X = p.X + s.X - upperRight.X },
                p with { X = p.X + s.X, Y = p.Y + upperRight.Y }
            );
            CurrentSubPath.AddLine(p with { X = p.X + s.X, Y = p.Y + s.Y - lowerRight.Y });
            CurrentSubPath.AddEllipse(
                new Vector2(
                    p.X + s.X - lowerRight.X,
                    p.Y + s.Y - lowerRight.Y
                ),
                lowerRight.X, lowerRight.Y, 0, MathF.PI * 0.5f, 0,
                p with { X = p.X + s.X, Y = p.Y + s.Y - lowerRight.Y },
                p with { X = p.X + s.X - lowerRight.X, Y = p.Y + s.Y }
            );
            CurrentSubPath.AddLine(p with { X = p.X + lowerLeft.X, Y = p.Y + s.Y });
            CurrentSubPath.AddEllipse(
                new Vector2(
                    p.X + lowerLeft.X,
                    p.Y + s.Y - lowerLeft.Y
                ),
                lowerLeft.X, lowerLeft.Y, MathF.PI * 0.5f, MathF.PI, 0,
                p with { X = p.X + lowerLeft.X, Y = p.Y + s.Y - lowerLeft.Y },
                p with { X = p.X, Y = p.Y + s.Y - lowerLeft.Y }
            );
            CurrentSubPath.AddLine(p with { X = p.X, Y = p.Y + upperLeft.Y });
            CurrentSubPath.AddEllipse(
                new Vector2(
                    p.X + upperLeft.X,
                    p.Y + upperLeft.Y
                ),
                upperLeft.X, upperLeft.Y, MathF.PI, MathF.PI * 1.5f, 0,
                p with { X = p.X, Y = p.Y + upperLeft.Y },
                p with { X = p.X + upperLeft.X, Y = p.Y }
            );
            CurrentSubPath.Close();
        }

        public void RoundRect(Vector2 p, Vector2 s, params float[] radii)
        {
           var temp = new Vector2[radii.Length];
            for (int i = 0; i < radii.Length; i++)
            {
                temp[i] = new Vector2(radii[i], radii[i]);
            }

            RoundRect(p, s, temp);
        }

        public void RoundRect(float x, float y, float w, float h, params Vector2[] radii)
            => RoundRect(new Vector2(x, y), new Vector2(w, h), radii);

        public void RoundRect(float x, float y, float w, float h, params float[] radii)
            => RoundRect(new Vector2(x, y), new Vector2(w, h), radii);

        #endregion

    }
}