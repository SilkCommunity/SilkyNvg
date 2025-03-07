using SilkyNvg.Common.Geometry;

namespace SilkyNvg.Tests.Geometry
{
    public class RectUTests
    {

        private uint _xInside, _yInside;
        private uint _xOutside, _yOutside;
        private RectU _large;
        private RectU _outside, _intersecting;
        private RectU _intersection;

        [SetUp]
        public void Setup()
        {
            _large = new RectU(5, 5, 100, 100);

            _xInside = 50; _yInside = 50;
            _xOutside = 0; _yOutside = 0;
            _outside = new RectU(0, 0, 10, 5);
            _intersecting = new RectU(80, 80, 40, 40);
            _intersection = new RectU(80, 80, 25, 25);
        }

        [Test]
        public void ContainsPointTest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_large.Contains(_xInside, _yInside), Is.True);
                Assert.That(_large.Contains(_xOutside, _yOutside), Is.False);
            });
        }

        [Test]
        public void ComparisonTest()
        {
            var other = _large;
            Assert.That(_large, Is.EqualTo(other));
        }

        [Test]
        public void ContainsRectTest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_large.Contains(_outside), Is.False);
                Assert.That(_large.Contains(_intersecting), Is.False);
                Assert.That(_large.Contains(_intersection), Is.True);
            });
        }

        [Test]
        public void IntersectsRectTest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_large.IsIntersecting(_outside), Is.False);
                Assert.That(_large.IsIntersecting(_intersecting), Is.True);
                Assert.That(_large.Intersect(_intersecting), Is.EqualTo(_intersection));
            });
        }

    }
}