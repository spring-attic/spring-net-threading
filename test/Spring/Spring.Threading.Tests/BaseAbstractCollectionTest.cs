using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Spring.Collections.Generic
{
    public abstract class BaseAbstractCollectionTest<T>
    {
        protected abstract AbstractCollection<T> Testee { get; }

        protected abstract T TheTestItem1 { get; }

        protected abstract ICollection<T> BackCollection { get; }

        [Test, ExpectedException(typeof(NotSupportedException))]
        public void AddIsNotSupported()
        {
            Testee.Add(TheTestItem1);
        }

        [Test, ExpectedException(typeof(NotSupportedException))]
        public void ClearIsNotSupported()
        {
            Testee.Clear();
        }

        [Test, ExpectedException(typeof(NotSupportedException))]
        public void RemoveIsNotSupported()
        {
            Testee.Remove(TheTestItem1);
        }

        [Test]
        public void ShouldBeReadOnly()
        {
            Assert.IsTrue(Testee.IsReadOnly);
        }

        [Test]
        public void ShouldNotBeSynchronized()
        {
            Assert.IsFalse(((ICollection)Testee).IsSynchronized);
        }

        [Test]
        public void SyncRootShouldBeNull()
        {
            Assert.IsNull(((ICollection)Testee).SyncRoot);
        }

        [Test]
        public void SunnyDay()
        {
            CollectionTestUtils.VerifyData<T>(BackCollection, Testee);
            Assert.IsFalse(Testee.Contains(TheTestItem1));
        }

    }
}