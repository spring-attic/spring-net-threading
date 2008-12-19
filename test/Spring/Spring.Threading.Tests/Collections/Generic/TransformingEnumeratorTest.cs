using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Spring.Collections.Generic
{
    /// <summary>
    /// Test fixture classes to test <see cref="TransformingEnumerator{TSource, TTarget}"/>
    /// and <see cref="TransformingEnumerator{TTarget}"/>
    /// </summary>
    /// <author>Kenneth Xu</author>
    public class TransformingEnumeratorTest
    {
        #region Test Fixtures for TransformingEnumerator<TSource, TTarget>

        /// <summary>
        /// Clear box test for <see cref="TransformingEnumerator{TSource,TTarget}"/>.
        /// </summary>
        [TestFixture] public class TSourceTTargetTest
        {
            readonly Converter<int, int> _addTow = delegate(int s) { return s + 2; };

            [Test]
            public void TestDispose()
            {
                TestEnumerator source = new TestEnumerator(0);
                new TransformingEnumerator<int, int>(source, _addTow).Dispose();
                Assert.IsTrue(source.IsDisposed);
            }

            [Test]
            public void TestReset()
            {
                TestEnumerator source = new TestEnumerator(5);
                TransformingEnumerator<int, int> te =
                    new TransformingEnumerator<int, int>(source, _addTow);
                te.MoveNext();
                Assert.AreNotEqual(-1, source.Value);
                te.Reset();
                Assert.AreEqual(-1, source.Value);
            }

            [Test]
            public void TestEnumeration()
            {
                int size = 5;
                TestEnumerator source = new TestEnumerator(size);
                TransformingEnumerator<int, int> te =
                    new TransformingEnumerator<int, int>(source, _addTow);
                for (int i = 0; i < size; i++)
                {
                    Assert.IsTrue(te.MoveNext());
                    Assert.AreEqual(i + 2, te.Current);
                }
                Assert.IsFalse(te.MoveNext());
            }

            [Test]
            public void TestEnumerationNonGenerics()
            {
                int size = 5;
                TestEnumerator source = new TestEnumerator(size);
                TransformingEnumerator<int, int> te =
                    new TransformingEnumerator<int, int>(source, _addTow);

                IEnumerator ie = te;

                for (int i = 0; i < size; i++)
                {
                    Assert.IsTrue(ie.MoveNext());
                    Assert.AreEqual(i + 2, ie.Current);
                }
                Assert.IsFalse(ie.MoveNext());
            }
        }

        /// <summary>
        /// Functional test for <see cref="TransformingEnumerator{TSource,TTarget}"/>
        /// from value type to value type.
        /// </summary>
        [TestFixture] public class Int2IntFunctionTest : FunctionTest<int, int>
        {
            static Int2IntFunctionTest()
            {
                Converter = delegate(int i) { return i + 2; };
            }
        }

        /// <summary>
        /// Functional test for <see cref="TransformingEnumerator{TSource,TTarget}"/>
        /// from reference type to value type.
        /// </summary>
        [TestFixture] public class String2StringFunctionTest : FunctionTest<string, string>
        {
            static String2StringFunctionTest()
            {
                Converter = delegate(string s) { return "Hello " + s; };
            }
        }

        /// <summary>
        /// Functional test for <see cref="TransformingEnumerator{TSource,TTarget}"/>
        /// from value type to reference type.
        /// </summary>
        [TestFixture] public class Int2StringFunctionTest : FunctionTest<int, string>
        {
            static Int2StringFunctionTest()
            {
                Converter = delegate(int i) { return i.ToString(); };
            }
        }

        /// <summary>
        /// Functional test for <see cref="TransformingEnumerator{TSource,TTarget}"/>
        /// from reference type to reference type.
        /// </summary>
        [TestFixture] public class String2IntFunctionTest : FunctionTest<string, int>
        {
            static String2IntFunctionTest()
            {
                Converter = delegate(string s) { return int.Parse(s); };
            }
        }

        /// <summary>
        /// Base class of all functional test for <see cref="TransformingEnumerator{TSource,TTarget}"/>.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        public class FunctionTest<TSource, TTarget> : EnumeratorFunctionTestBase<TTarget>
        {
            public static Converter<TSource, TTarget> Converter;

            private IEnumerator<TTarget> _testee;
            private IList<TSource> _source;
            private IList<TTarget> _expected;


            protected override IEnumerator<TTarget> Testee
            {
                get { return _testee; }
            }
            
            [TestFixtureSetUp] public void FixtureSetup()
            {
                _source = CollectionTestUtils.MakeTestList<TSource>(10);
                _expected = new List<TTarget>(_source.Count);
                foreach (TSource i in _source)
                {
                    _expected.Add(Converter(i));
                }
            }

            [SetUp]
            public void SetUp()
            {
                _testee = new TransformingEnumerator<TSource, TTarget>(_source.GetEnumerator(), Converter);
            }

            [Test]
            public void ChokesOnConstructingWithNullSource()
            {
                TestHelper.AssertException<ArgumentNullException>(
                    delegate { new TransformingEnumerator<TSource, TTarget>(null, Converter); },
                    MessageMatch.Contains, "source");
            }

            [Test]
            public void ChokesOnConstructingWithNullTransformer()
            {
                TestHelper.AssertException<ArgumentNullException>(
                    delegate { new TransformingEnumerator<TSource, TTarget>(_source.GetEnumerator(), null); },
                    MessageMatch.Contains, "transformer");
            }

            protected override IEnumerator<TTarget> GetExpectedEnumerator()
            {
                return _expected.GetEnumerator();
            }

        }

	    #endregion
        
        #region TransformingEnumerator<TTarget>

        /// <summary>
        /// Clear box test for <see cref="TransformingEnumerator{TTarget}"/>.
        /// </summary>
        [TestFixture] public class TTargetTest
        {
            readonly Converter<object, string> _sayHello = delegate(object o) { return "Hello " + o; };

            [Test]
            public void TestDispose()
            {
                TestEnumerator source = new TestEnumerator(0);
                new TransformingEnumerator<string>(source, _sayHello).Dispose();
                Assert.IsTrue(source.IsDisposed);

                ArrayList list = new ArrayList();
                new TransformingEnumerator<string>(list.GetEnumerator(), _sayHello).Dispose();
            }

            [Test]
            public void TestReset()
            {
                TestEnumerator source = new TestEnumerator(5);
                TransformingEnumerator<string> te =
                    new TransformingEnumerator<string>(source, _sayHello);
                te.MoveNext();
                Assert.AreNotEqual(-1, source.Value);
                te.Reset();
                Assert.AreEqual(-1, source.Value);
            }

            [Test]
            public void TestEnumeration()
            {
                int size = 5;
                TestEnumerator source = new TestEnumerator(size);
                TransformingEnumerator<string> te =
                    new TransformingEnumerator<string>(source, _sayHello);
                for (int i = 0; i < size; i++)
                {
                    Assert.IsTrue(te.MoveNext());
                    Assert.AreEqual("Hello " + i, te.Current);
                }
                Assert.IsFalse(te.MoveNext());
            }

            [Test]
            public void TestEnumerationNonGenerics()
            {
                int size = 5;
                TestEnumerator source = new TestEnumerator(size);
                TransformingEnumerator<string> te =
                    new TransformingEnumerator<string>(source, _sayHello);

                IEnumerator ie = te;

                for (int i = 0; i < size; i++)
                {
                    Assert.IsTrue(ie.MoveNext());
                    Assert.AreEqual("Hello " + i, ie.Current);
                }
                Assert.IsFalse(ie.MoveNext());
            }
        }

        /// <summary>
        /// Functional test for <see cref="TransformingEnumerator{TTarget}"/>
        /// from non-generic enumerator to value type.
        /// </summary>
		[TestFixture] public class Object2IntFunction : FunctionTest<int>
        {
            static Object2IntFunction()
            {
                Converter = delegate(object o) { return o.GetHashCode(); };
            }
        }

        /// <summary>
        /// Functional test for <see cref="TransformingEnumerator{TTarget}"/>
        /// from non-generic enumerator to reference type.
        /// </summary>
        [TestFixture] public class Object2StringFunction: FunctionTest<string>
        {
            static Object2StringFunction()
            {
                Converter = delegate(object o) { return "ToString: " + o; };
            }
        }

        /// <summary>
        /// Base class of all functional tests for <see cref="TransformingEnumerator{TTarget}"/>.
        /// </summary>
        /// <typeparam name="T">Type of the element to iterate.</typeparam>
        public class FunctionTest<T> : EnumeratorFunctionTestBase<T>
        {
            public static Converter<object, T> Converter;

            private IEnumerator<T> _testee;
            private IList _source;
            private IList<T> _expected;


            protected override IEnumerator<T> Testee
            {
                get { return _testee; }
            }

            [TestFixtureSetUp]
            public void FixtureSetup()
            {
                _source = CollectionTestUtils.MakeTestArray<object>(10);
                _expected = new List<T>(_source.Count);
                foreach (object i in _source)
                {
                    _expected.Add(Converter(i));
                }
            }

            [SetUp]
            public void SetUp()
            {
                _testee = new TransformingEnumerator<T>(_source.GetEnumerator(), Converter);
            }

            [Test]
            public void ChokesOnConstructingWithNullSource()
            {
                TestHelper.AssertException<ArgumentNullException>(
                    delegate { new TransformingEnumerator<T>(null, Converter); },
                    MessageMatch.Contains, "source");
            }

            [Test]
            public void ChokesConstructingWithNullTransformer()
            {
                TestHelper.AssertException<ArgumentNullException>(
                    delegate { new TransformingEnumerator<T>(_source.GetEnumerator(), null); },
                    MessageMatch.Contains, "transformer");
            }
            
            protected override IEnumerator<T> GetExpectedEnumerator()
            {
                return _expected.GetEnumerator();
            }

        }

	    #endregion    
    }
}
