using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Spring.Collections;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Execution;
using Spring.Threading.Future;

namespace Spring.Threading.Collections
{
    [TestFixture]
    public class DelayQueueTests : BaseThreadingTestCase
    {
        private class AnonymousClassRunnable
        {
            public AnonymousClassRunnable( DelayQueue<PDelay> q )
            {
                this.q = q;
            }

            private DelayQueue<PDelay> q;

            public void Run()
            {
                int Added = 0;
                try
                {
                    q.Put( new PDelay( new TimeSpan( 0 ) ) );
                    ++Added;
                    q.Put( new PDelay( new TimeSpan( 0 ) ) );
                    ++Added;
                    q.Put( new PDelay( new TimeSpan( 0 ) ) );
                    ++Added;
                    q.Put( new PDelay( new TimeSpan( 0 ) ) );
                    ++Added;
                    Assert.IsTrue( Added == 4 );
                }
                finally {}
            }
        }

        private class AnonymousClassRunnable1
        {
            public AnonymousClassRunnable1( DelayQueue<PDelay> q )
            {
                this.q = q;
            }

            private DelayQueue<PDelay> q;

            public void Run()
            {
                try
                {
                    q.Put( new PDelay( new TimeSpan( 0 ) ) );
                    q.Put( new PDelay( new TimeSpan( 0 ) ) );
                    Assert.IsTrue( q.Offer( new PDelay( new TimeSpan( 0 ) ), SHORT_DELAY_MS ) );
                    Assert.IsTrue( q.Offer( new PDelay( new TimeSpan( 0 ) ), LONG_DELAY_MS ) );
                }
                finally {}
            }
        }

        private class AnonymousClassRunnable2
        {
            public AnonymousClassRunnable2( DelayQueue<PDelay> q )
            {
                this.q = q;
            }

            private DelayQueue<PDelay> q;

            public void Run()
            {
                try
                {
                    q.Take();
                    Assert.Fail( "Should throw an exception" );
                }
                catch ( ThreadInterruptedException ) {}
            }
        }

        private class AnonymousClassRunnable3
        {
            private DelayQueueTests _enclosingInstance;

            public AnonymousClassRunnable3( DelayQueueTests enclosingInstance )
            {
                _enclosingInstance = enclosingInstance;
            }

            public void Run()
            {
                try
                {
                    DelayQueue<PDelay> q = _enclosingInstance.populatedQueue( DEFAULT_COLLECTION_SIZE );
                    for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
                    {
                        Assert.AreEqual( new PDelay( new TimeSpan( i ) ), ( q.Take() ) );
                    }
                    q.Take();
                    Assert.Fail( "Should throw an exception" );
                }
                catch ( ThreadInterruptedException ) {}
            }
        }

        private class AnonymousClassRunnable4
        {
            private DelayQueueTests _enclosingInstance;

            public AnonymousClassRunnable4( DelayQueueTests enclosingInstance )
            {
                _enclosingInstance = enclosingInstance;
            }

            public void Run()
            {
                try
                {
                    DelayQueue<PDelay> q = _enclosingInstance.populatedQueue( DEFAULT_COLLECTION_SIZE );
                    PDelay pDelay;
                    for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
                    {
                        q.Poll(SHORT_DELAY_MS, out pDelay);
                        Assert.AreEqual( new PDelay( new TimeSpan( i ) ), pDelay  );
                    }
                    q.Poll(SHORT_DELAY_MS, out pDelay);
                    Assert.IsNull( null );
                }
                catch ( ThreadInterruptedException ) {}
            }
        }

        private class AnonymousClassRunnable5
        {
            public AnonymousClassRunnable5( DelayQueue<PDelay> q )
            {
                this.q = q;
            }

            private DelayQueue<PDelay> q;

            public void Run()
            {
                try
                {
                    PDelay pDelay;
                    q.Poll(SHORT_DELAY_MS, out pDelay);
                    Assert.IsNull( pDelay  );
                    q.Poll(SHORT_DELAY_MS, out pDelay);
                    q.Poll(SHORT_DELAY_MS, out pDelay);
                    Assert.Fail( "Should block" );
                }
                catch ( ThreadInterruptedException ) {}
            }
        }

