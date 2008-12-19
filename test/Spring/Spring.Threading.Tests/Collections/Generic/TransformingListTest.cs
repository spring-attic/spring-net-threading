using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;

namespace Spring.Collections.Generic
{
    /// <summary>
    /// Test cases for <see cref="AbstractTransformingList{TFrom,TTo}"/>
    /// class and <see cref="TransformingList{TFrom,TTo}"/> class.
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture]
    public class TrasnformingListTest
    {
        static readonly Converter<int, string> _intToString = delegate(int i) { return i.ToString(); };
        static readonly Converter<string, int> _stringToInt = delegate(string s) { return int.Parse(s); };
        private const int _size = 5;

        private List<int> _source;
        private IList<string> _nonReversable;
        private IList<string> _reversable;

        private MockRepository _mocks;

        [SetUp]
        public void SetUp()
        {
            _mocks = new MockRepository();

            _source = new List<int>(CollectionTestUtils.MakeTestList<int>(_size));

            _nonReversable = new TransformingList<int, string>(_source, _intToString);

            _reversable = new TransformingList<int, string>(_source, _intToString, _stringToInt);
        }

        [Test]
        public void ChokesOnConstructingWithNullSource()
        {
            TestHelper.AssertException<ArgumentNullException>(
                delegate { new TransformingList<int, string>(null, _intToString); },
                MessageMatch.Contains, "source");
        }

        [Test]
        public void ChokesOnConstructingWithNullTransformer()
        {
            TestHelper.AssertException<ArgumentNullException>(
                delegate
                {
                    new TransformingList<int, string>(
             CollectionTestUtils.MakeTestList<int>(10), null);
                },
                MessageMatch.Contains, "transformer");
        }

        [Test] public void IndexerGet()
        {
            for(int i = 0; i<_size; i++)
            {
                string expected = _intToString(_source[i]);
                Assert.AreEqual(expected, _nonReversable[i]);
                Assert.AreEqual(expected, _reversable[i]);
            }
        }

        [Test] public void IndexerSet()
        {
            int toBeSet = _size/2;
            TestHelper.AssertException<NotSupportedException>(
                delegate { _nonReversable[toBeSet] ="Y"; }
            );

            _reversable[toBeSet] = _intToString(toBeSet);
            Assert.AreEqual(toBeSet, _source[toBeSet]);
        }
        [Test]
        public void Add()
        {
            TestHelper.AssertException<NotSupportedException>(
                delegate { _nonReversable.Add("Y"); }
            );

            _reversable.Add(_size.ToString());
            Assert.AreEqual(_size, _source[_size]);
        }

        [Test]
        public void Insert()
        {
            TestHelper.AssertException<NotSupportedException>(
                delegate { _nonReversable.Insert(0, "Y"); }
            );

            _reversable.Insert(0, _size.ToString());
            Assert.AreEqual(_size, _source[0]);
        }

        [Test]
        public void RemoveNonReversable()
        {
            int toBeRemoved = _size / 2;

            Assert.IsTrue(_nonReversable.Remove(toBeRemoved.ToString()));
            Assert.IsFalse(_source.Contains(toBeRemoved));

            Assert.IsFalse(_nonReversable.Remove(_size.ToString()));
        }

        [Test]
        public void RemoveReversable()
        {
            int toBeRemoved = _size / 2;

            Assert.IsTrue(_reversable.Remove(toBeRemoved.ToString()));
            Assert.IsFalse(_source.Contains(toBeRemoved));

            Assert.IsFalse(_reversable.Remove(_size.ToString()));
        }

        [Test] public void RemoveAt()
        {
            int toBeRemoved = _size / 2;

            _nonReversable.RemoveAt(toBeRemoved);
            Assert.IsFalse(_source.Contains(toBeRemoved));
        }

        [Test]
        public void Clear()
        {
            _nonReversable.Clear();
            Assert.IsEmpty(_source);

            _source.AddRange(CollectionTestUtils.MakeTestList<int>(_size));
            Assert.AreEqual(_size, _source.Count);

            _reversable.Clear();
            Assert.IsEmpty(_source);
        }

        [Test]
        public void Contains()
        {
            Assert.IsTrue(_nonReversable.Contains((_size / 2).ToString()));
            Assert.IsTrue(_reversable.Contains((_size / 2).ToString()));

            Assert.IsFalse(_nonReversable.Contains(_size.ToString()));
            Assert.IsFalse(_reversable.Contains(_size.ToString()));
        }

        [Test]
        public void Count()
        {
            Assert.AreEqual(_size, _nonReversable.Count);
            Assert.AreEqual(_size, _reversable.Count);
        }

        [Test]
        public void IsReadOnly()
        {
            Assert.IsFalse(_reversable.IsReadOnly);

            IList<int> readOnlyList = _mocks.Stub<IList<int>>();
            SetupResult.For(readOnlyList.IsReadOnly).Return(true);
            _mocks.ReplayAll();

            Assert.IsTrue(new TransformingList<int, string>(readOnlyList, _intToString).IsReadOnly);
        }

        [Test]
        public void GetEmuerator()
        {
            TestHelper.AssertEnumeratorEquals(
                CollectionTestUtils.MakeTestList<string>(_size).GetEnumerator(),
                _nonReversable.GetEnumerator());
        }

        [Test]
        public void IsSyncronized()
        {
            NonGenericTestFixture fixture = SetupNonGeneric();

            Expect.Call(fixture.OmniUnsync.IsSynchronized).Return(false);
            Expect.Call(fixture.OmniSync.IsSynchronized).Return(true);
            _mocks.ReplayAll();

            Assert.IsFalse((fixture.FromGeneric).IsSynchronized);
            Assert.IsTrue((fixture.FromOmniSync).IsSynchronized);
            Assert.IsFalse((fixture.FromOmniUnsync).IsSynchronized);

            _mocks.VerifyAll();
        }

        [Test]
        public void SyncRoot()
        {
            NonGenericTestFixture fixture = SetupNonGeneric();
            object syncRoot = new object();

            Expect.Call(fixture.OmniUnsync.SyncRoot).Return(null);
            Expect.Call(fixture.OmniSync.SyncRoot).Return(syncRoot);
            _mocks.ReplayAll();

            Assert.IsNull((fixture.FromGeneric).SyncRoot);
            Assert.AreEqual(syncRoot, (fixture.FromOmniSync).SyncRoot);
            Assert.IsNull((fixture.FromOmniUnsync).SyncRoot);

            _mocks.VerifyAll();
        }

        [Test]
        public void TryReverseInAbstractClass()
        {
            IList<string> c = _mocks.PartialMock<AbstractTransformingList<int, string>>(_source);
            _mocks.ReplayAll();
            TestHelper.AssertException<NotSupportedException>(
                delegate { c.Add(_size.ToString()); }
            );
        }

        struct NonGenericTestFixture
        {
            public IList<int> Generic;
            public IOmniList<int> OmniSync;
            public IOmniList<int> OmniUnsync;

            public IList FromGeneric;

            public IList FromOmniSync;

            public IList FromOmniUnsync;
        }

        private NonGenericTestFixture SetupNonGeneric()
        {
            NonGenericTestFixture fixture = new NonGenericTestFixture();
            fixture.Generic = _mocks.CreateMock<IList<int>>();
            fixture.OmniSync = _mocks.CreateMock<IOmniList<int>>();
            fixture.OmniUnsync = _mocks.CreateMock<IOmniList<int>>();

            fixture.FromGeneric = new TransformingList<int, string>(fixture.Generic, _intToString);

            fixture.FromOmniSync = new TransformingList<int, string>(fixture.OmniSync, _intToString);

            fixture.FromOmniUnsync = new TransformingList<int, string>(fixture.OmniUnsync, _intToString);

            return fixture;
        }
    }
}
