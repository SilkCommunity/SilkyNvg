using SilkyNvg.Common.Geometry;
using System.Numerics;

namespace SilkyNvg.Tests.Geometry
{
    public class RectFTests
    {

        private Vector2 _pointOutside, _pointInside;
        private RectF _large;
        private RectF _outside, _intersecting;
        private RectF _intersection;

        [SetUp]
        public void Setup()
        {
            _large = new RectF(5f, 5f, 100f, 100f);

            _pointInside = new Vector2(50f, 50f);
            _pointOutside = new Vector2(0f, 0f);
            _outside = new RectF(0f, 0f, 10f, 5f);
            _intersecting = new RectF(80f, 80f, 40f, 40f);
            _intersection = new RectF(80f, 80f, 25f, 25f);
        }

        [Test]
        public void ContainsPointTest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_large.Contains(_pointInside), Is.True);
                Assert.That(_large.Contains(_pointOutside), Is.False);
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