using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Collections.Generic;
using NUnit.CommonFixtures;

namespace Spring.TestFixtures.Collections
{
    public abstract class AbstractCollectionTestFixture<T>
    {
        private int _sampleSize = 5;

        protected int SampleSize
        {
            get { return _sampleSize; }
            set { _sampleSize = value; }
        }

        protected T[] Samples { get; set; }
        private AbstractCollection<T> _sut;

        protected virtual T[] NewSamples()
        {
            return TestData<T>.MakeTestArray(SampleSize);
        }

        /// <summary>
        /// Return a new empty <see cref="AbstractCollection{T}"/>.
        /// </summary>
        /// <returns></returns>
        protected abstract AbstractCollection<T> NewCollection();

        [SetUp]
        public virtual void SetUpSamples()
        {
            Samples = NewSamples();
        }

        [Test]
        public void AddRangeAddAllElementsInTraversalOrder()
        {
            T[] testData = TestData<T>.MakeTestArray(_sampleSize);
            _sut.Stub(x => x.Add(Arg<T>.Is.Anything));
            Assert.That(_sut.AddRange(testData), Is.True);
            Activities last = null;
            foreach (T data in testData)
            {
                T element = data;
                var call = _sut.ActivityOf(x => x.Add(element));
                if (last != null) Mockery.Assert(last < call);
                last = call;
            }
        }

        [Test]
        public void AddRangeWelcomesNullElements()
        {
            TestHelper.SkipOnValueType(typeof(T));
            Assert.That(() => _sut.AddRange(new T[1]), Throws.Nothing);
        }

        [Test]
        public void AddRangeReturnsFalseWhenParameterIsEmptyCollection()
        {
            T[] testData = new T[0];
            Assert.That(_sut.AddRange(testData), Is.False);
        }

        [Test]
        public void AddRangeChokesOnNullParameter()
        {
            var e = Assert.Throws<ArgumentNullException>(() => _sut.AddRange(null));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test]
        public void AddRangeChokesWhenAddItself()
        {
            var e = Assert.Throws<ArgumentException>(() => _sut.AddRange(_sut));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test]
        public void AddRangeChokesWhenNotEnoughRoom()
        {
            _sut.Stub(x => x.Add(Arg<T>.Is.Anything)).Repeat.Once();
            _sut.Stub(x => x.Add(Arg<T>.Is.Anything)).Throw(new InvalidOperationException()).Repeat.Once();

            Assert.Throws<InvalidOperationException>(
                () => _sut.AddRange(TestData<T>.MakeTestArray(2)));
        }
    }
}
