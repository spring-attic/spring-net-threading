using System;
using NUnit.CommonFixtures;
using NUnit.CommonFixtures.Threading;
using NUnit.Framework;
using Spring.Threading.Execution;

namespace Spring
{
    public abstract class ThreadingTestFixture
    {
        public const int DEFAULT_COLLECTION_SIZE = 20;

        protected TestThreadManager ThreadManager { get; private set; }

        [SetUp]
        public virtual void SetUpThreadManager()
        {
            ThreadManager = new TestThreadManager();
        }

        [TearDown]
        public virtual void TearDownThreadManager()
        {
            ThreadManager.TearDown(true);
        }

        public virtual void JoinPool(IExecutorService exec)
        {
            JoinPool(exec, Delays.Long);
        }

        public virtual void JoinPool(IExecutorService exec, TimeSpan waitTime)
        {
            OnJoinPool(exec, false);
            exec.Shutdown();
            Assert.IsTrue(exec.AwaitTermination(waitTime));
        }


        public virtual void InterruptAndJoinPool(IExecutorService exec)
        {
            InterruptAndJoinPool(exec, Delays.Long);
        }

        public virtual void InterruptAndJoinPool(IExecutorService exec, TimeSpan waitTime)
        {
            OnJoinPool(exec, true);
            exec.ShutdownNow();
            Assert.IsTrue(exec.AwaitTermination(waitTime));
        }
        
        protected virtual void OnJoinPool(IExecutorService exec, bool isInterrupted)
        {
        }
    }

    public abstract class ThreadingTestFixture<T> : ThreadingTestFixture
    {
        protected static T eight = TestData<T>.MakeData(8);
        protected static T five = TestData<T>.MakeData(5);
        protected static T four = TestData<T>.MakeData(4);
        protected static T m1 = TestData<T>.MakeData(-1);
        protected static T m10 = TestData<T>.MakeData(-10);
        protected static T m2 = TestData<T>.MakeData(-2);
        protected static T m3 = TestData<T>.MakeData(-3);
        protected static T m4 = TestData<T>.MakeData(-4);
        protected static T m5 = TestData<T>.MakeData(-5);
        protected static T nine = TestData<T>.MakeData(9);
        protected static T one = TestData<T>.MakeData(1);
        protected static T seven = TestData<T>.MakeData(7);

        protected static T six = TestData<T>.MakeData(6);
        protected static T three = TestData<T>.MakeData(3);
        protected static T two = TestData<T>.MakeData(2);
        protected static T zero = TestData<T>.MakeData(0);
    }
}