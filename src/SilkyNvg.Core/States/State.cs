using Silk.NET.Maths;

namespace SilkyNvg.Core.States
{
    public class State
    {

        private CompositeOperationState _compositeOperationState;
        private bool _shapeAntiAlias;
        private Paint _fill;
        private Paint _stroke;
        private float _strokeWidth;
        private float _miterLimit;
        private LineCap _lineJoin;
        private LineCap _lineCap;
        private float _alpha;
        private Matrix3X2<float> _xform;
        private Scissor _scissor;

        public CompositeOperationState CompositeOperation
        {
            get => _compositeOperationState;
            set => _compositeOperationState = value;
        }

        public bool ShapeAntiAlias
        {
            get => _shapeAntiAlias;
            set => _shapeAntiAlias = value;
        }

        public Paint Fill
        {
            get => _fill;
            set => _fill = value;
        }

        public Paint Stroke
        {
            get => _stroke;
            set => _stroke = value;
        }

        public float StrokeWidth
        {
            get => _strokeWidth;
            set => _strokeWidth = value;
        }

        public float MiterLimit
        {
            get => _miterLimit;
            set => _miterLimit = value;
        }

        public LineCap LineCap
        {
            get => _lineCap;
            set => _lineCap = value;
        }

        public LineCap LineJoin
        {
            get => _lineJoin;
            set => _lineJoin = value;
        }

        public float Alpha
        {
            get => _alpha;
            set => _alpha = value;
        }

        public Matrix3X2<float> Transform
        {
            get => _xform;
            set => _xform = value;
        }

        public Scissor Scissor
        {
            get => _scissor;
        }

        public State()
        {
            Reset();
        }

        public void Reset()
        {
            _fill = new Paint(new Colour(255, 255, 255, 255));
            _stroke = new Paint(new Colour(0, 0, 0, 255));
            _compositeOperationState = new CompositeOperationState(SilkyNvg.CompositeOperation.SourceOver);
            _shapeAntiAlias = true;
            _strokeWidth = 1.0f;
            _miterLimit = 1.0f;
            _lineCap = LineCap.Butt;
            _lineJoin = LineCap.Miter;
            _alpha = 1.0f;
            _xform = Matrix3X2<float>.Identity;

            _scissor = new Scissor(new Vector2D<float>(-1.0f));
        }

    }
}
