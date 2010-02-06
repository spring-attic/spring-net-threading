using System;
using NUnit.CommonFixtures.Collections;
using NUnit.Framework;
using Spring.Collections.Generic;
using NUnit.CommonFixtures;

namespace Spring.TestFixtures.Collections
{
    public abstract class AbstractCollectionContract<T> : CollectionContractBase<T>
    {
        protected AbstractCollectionContract()
        {
        }

        protected AbstractCollectionContract(CollectionContractOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Return a new empty <see cref="AbstractCollection{TE}"/>.
        /// </summary>
        /// <returns></returns>
        protected abstract AbstractCollection<TE> NewCollection<TE>();

        /// <summary>
        /// Return a new empty <see cref="AbstractCollection{T}"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual AbstractCollection<T> NewCollection()
        {
            return NewCollection<T>();
        }

        protected virtual AbstractCollection<T> NewCollectionFilledWithSample()
        {
            var result = NewCollection();
            foreach (var sample in Samples)
            {
                result.Add(sample);
            }
            return result;
        }

        [Test] public void AddRangeAddAllElementsToCollection()
        {
            var sut = NewCollection();
            var samples = NewSamples();
            bool result = false;
            PassIfBounded(() => result = sut.AddRange(samples));
            Assert.That(result, Is.True);
            CollectionAssert.AreEquivalent(samples, sut);
        }

        [Test] public void AddRangeWelcomesNullElements()
        {
            Options.SkipWhen(CollectionContractOptions.NoNull);
            var sut = NewCollection();
            sut.AddRange(new T[1]);
        }

        [Test]
        public void AddRangeReturnsFalseWhenParameterIsEmptyCollection()
        {
            var sut = NewCollection();
            bool result = true; 
            PassIfBounded(()=>result = sut.AddRange(new T[0]));
            Assert.That(result, Is.False);
        }

        [Test]
        public void AddRangeChokesOnNullParameter()
        {
            var sut = NewCollection();
            var e = Assert.Throws<ArgumentNullException>(() => sut.AddRange(null));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test]
        public void AddRangeChokesWhenAddItself()
        {
            var sut = NewCollection();
            var e = Assert.Throws<ArgumentException>(() => sut.AddRange(sut));
            Assert.That(e.ParamName, Is.EqualTo("collection"));
        }

        [Test] public void ToArrayWritesAllElementsToNewArray() 
        {
            var sut = NewCollectionFilledWithSample();
            var array = sut.ToArray();
            CollectionAssert.AreEquivalent(sut, array);
        }

        [Test] public void ToArrayWritesAllElementsToExistingArray() 
        {
            var sut = NewCollectionFilledWithSample();
            var array = new T[SampleSize];
            array = sut.ToArray(array);
            CollectionAssert.AreEquivalent(sut, array);
        }

        [Test] public void ToArrayChokesOnNullArray() 
        {
            var sut = NewCollectionFilledWithSample();
            var e = Assert.Throws<ArgumentNullException>(()=>sut.ToArray(null));
            Assert.That(e.ParamName, Is.EqualTo("targetArray"));
        }

        [Test] public void ToArrayChokesOnIncompatibleArray()
        {
            TestHelper.SkipOnValueType(typeof(T));
            var sut = NewCollection<object>();
            sut.Add(new object());
            ContractAssert.Catch<ArrayTypeMismatchException>(() => sut.ToArray(new string[10]));
        }

        [Test] public void ToArrayWorksFineWithArrayOfSubType()
        {
            TestHelper.SkipOnValueType(typeof(T));
            var sut = NewCollection<object>();
            sut.Add(TestData<string>.One);
            var array = new string[10];
            sut.ToArray(array);
            Assert.That(array[0], Is.EqualTo(TestData<string>.One));
        }

        [Test] public void ToArrayExpendsShorterArray()
        {
            var sut = NewCollectionFilledWithSample();
            var a = new T[0];
            var a2 = sut.ToArray(a);
            CollectionAssert.AreEquivalent(sut, a2);
        }

        [Test] public void ToArrayExpendsShorterArrayWithSameType()
        {
            var sut = NewCollection<object>();
            sut.Add(TestData<string>.One);
            var a = new string[0];
            var a2 = sut.ToArray(a);
            CollectionAssert.AreEquivalent(sut, a2);
            Assert.That(a2, Is.InstanceOf<string[]>());
        }
    }
}