        private class AnonymousClassRunnable6 : IRunnable
        {
            public AnonymousClassRunnable6( DelayQueue<PDelay> q )
            {
                this.q = q;
            }

            private DelayQueue<PDelay> q;

            public void Run()
            {
                PDelay pDelay;
                q.Poll(out pDelay);
                Assert.IsNull( pDelay );
                q.Poll(MEDIUM_DELAY_MS, out pDelay);
                Assert.IsTrue( null != pDelay );
                Assert.IsTrue( q.IsEmpty );
            }
        }

        private class AnonymousClassRunnable7 : IRunnable
        {
            public AnonymousClassRunnable7( DelayQueue<PDelay> q )
            {
                this.q = q;
            }

            private DelayQueue<PDelay> q;

            public void Run()
            {
                Thread.Sleep( new TimeSpan( (Int64) 10000*SHORT_DELAY_MS.Milliseconds ) );
                q.Put( new PDelay( new TimeSpan( 1 ) ) );
            }
        }

        private class AnonymousClassRunnable8
        {
            public AnonymousClassRunnable8( DelayQueue<PDelay> q )
            {
                this.q = q;
            }

            private DelayQueue<PDelay> q;

            public void Run()
            {
                q.Put( new PDelay( new TimeSpan( DEFAULT_COLLECTION_SIZE + 1 ) ) );
            }
        }

        private static readonly int NOCAP = Int32.MaxValue;

        internal class PDelay : IDelayed, IComparable
        {
            internal TimeSpan pseudodelay;

            internal PDelay( TimeSpan timeSpan )
            {
                pseudodelay = TimeSpan.MinValue.Add( timeSpan );
            }

            public int CompareTo( Object y )
            {
                TimeSpan i = pseudodelay;
                TimeSpan j = ( (PDelay) y ).pseudodelay;
                if ( i < j )
                {
                    return - 1;
                }
                if ( i > j )
                {
                    return 1;
                }
                return 0;
            }
          

            public int CompareTo( IDelayed y )
            {
                TimeSpan i = pseudodelay;
                TimeSpan j = ( (PDelay) y ).pseudodelay;
                if ( i < j )
                {
                    return - 1;
                }
                if ( i > j )
                {
                    return 1;
                }
                return 0;
            }

            public override bool Equals( Object other )
            {
                return ( (PDelay) other ).pseudodelay == pseudodelay;
            }
            public override int GetHashCode()
            {
                return base.GetHashCode ();
            }

            public bool equals( PDelay other )
            {
                return ( other ).pseudodelay == pseudodelay;
            }

            public TimeSpan GetRemainingDelay()
            {
                return pseudodelay;
            }

            public override String ToString()
            {
                return Convert.ToString( pseudodelay );
            }
        }

        internal class NanoDelay : IDelayed
        {
            public DateTime TriggerTime
            {
                get { return trigger; }
            }

            internal DateTime trigger;

            internal NanoDelay( TimeSpan i )
            {
                trigger = DateTime.Now.Add( i );
            }

            public int CompareTo( Object y )
            {
                DateTime i = trigger;
                DateTime j = ( (NanoDelay) y ).trigger;
                if ( i < j )
                {
                    return - 1;
                }
                if ( i > j )
                {
                    return 1;
                }
                return 0;
            }

            public int CompareTo( IDelayed y )
            {
                DateTime i = trigger;
                DateTime j = ( (NanoDelay) y ).trigger;
                if ( i < j )
                {
                    return - 1;
                }
                if ( i > j )
                {
                    return 1;
                }
                return 0;
            }

            public override bool Equals( Object other )
            {
                return ( (NanoDelay) other ).trigger == trigger;
            }
            public override int GetHashCode()
            {
                return base.GetHashCode ();
            }
            public bool equals( NanoDelay other )
            {
                return ( other ).trigger == trigger;
            }

