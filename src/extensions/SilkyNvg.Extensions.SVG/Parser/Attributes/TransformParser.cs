using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SilkyNvg.Extensions.Svg.Parser.Attributes
{
    internal class TransformParser
    {

        // Containing element transform
        private float _eX;
        private float _eY;
        private float _eWidth;
        private float _eHeight;

        // View box
        private float _vbX;
        private float _vbY;
        private float _vbWidth;
        private float _vbHeight;

        private void Reset()
        {

        }

        internal Matrix3X2<float> ParseTransform(XmlElement element)
        {
            return default;
        }

    }
}
