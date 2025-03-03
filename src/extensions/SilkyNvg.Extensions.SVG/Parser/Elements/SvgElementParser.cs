using AngleSharp.Css;
using AngleSharp.Css.Values;
using AngleSharp.Text;
using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Extensions.Svg.Parser.Constants;
using SilkyNvg.Extensions.Svg.Transforms;
using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser.Elements
{
    internal class SvgElementParser(SvgParser parser) : ISvgElementParser
    {

        private void ParseX(StringSource content)
        {
            if (!Length.TryParse(content.Content, out Length x))
            {
                return;
            }
            float tx = (float)x.ToPixel(parser.State, RenderMode.Horizontal);
            parser.State.Transform = Maths.Multiply(Matrix3X2.CreateTranslation(new Vector2D<float>(tx, 0f)), parser.State.Transform);
        }

        private void ParseY(StringSource content)
        {
            if (!Length.TryParse(content.Content, out Length y))
            {
                return;
            }
            float ty = (float)y.ToPixel(parser.State, RenderMode.Vertical);
            parser.State.Transform = Maths.Multiply(Matrix3X2.CreateTranslation(new Vector2D<float>(0f, ty)), parser.State.Transform);
        }

        private void ParseWidth(StringSource content)
        {
            if (!Length.TryParse(content.Content, out Length width))
            {
                return;
            }
            parser.Width = (float)width.ToPixel(parser.State, RenderMode.Horizontal);
        }

        private void ParseHeight(StringSource content)
        {
            if (!Length.TryParse(content.Content, out Length height))
            {
                return;
            }
            parser.Height = (float)height.ToPixel(parser.State, RenderMode.Vertical);
        }

        private void ParsePreserveAspectRatio(StringSource content)
        {
            string[] tokens = content.Content.SplitSpaces();

            if (tokens.Length < 1)
            {
                return;
            }

            // align
            Align align = Align.None;
            string alignS = tokens[0];
            if (alignS.Contains("xMin"))
            {
                align |= Align.XMin;
            }
            else if (alignS.Contains("xMid"))
            {
                align |= Align.XMid;
            }
            else if (alignS.Contains("xMax"))
            {
                align |= Align.XMax;
            }

            if (alignS.Contains("yMin"))
            {
                align |= Align.YMin;
            }
            else if (alignS.Contains("yMid"))
            {
                align |= Align.YMid;
            }
            else if (alignS.Contains("yMax"))
            {
                align |= Align.YMax;
            }

            // meet or slice
            MeetOrSlice meetOrSlice = MeetOrSlice.Meet;
            if (tokens.Length == 2)
            {
                if (tokens[1] == "slice")
                {
                    meetOrSlice = MeetOrSlice.Slice;
                }
            }

            parser.State.Align = align;
            parser.State.MeetOrSlice = meetOrSlice;
        }

        private void ParseViewBox(StringSource content)
        {
            string[] dimensions = content.Content.SplitSpaces();
            if (dimensions.Length != 4)
            {
                return;
            }

            if (!float.TryParse(dimensions[0], out float vbX))
            {
                return;
            }
            if (!float.TryParse(dimensions[1], out float vbY))
            {
                return;
            }
            if (!float.TryParse(dimensions[2], out float vbWidth))
            {
                return;
            }
            if (!float.TryParse(dimensions[3], out float vbHeight))
            {
                return;
            }

            float eX = parser.State.Viewport.X;
            float eY = parser.State.Viewport.Y;
            float eWidth = parser.State.Viewport.Width == 0 ? parser.Width : parser.State.Viewport.Width;
            float eHeight = parser.State.Viewport.Height == 0 ? parser.Height : parser.State.Viewport.Height;

            Align align = parser.State.Align;
            MeetOrSlice meetOrSlice = parser.State.MeetOrSlice;

            float scaleX = eWidth / vbWidth;
            float scaleY = eHeight / vbHeight;

            if (align != Align.None && meetOrSlice == MeetOrSlice.Meet)
            {
                if (scaleX > scaleY)
                {
                    scaleX = scaleY;
                }
                else if (scaleY > scaleX)
                {
                    scaleY = scaleX;
                }
            }
            else if (align != Align.None && meetOrSlice == MeetOrSlice.Slice)
            {
                if (scaleX > scaleY)
                {
                    scaleY = scaleX;
                }
                else if (scaleY > scaleX)
                {
                    scaleX = scaleY;
                }
            }

            float translateX = eX - (vbX * scaleX);
            float translateY = eY - (vbY * scaleY);

            if (align.HasFlag(Align.XMid))
            {
                translateX += (eWidth - vbWidth * scaleX) / 2.0f;
            }
            else if (align.HasFlag(Align.XMax))
            {
                translateX += eWidth - vbWidth * scaleX;
            }
            if (align.HasFlag(Align.YMid))
            {
                translateY += (eHeight - vbHeight * scaleY) / 2.0f;
            }
            else if (align.HasFlag(Align.YMax))
            {
                translateY += eHeight - vbHeight * scaleY;
            }

            parser.State.Transform = Maths.Multiply(Maths.Multiply(Matrix3X2.CreateTranslation(translateX, translateY), Matrix3X2.CreateScale(scaleX, scaleY)), parser.State.Transform);
        }

        public void Parse(XmlElement element)
        {
            // Parse svg specific attributes
            ISvgElementParser.ParseAttr(ParseWidth, element.GetAttribute(SvgAttributes.Width));
            ISvgElementParser.ParseAttr(ParseHeight, element.GetAttribute(SvgAttributes.Height));
            ISvgElementParser.ParseAttr(ParseX, element.GetAttribute(SvgAttributes.X));
            ISvgElementParser.ParseAttr(ParseY, element.GetAttribute(SvgAttributes.Y));
            ISvgElementParser.ParseAttr(ParsePreserveAspectRatio, element.GetAttribute(SvgAttributes.PreserveAspectRatio));
            ISvgElementParser.ParseAttr(ParseViewBox, element.GetAttribute(SvgAttributes.ViewBox));
        }

    }
}