            public TimeSpan GetRemainingDelay()
            {
                return trigger.Subtract( DateTime.Now );
            }

            public override String ToString()
            {
                return Convert.ToString( trigger );
            }
        }

        private DelayQueue<PDelay> populatedQueue( int n )
        {
            DelayQueue<PDelay> q = new DelayQueue<PDelay>();
            Assert.IsTrue( q.IsEmpty );
            for ( int i = n - 1; i >= 0; i -= 2 )
            {
                Assert.IsTrue( q.Offer( new PDelay( new TimeSpan( i ) ) ) );
            }
            for ( int i = ( n & 1 ); i < n; i += 2 )
            {
                Assert.IsTrue( q.Offer( new PDelay( new TimeSpan( i ) ) ) );
            }
            Assert.IsFalse( q.IsEmpty );
            Assert.AreEqual( NOCAP, q.RemainingCapacity );
            Assert.AreEqual( n, q.Count );
            return q;
        }

        [Test]
        public void Constructor1()
        {
            Assert.AreEqual( NOCAP, new DelayQueue<PDelay>().RemainingCapacity );
        }

        [Test]
        public void Constructor3()
        {
            try
            {
                DelayQueue<PDelay> q = new DelayQueue<PDelay>( null );
                Assert.Fail( "should throw exception" );
            }
            catch ( ArgumentNullException ) {}
        }

        [Test]
        public void Constructor4()
        {
            try
            {
                PDelay[] ints = new PDelay[DEFAULT_COLLECTION_SIZE];

                DelayQueue<PDelay> q = new DelayQueue<PDelay>( new List<PDelay>( ints ) );
                Assert.Fail( "should throw exception" );
            }
            catch ( ArgumentNullException ) {}
        }

        [Test]
        public void Constructor5()
        {
            try
            {
                PDelay[] ints = new PDelay[DEFAULT_COLLECTION_SIZE];
                for ( int i = 0; i < DEFAULT_COLLECTION_SIZE - 1; ++i )
                {
                    ints[i] = new PDelay( new TimeSpan( i ) );
                }

                DelayQueue<PDelay> q = new DelayQueue<PDelay>( new List<PDelay>( ints ) );
                Assert.Fail( "should throw exception" );
            }
            catch ( ArgumentNullException ) {}
        }

        [Test]
        public void Constructor6()
        {
            try
            {
                PDelay[] ints = new PDelay[DEFAULT_COLLECTION_SIZE];
                for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
                {
                    ints[i] = new PDelay( new TimeSpan( i ) );
                }

                DelayQueue<PDelay> q = new DelayQueue<PDelay>( new List<PDelay>( ints ) );
                for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
                {
                    PDelay pDelay;
                    q.Poll(out pDelay);
                    Assert.AreEqual( ints[i], pDelay );
                }
            }
            finally {}
        }

        [Test]
        public void Empty()
        {
            DelayQueue<PDelay> q = new DelayQueue<PDelay>();
            Assert.IsTrue( q.IsEmpty );
            Assert.AreEqual( NOCAP, q.RemainingCapacity );
            q.Add( new PDelay( new TimeSpan( 1 ) ) );
            Assert.IsFalse( q.IsEmpty );
            q.Add( new PDelay( new TimeSpan( 2 ) ) );
            q.Remove();
            q.Remove();
            Assert.IsTrue( q.IsEmpty );
        }

