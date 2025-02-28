using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Extensions.Svg.Parser.Attributes;
using SilkyNvg.Extensions.Svg.Parser.Constants;
using SilkyNvg.Extensions.Svg.Parser.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser.Elements
{
    internal class PathElementParser : ISvgElementParser
    {

        private delegate bool ParseInstruction(StringSource source);

        private readonly List<IInstruction> _instructions = [];

        private readonly Dictionary<char, ParseInstruction> _instructionParsers;
        private readonly SvgParser _parser;

        private Vector2D<float> _currentPosition;
        private Vector2D<float>? _lastControlPointCS, _lastControlPointQT;

        internal PathElementParser(SvgParser parser)
        {
            _parser = parser;
            _instructionParsers = new()
            {
                ['M'] = source => ParseMoveTo(source, false),
                ['m'] = source => ParseMoveTo(source, true),
                ['L'] = source => ParseLineTo(source, false),
                ['l'] = source => ParseLineTo(source, true),

                ['Z'] = source => ParseClose(),
                ['z'] = source => ParseClose(),

                ['H'] = source => ParseHorizontalLineTo(source, false),
                ['h'] = source => ParseHorizontalLineTo(source, true),
                ['V'] = source => ParseVerticalLineTo(source, false),
                ['v'] = source => ParseVerticalLineTo(source, true),

                ['C'] = source => ParseCurveTo(source, false),
                ['c'] = source => ParseCurveTo(source, true),
                ['S'] = source => ParseSmoothCurveTo(source, false),
                ['s'] = source => ParseSmoothCurveTo(source, true),

                ['Q'] = source => ParseQuadraticBezierCurveTo(source, false),
                ['q'] = source => ParseQuadraticBezierCurveTo(source, true),
                ['T'] = source => ParseSmoothQuadraticBezierCurveTo(source, false),
                ['t'] = source => ParseSmoothQuadraticBezierCurveTo(source, true),

                ['A'] = source => ParseEllipticalArc(source, false),
                ['a'] = source => ParseEllipticalArc(source, true)
            };
        }

        private bool ParseMoveTo(StringSource source, bool relative)
        {
            var sequence = source.ParseCoordinatePairSequence();
            if (sequence == null)
            {
                return false;
            }
            Vector2D<float> pos = default;
            foreach (var pair in sequence)
            {
                pos = relative ? (_currentPosition + pair) : pair;
                _instructions.Add(new MoveToInstruction(pos));
            }
            _currentPosition = pos;
            return true;
        }

        private bool ParseLineTo(StringSource source, bool relative)
        {
            var sequence = source.ParseCoordinatePairSequence();
            if (sequence == null)
            {
                return false;
            }
            Vector2D<float> pos = default;
            foreach (var pair in sequence)
            {
                pos = relative ? (_currentPosition + pair) : pair;
                _instructions.Add(new LineToInstruction(pos));
            }
            _currentPosition = pos;
            return true;
        }

        private bool ParseClose()
        {
            _instructions.Add(new CloseInstruction());
            return true;
        }

        private bool ParseHorizontalLineTo(StringSource source, bool relative)
        {
            var sequence = source.ParseCoordinateSequence();
            if (sequence == null)
            {
                return false;
            }
            Vector2D<float> pos = default;
            foreach (var coord in sequence)
            {
                pos = new Vector2D<float>(relative ? (_currentPosition.X + coord) : coord, _currentPosition.Y);
                _instructions.Add(new LineToInstruction(pos));
            }
            _currentPosition = pos;
            return true;
        }

        private bool ParseVerticalLineTo(StringSource source, bool relative)
        {
            var sequence = source.ParseCoordinateSequence();
            if (sequence == null)
            {
                return false;
            }
            Vector2D<float> pos = default;
            foreach (var coord in sequence)
            {
                pos = new Vector2D<float>(_currentPosition.X, relative ? (_currentPosition.Y + coord) : coord);
                _instructions.Add(new LineToInstruction(pos));
            }
            _currentPosition = pos;
            return true;
        }

        private bool ParseCurveTo(StringSource source, bool relative)
        {
            var sequence = ParseCurveToCoordinateSequence(source);
            if (sequence == null)
            {
                return false;
            }
            foreach (var triplet in sequence)
            {
                Vector2D<float> p0 = relative ? (_currentPosition + triplet[0]) : triplet[0];
                Vector2D<float> p1 = relative ? (_currentPosition + triplet[1]) : triplet[1];
                Vector2D<float> p2 = relative ? (_currentPosition + triplet[2]) : triplet[2];
                _instructions.Add(new BezierToInstruction(p0, p1, p2));
                _lastControlPointCS = p1;
            }
            _currentPosition = sequence[^1][2];
            return true;
        }

        private bool ParseSmoothCurveTo(StringSource source, bool relative)
        {
            var sequence = source.ParseCoordinatePairDoubleSequence();
            if (sequence == null)
            {
                return false;
            }
            Vector2D<float> current = _currentPosition;
            foreach (var @double in sequence)
            {
                Vector2D<float> lastCP2 = _lastControlPointCS ?? current;

                Vector2D<float> p0 = 2 * current - lastCP2; // = current - (lastCP2 - current)
                Vector2D<float> p1 = relative ? (_currentPosition + @double[0]) : @double[0];
                Vector2D<float> p2 = relative ? (_currentPosition + @double[1]) : @double[1];
                _instructions.Add(new BezierToInstruction(p0, p1, p2));

                _lastControlPointCS = p1;
                current = p2;
            }
            _currentPosition = current;
            return true;
        }

        private bool ParseQuadraticBezierCurveTo(StringSource source, bool relative)
        {
            var sequence = source.ParseCoordinatePairDoubleSequence();
            if (sequence == null)
            {
                return false;
            }
            foreach (var @double in sequence)
            {
                Vector2D<float> p0 = relative ? (_currentPosition + @double[0]) : @double[0];
                Vector2D<float> p1 = relative ? (_currentPosition + @double[1]) : @double[1];

                // Convert to cubic Bezier
                Vector2D<float> cp0 = _currentPosition + 2.0f / 3.0f * (p0 - _currentPosition);
                Vector2D<float> cp1 = p1 + 2.0f / 3.0f * (p0 - p1);

                _instructions.Add(new BezierToInstruction(cp0, cp1, p1));

                _lastControlPointQT = p0;
            }
            _currentPosition = sequence[^1][1];
            return true;
        }

        private bool ParseSmoothQuadraticBezierCurveTo(StringSource source, bool relative)
        {
            var sequence = source.ParseCoordinatePairSequence();
            if (sequence == null)
            {
                return false;
            }
            Vector2D<float> current = _currentPosition;
            foreach (var coord in sequence)
            {
                Vector2D<float> lastCP = _lastControlPointQT ?? current;

                Vector2D<float> p0 = 2 * current - lastCP;
                Vector2D<float> p1 = relative ? (_currentPosition + coord) : coord;

                // Convert to cubic Bezier
                Vector2D<float> cp0 = _currentPosition + 2.0f / 3.0f * (p0 - _currentPosition);
                Vector2D<float> cp1 = p1 + 2.0f / 3.0f * (p0 - p1);

                _instructions.Add(new BezierToInstruction(cp0, cp1, p1));

                _lastControlPointQT = p0;
                current = p1;
            }
            _currentPosition = sequence[^1];
            return true;
        }

        // totally not stolen from nanosvg
        private void BuildArcFromBeziers(float rx, float ry, float xRot, bool largeFlag, bool sweepFlag, Vector2D<float> pos)
        {
            Vector2D<float> delta = _currentPosition - pos;
            float d = delta.Length;
            if ((d < 1e-6f) || (rx < 1e-6f) || (ry < 1e-6f))
            {
                // Simply draw a line for small arcs
                // TODO: Make this dependent on current scaling
                _instructions.Add(new LineToInstruction(pos));
                return;
            }

            xRot = float.DegreesToRadians(xRot);

            float sinRx = MathF.Sin(xRot);
            float cosRx = MathF.Cos(xRot);

            // Convert to center point parameterisation
            float x1p = cosRx * delta.X / 2.0f + sinRx * delta.Y / 2.0f;
            float y1p = -sinRx * delta.X / 2.0f + cosRx * delta.Y / 2.0f;
            d = Maths.Square(x1p / rx) + Maths.Square(y1p / ry);
            if (d > 1.0f)
            {
                d = MathF.Sqrt(d);
                rx *= d;
                ry *= d;
            }

            // Compute cx', cy'
            float s = 0.0f;
            float sa = Maths.Square(rx * ry) - Maths.Square(rx * y1p) - Maths.Square(ry * x1p);
            float sb = Maths.Square(rx * y1p) + Maths.Square(ry * x1p);
            sa = MathF.Max(sa, 0.0f);
            if (sb > 0.0f)
            {
                s = MathF.Sqrt(sa / sb);
            }
            if (largeFlag == sweepFlag)
            {
                s = -s;
            }
            float cxP = s * rx * y1p / ry;
            float cyP = s * (-ry) * x1p / rx;

            // Compute cx, cy from cx', cy'
            Vector2D<float> c = new(
                (_currentPosition.X + pos.X) / 2.0f + cosRx * cxP - sinRx * cyP,
                (_currentPosition.Y + pos.Y) / 2.0f + sinRx * cxP + cosRx * cyP
            );

            // Calculate theta1 and delta theta
            Vector2D<float> u = new(
                (x1p - cxP) / rx,
                (y1p - cyP) / ry
            );
            Vector2D<float> v = new(
                (-x1p - cxP) / rx,
                (-y1p - cyP) / ry
            );
            float a1 = Maths.VecAngle(Vector2D<float>.UnitX, u); // Initial angle
            float da = Maths.VecAngle(u, v); // Delta angle

            if (!sweepFlag && (da > 0.0f))
            {
                da -= MathF.Tau;
            }
            else if (sweepFlag && (da < 0.0f))
            {
                da += MathF.Tau;
            }

            // Approximate the arc using cubic spline segments
            Matrix3X2<float> xform = new(
                cosRx, sinRx,
                -sinRx, cosRx,
                c.X, c.Y
            );

            // Split arc into max PI/2 segments
            // The loop assumes an iteration per end point (including start and end), this +1.
            int nDivs = (int)(MathF.Abs(da) / (MathF.PI / 2.0f) + 1.0f);
            float hda = (da / nDivs) / 2.0f;
            if ((hda < 1e-3f) & (hda > -1e-3f))
            {
                hda *= 0.5f;
            }
            else
            {
                hda = (1.0f - MathF.Cos(hda)) / MathF.Sin(hda);
            }

            float kappa = MathF.Abs(4.0f / 3.0f * hda);
            if (da < 0.0f)
            {
                kappa = -kappa;
            }

            Vector2D<float> previous = Vector2D<float>.Zero;
            Vector2D<float> previousTan = Vector2D<float>.Zero;
            for (int i = 0; i <= nDivs; i++)
            {
                float a = a1 + da * ((float)i / (float)nDivs);
                float dx = MathF.Cos(a);
                float dy = MathF.Sin(a);
                Vector2D<float> endPos = Vector2D.Transform(new Vector2D<float>(dx * rx, dy * ry), xform);
                Vector2D<float> tan = Vector2D.TransformNormal(new Vector2D<float>(-dy * rx * kappa, dx * ry * kappa), xform);
                if (i > 0)
                {
                    _instructions.Add(new BezierToInstruction(previous + previousTan, endPos - tan, endPos));
                }
                previous = endPos;
                previousTan = tan;
            }
        }

        private bool ParseEllipticalArc(StringSource source, bool relative)
        {
            var sequence = ParseEllipticalArcArgumentSequence(source);
            if (sequence == null)
            {
                return false;
            }
            Vector2D<float> pos = default;
            foreach (var parameters in sequence)
            {
                pos = relative ? (_currentPosition + parameters.Item6) : parameters.Item6;
                BuildArcFromBeziers(parameters.Item1, parameters.Item2, parameters.Item3, parameters.Item4, parameters.Item5, pos);
            }
            _currentPosition = pos;
            return true;
        }

        private void ParseD(StringSource content)
        {
            _currentPosition = Vector2D<float>.Zero;
            _lastControlPointCS = _lastControlPointQT = null;
            _instructions.Clear();
            while (!content.IsDone)
            {
                content.ConsumeWsp();
                char instructionName = content.Current;
                if (!_instructionParsers.TryGetValue(instructionName, out ParseInstruction? instructionParser))
                {
                    return;
                }
                content.Next(); // Instruction name
                content.ConsumeWsp();
                if (!instructionParser.Invoke(content))
                {
                    return;
                }

                // We need the last Bezier's CP for S curve instruction. Delete them an instruction afterwards.
                if ((instructionName != 'C') && (instructionName != 'c') && (instructionName != 'S') && (instructionName != 's'))
                {
                    _lastControlPointCS = null;
                }
                // We need the last Bezier's CP for T curve instruction. Delete them an instruction afterwards.
                if ((instructionName != 'Q') && (instructionName != 'q') && (instructionName != 'T') && (instructionName != 't'))
                {
                    _lastControlPointQT = null;
                }
            }

            _parser.Shapes.Add(new Shape(_instructions, _parser.State));
        }

        public void Parse(XmlElement element)
        {
            ISvgElementParser.ParseAttr(ParseD, element.GetAttribute(SvgAttributes.D));
        }

        private static bool? ParseFlag(StringSource source)
        {
            char current = source.Current;
            source.Next();
            if (current == '0')
            {
                return false;
            }
            else if (current == '1')
            {
                return true;
            }
            return null;
        }

        private static IReadOnlyList<Vector2D<float>[]>? ParseCurveToCoordinateSequence(StringSource source)
        {
            Vector2D<float>[]? triplet = source.ParseCoordinatePairTriplet();
            if (triplet == null)
            {
                return null;
            }
            List<Vector2D<float>[]> sequence = [];
            int idx;
            do
            {
                source.ConsumeCommaWsp();
                idx = source.Index;
                sequence.Add(triplet);
                triplet = source.ParseCoordinatePairTriplet();
            } while (triplet != null);
            source.BackTo(idx);
            return sequence.AsReadOnly();
        }

        private static (float, float, float, bool, bool, Vector2D<float>)? ParseEllipticalArcArgument(StringSource source)
        {
            float? rx = source.ParseCoordinate();
            if (!rx.HasValue)
            {
                return null;
            }
            source.ConsumeCommaWsp();
            float? ry = source.ParseCoordinate();
            if (!ry.HasValue)
            {
                return null;
            }
            source.ConsumeCommaWsp();
            float? xRot = source.ParseCoordinate();
            if (!xRot.HasValue)
            {
                return null;
            }
            if (!source.ConsumeCommaWsp())
            {
                return null;
            }
            bool? largeFlag = ParseFlag(source);
            if (!largeFlag.HasValue)
            {
                return null;
            }
            source.ConsumeCommaWsp();
            bool? sweepFlag = ParseFlag(source);
            if (!sweepFlag.HasValue)
            {
                return null;
            }
            source.ConsumeCommaWsp();
            Vector2D<float>? pos = source.ParseCoordinatePair();
            if (!pos.HasValue)
            {
                return null;
            }
            return (rx.Value, ry.Value, xRot.Value, largeFlag.Value, sweepFlag.Value, pos.Value);
        }

        private static IReadOnlyList<(float, float, float, bool, bool, Vector2D<float>)>? ParseEllipticalArcArgumentSequence(StringSource source)
        {
            (float, float, float, bool, bool, Vector2D<float>)? argument = ParseEllipticalArcArgument(source);
            if (!argument.HasValue)
            {
                return null;
            }
            List<(float, float, float, bool, bool, Vector2D<float>)> sequence = [];
            int idx;
            do
            {
                source.ConsumeCommaWsp();
                idx = source.Index;
                sequence.Add(argument.Value);
                argument = ParseEllipticalArcArgument(source);
            } while (argument.HasValue);
            source.BackTo(idx);
            return sequence.AsReadOnly();
        }

    }
}