        [Test]
        public void RemainingCapacity()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                Assert.AreEqual( NOCAP, q.RemainingCapacity );
                Assert.AreEqual( DEFAULT_COLLECTION_SIZE - i, q.Count );
                q.Remove();
            }
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                Assert.AreEqual( NOCAP, q.RemainingCapacity );
                Assert.AreEqual( i, q.Count );
                q.Add( new PDelay( new TimeSpan( i ) ) );
            }
        }

        [Test]
        public void OfferNull()
        {
            try
            {
                DelayQueue<PDelay> q = new DelayQueue<PDelay>();
                q.Offer( null );
                Assert.Fail( "should throw exception" );
            }
            catch ( ArgumentNullException ) {}
        }

        [Test]
        public void AddNull()
        {
            try
            {
                DelayQueue<PDelay> q = new DelayQueue<PDelay>();
                q.Add( null );
                Assert.Fail( "should throw exception" );
            }
            catch ( ArgumentNullException ) {}
        }

        [Test]
        public void Offer()
        {
            DelayQueue<PDelay> q = new DelayQueue<PDelay>();
            Assert.IsTrue( q.Offer( new PDelay( new TimeSpan( 0 ) ) ) );
            Assert.IsTrue( q.Offer( new PDelay( new TimeSpan( 1 ) ) ) );
        }

        [Test]
        public void Add()
        {
            DelayQueue<PDelay> q = new DelayQueue<PDelay>();
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                Assert.AreEqual( i, q.Count );
                Assert.IsTrue( q.Offer( new PDelay( new TimeSpan( i ) ) ) );
            }
        }

        [Test]
        public void AddAll1()
        {
            try
            {
                DelayQueue<PDelay> q = new DelayQueue<PDelay>();
                q.AddAll( null );
                Assert.Fail( "should throw exception" );
            }
            catch ( ArgumentNullException ) {}
        }

        [Test]
        public void AddAllSelf()
        {
            try
            {
                DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
                q.AddAll( q );
                Assert.Fail( "should throw exception" );
            }
            catch ( ArgumentException ) {}
        }

        [Test]
        public void AddAll2()
        {
            try
            {
                DelayQueue<PDelay> q = new DelayQueue<PDelay>();
                PDelay[] ints = new PDelay[DEFAULT_COLLECTION_SIZE];

                q.AddAll( new List<PDelay>( ints ) );
                Assert.Fail( "should throw exception" );
            }
            catch ( ArgumentNullException ) {}
        }

        [Test]
        public void AddAll3()
        {
            try
            {
                DelayQueue<PDelay> q = new DelayQueue<PDelay>();
                PDelay[] ints = new PDelay[DEFAULT_COLLECTION_SIZE];
                for ( int i = 0; i < DEFAULT_COLLECTION_SIZE - 1; ++i )
                {
                    ints[i] = new PDelay( new TimeSpan( i ) );
                }

                q.AddAll( new List<PDelay>( ints ) );
                Assert.Fail( "should throw exception" );
            }
            catch ( ArgumentNullException ) {}
        }

        [Test]
        public void AddAll5()
        {
            try
            {
                PDelay[] empty = new PDelay[0];
                PDelay[] ints = new PDelay[DEFAULT_COLLECTION_SIZE];
                for ( int i = DEFAULT_COLLECTION_SIZE - 1; i >= 0; --i )
                {
                    ints[i] = new PDelay( new TimeSpan( i ) );
                }
                DelayQueue<PDelay> q = new DelayQueue<PDelay>();

                Assert.IsFalse( q.AddAll( new List<PDelay>( empty ) ) );

                Assert.IsTrue( q.AddAll( new List<PDelay>( ints ) ) );
               
                for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
                {
                    PDelay pDelay;
                    q.Poll(out pDelay);
                    Assert.AreEqual( ints[i], pDelay);
                }
            }
            finally {}
        }

        [Test]
        public void PutNull()
        {
            try
            {
                DelayQueue<PDelay> q = new DelayQueue<PDelay>();
                q.Put( null );
                Assert.Fail( "should throw exception" );
            }
            catch ( ArgumentNullException ) {}
        }

        [Test]
        public void Put()
        {
            try
            {
                DelayQueue<PDelay> q = new DelayQueue<PDelay>();
                for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
                {
                    PDelay I = new PDelay( new TimeSpan( i ) );
                    q.Put( I );
                }
                Assert.AreEqual( DEFAULT_COLLECTION_SIZE, q.Count );
            }
            finally {}
        }

        [Test]
        public void PutWithTake()
        {
            DelayQueue<PDelay> q = new DelayQueue<PDelay>();
            Thread t = new Thread( new ThreadStart( new AnonymousClassRunnable( q ).Run ) );
            t.Start();

            Thread.Sleep( new TimeSpan( (Int64) 10000*SHORT_DELAY_MS.Milliseconds ) );
            q.Take();
            t.Interrupt();
            t.Join();
        }

        [Test]
        public void TimedOffer()
        {
            DelayQueue<PDelay> q = new DelayQueue<PDelay>();
            Thread t = new Thread( new ThreadStart( new AnonymousClassRunnable1( q ).Run ) );

            t.Start();

            Thread.Sleep( new TimeSpan( (Int64) 10000*SMALL_DELAY_MS.Milliseconds ) );
            t.Interrupt();
            t.Join();
        }

        [Test]
        public void Take()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                Assert.AreEqual( new PDelay( new TimeSpan( i ) ), ( q.Take() ) );
            }
        }

        [Test]
        public void TakeFromEmpty()
        {
            DelayQueue<PDelay> q = new DelayQueue<PDelay>();
            Thread t = new Thread( new ThreadStart( new AnonymousClassRunnable2( q ).Run ) );
            t.Start();

            Thread.Sleep( new TimeSpan( (Int64) 10000*SHORT_DELAY_MS.Milliseconds ) );
            t.Interrupt();
            t.Join();
        }

        [Test]
        public void BlockingTake()
        {
            Thread t = new Thread( new ThreadStart( new AnonymousClassRunnable3( this ).Run ) );
            t.Start();
            Thread.Sleep( new TimeSpan( (Int64) 10000*SHORT_DELAY_MS.Milliseconds ) );
            t.Interrupt();
            t.Join();
        }

        [Test]
        public void Poll()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            PDelay pDelay;
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                q.Poll(out pDelay);
                Assert.AreEqual( new PDelay( new TimeSpan( i ) ), pDelay );
            }
                q.Poll(out pDelay);
            Assert.IsNull( pDelay);
        }

        [Test]
        public void TimedPoll0()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            PDelay pDelay;
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                q.Poll(new TimeSpan(0), out pDelay);
                Assert.AreEqual( new PDelay( new TimeSpan( i ) ), pDelay );
            }
            q.Poll(new TimeSpan(0), out pDelay);
            Assert.IsNull( pDelay );
        }

        [Test]
        public void TimedPoll()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );

            PDelay pDelay;
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                q.Poll(SHORT_DELAY_MS, out pDelay);
                Assert.AreEqual( new PDelay( new TimeSpan( i ) ), pDelay );
            }
                q.Poll(SHORT_DELAY_MS, out pDelay);
            Assert.IsNull( pDelay );
        }

        [Test]
        public void InterruptedTimedPoll()
        {
            Thread t = new Thread( new ThreadStart( new AnonymousClassRunnable4( this ).Run ) );
            t.Start();
            Thread.Sleep( new TimeSpan( (Int64) 10000*SHORT_DELAY_MS.Milliseconds ) );
            t.Interrupt();
            t.Join();
        }

        [Test]
        public void TimedPollWithOffer()
        {
            DelayQueue<PDelay> q = new DelayQueue<PDelay>();
            Thread t = new Thread( new ThreadStart( new AnonymousClassRunnable5( q ).Run ) );
            t.Start();

            Thread.Sleep( new TimeSpan( (Int64) 10000*SMALL_DELAY_MS.Milliseconds ) );
            Assert.IsTrue( q.Offer( new PDelay( new TimeSpan( 0 ) ), SHORT_DELAY_MS ) );
            t.Interrupt();
            t.Join();
        }

        [Test]
        public void Peek()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            PDelay pDelay;
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                q.Peek(out pDelay);
                Assert.AreEqual( new PDelay( new TimeSpan( i ) ), pDelay );
                q.Poll(out pDelay);
                if ( q.IsEmpty )
                {
                    q.Peek(out pDelay);
                    Assert.IsNull( pDelay );
                }
                else
                {
                    q.Peek(out pDelay);
                    Assert.IsTrue( i != pDelay.pseudodelay.Ticks );
                }
            }
            q.Peek(out pDelay);
            Assert.IsNull( pDelay);
        }

        [Test]
        public void Element()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                Assert.AreEqual( new PDelay( new TimeSpan( i ) ), ( q.Element() ) );
                PDelay pDelay;
                q.Poll(out pDelay);
            }
            try
            {
                q.Element();
                Assert.Fail( "should throw exception" );
            }
            catch ( NoElementsException ) {}
        }

        [Test]
        public void Remove()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                Assert.AreEqual( new PDelay( new TimeSpan( i ) ), ( q.Remove() ) );
            }
            try
            {
                q.Remove();
                Assert.Fail( "should throw exception" );
            }
            catch ( NoElementsException ) {}
        }

        [Test]
        public void RemoveElement()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            for ( int i = 1; i < DEFAULT_COLLECTION_SIZE; i += 2 )
            {
                Assert.IsTrue( q.Remove( new PDelay( new TimeSpan( i ) ) ) );
            }
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; i += 2 )
            {
                Assert.IsTrue( q.Remove( new PDelay( new TimeSpan( i ) ) ) );
                Assert.IsFalse( q.Remove( new PDelay( new TimeSpan( i + 1 ) ) ) );
            }
            Assert.IsTrue( q.IsEmpty );
        }

        [Test]
        public void Contains()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            PDelay pDelay;
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                Assert.IsTrue(q.Poll(out pDelay));
            }
        }

        [Test]
        public void Clear()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            q.Clear();
            Assert.IsTrue( q.IsEmpty );
            Assert.AreEqual( 0, q.Count );
            Assert.AreEqual( NOCAP, q.RemainingCapacity );
            PDelay x = new PDelay( new TimeSpan( 1 ) );
            q.Add( x );
            Assert.IsFalse( q.IsEmpty );
            q.Clear();
            Assert.IsTrue( q.IsEmpty );
        }

        [Test]
        public void ContainsAll()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            DelayQueue<PDelay> p = new DelayQueue<PDelay>();
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                p.Add( new PDelay( new TimeSpan( i ) ) );
            }
        }

        [Test]
        public void ToArray()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            PDelay[] o = new PDelay[DEFAULT_COLLECTION_SIZE];
            q.CopyTo( o, 0 );

            Array.Sort( o );
            for ( int i = 0; i < o.Length; i++ )
            {
                Assert.AreEqual( o[i], q.Take() );
            }
        }

        [Test]
        public void ToArray2()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            PDelay[] ints = new PDelay[DEFAULT_COLLECTION_SIZE];
            q.CopyTo( ints, 0 );

            Array.Sort( ints );
            for ( int i = 0; i < ints.Length; i++ )
            {
                Assert.AreEqual( ints[i], q.Take() );
            }
        }

        [Test]
        public void ToArray_BadArg()
        {
            try
            {
                DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
                q.CopyTo( null, 0 );
                Assert.Fail( "should throw exception" );
            }
            catch ( ArgumentNullException ) {}
        }

        [Test]
        public void Iterator()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            int counter = 0;
			
            for ( int i = 0; i < q.Count; i++ )
            {
                ++counter;
            }
            Assert.AreEqual( counter, DEFAULT_COLLECTION_SIZE );
        }

        [Test]
        [Ignore( "Re-evaluate when tests for ThreadPoolExecutor are created. " )]
        public void PollInExecutor()
        {
            DelayQueue<PDelay> q = new DelayQueue<PDelay>();
            IExecutorService executor = Executors.NewFixedThreadPool( 2 );
            executor.Execute( new AnonymousClassRunnable6( q ) );

            executor.Execute( new AnonymousClassRunnable7( q ) );
            JoinPool( executor );
        }

        [Test]
        public void Delay()
        {
            DelayQueue<IDelayed> q = new DelayQueue<IDelayed>();
            NanoDelay[] Elements = new NanoDelay[DEFAULT_COLLECTION_SIZE];
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                Elements[i] = new NanoDelay( new TimeSpan( DEFAULT_COLLECTION_SIZE - i ) );
            }
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                q.Add( Elements[i] );
            }

            DateTime last = DateTime.MinValue;
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                NanoDelay e = (NanoDelay) ( q.Take() );
                DateTime tt = e.TriggerTime;
                Assert.IsTrue( tt <= DateTime.Now );
                if ( i != 0 )
                {
                    Assert.IsTrue( tt >= last );
                }
                last = tt;
            }
        }

        [Test]
        public void PeekDelayed()
        {
            DelayQueue<IDelayed> q = new DelayQueue<IDelayed>();
            q.Add( new NanoDelay( LONG_DELAY_MS ) );
            IDelayed nd;
            Assert.IsFalse( q.Peek(out nd) );
        }

        [Test]
        public void PollDelayed()
        {
            DelayQueue<IDelayed> q = new DelayQueue<IDelayed>();
            q.Add( new NanoDelay( LONG_DELAY_MS ) );
            IDelayed nd;
            Assert.IsFalse( q.Poll(out nd) );
        }

        [Test]
        public void DrainToNull()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            try
            {
                q.DrainTo( null );
                Assert.Fail( "should throw exception" );
            }
            catch ( ArgumentNullException ) {}
        }

        [Test]
        public void DrainToSelf()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            try
            {
                q.DrainTo( q );
                Assert.Fail( "should throw exception" );
            }
            catch ( ArgumentException ) {}
        }

        [Test]
        public void DrainTo()
        {
            DelayQueue<PDelay> q = new DelayQueue<PDelay>();
            PDelay[] elems = new PDelay[DEFAULT_COLLECTION_SIZE];
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                elems[i] = new PDelay( new TimeSpan( i ) );
                q.Add( elems[i] );
            }
            IList<PDelay> l = new List<PDelay>();
            q.DrainTo( l );
            Assert.AreEqual( q.Count, 0 );
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i )
            {
                Assert.AreEqual( l[i], elems[i] );
            }
            q.Add( elems[0] );
            q.Add( elems[1] );
            Assert.IsFalse( q.IsEmpty );

            l.Clear();
            q.DrainTo( l );
            Assert.AreEqual( q.Count, 0 );
            Assert.AreEqual( l.Count, 2 );
            for ( int i = 0; i < 2; ++i )
            {
                Assert.AreEqual( l[i], elems[i] );
            }
        }

        [Test]
        public void DrainToWithActivePut()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            Thread t = new Thread( new ThreadStart( new AnonymousClassRunnable8( q ).Run ) );
            t.Start();
            IList<PDelay> l = new List<PDelay>();
            q.DrainTo( l );
            Assert.IsTrue( l.Count >= DEFAULT_COLLECTION_SIZE );
            t.Join();
            Assert.IsTrue( q.Count + l.Count >= DEFAULT_COLLECTION_SIZE );
        }

        [Test]
        public void DrainToNullN()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            try
            {
                q.DrainTo( null, 0 );
                Assert.Fail( "should throw exception" );
            }
            catch ( ArgumentNullException ) {}
        }

        [Test]
        public void DrainToSelfN()
        {
            DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
            try
            {
                q.DrainTo( q, 0 );
                Assert.Fail( "should throw exception" );
            }
            catch ( ArgumentException ) {}
        }

        [Test]
        public void DrainToN()
        {
            for ( int i = 0; i < DEFAULT_COLLECTION_SIZE + 2; ++i )
            {
                DelayQueue<PDelay> q = populatedQueue( DEFAULT_COLLECTION_SIZE );
                IList<PDelay> l = new List<PDelay>();
                q.DrainTo( l, i );
                int k = ( i < DEFAULT_COLLECTION_SIZE ) ? i : DEFAULT_COLLECTION_SIZE;
                Assert.AreEqual( q.Count, DEFAULT_COLLECTION_SIZE - k );
                Assert.AreEqual( l.Count, k );
            }
        }
    }
}