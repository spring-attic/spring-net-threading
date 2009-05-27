using System;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spring.Threading.Collections;

namespace Spring.Threading.Tests.Collections {
    /// <author>Doug Lea>author>
    /// <author>Andreas Döhring (.NET)</author>
    [TestFixture]
    public class SynchronousQueueTest : BaseThreadingTestCase {

        private class TestReferenceType {
        }

        /// <summary>
        /// A SynchronousQueue is both empty and full
        /// </summary>
        [Test]
        public void TestEmptyFull() {
            SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>();
            Assert.IsTrue(q.IsEmpty);
            Assert.That(q.Count, Is.EqualTo(0));
            Assert.That(q.RemainingCapacity, Is.EqualTo(0));
            Assert.IsFalse(q.Offer(new TestReferenceType()));
        }

        /// <summary>
        /// A SynchronousQueue is both empty and full
        /// </summary>
        [Test]
        public void TestEmptyFull2() {
            SynchronousQueue<int> q = new SynchronousQueue<int>();
            Assert.IsTrue(q.IsEmpty);
            Assert.That(q.Count, Is.EqualTo(0));
            Assert.That(q.RemainingCapacity, Is.EqualTo(0));
            Assert.IsFalse(q.Offer(1));
        }

        /// <summary>
        /// A fair SynchronousQueue is both empty and full
        /// </summary>
        [Test]
        public void TestFairEmptyFull() {
            SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>(true);
            Assert.IsTrue(q.IsEmpty);
            Assert.That(q.Count, Is.EqualTo(0));
            Assert.That(q.RemainingCapacity, Is.EqualTo(0));
            Assert.IsFalse(q.Offer(new TestReferenceType()));
        }

        /// <summary>
        /// A fair SynchronousQueue is both empty and full
        /// </summary>
        [Test]
        public void TestFairEmptyFull2() {
            SynchronousQueue<int> q = new SynchronousQueue<int>(true);
            Assert.IsTrue(q.IsEmpty);
            Assert.That(q.Count, Is.EqualTo(0));
            Assert.That(q.RemainingCapacity, Is.EqualTo(0));
            Assert.IsFalse(q.Offer(1));
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestOfferNull() {
            SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>();
            q.Offer(null);
        }

        [Test]
        public void TestOfferNull2() {
            SynchronousQueue<int> q = new SynchronousQueue<int>();
            q.Offer(0);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestAddNull() {
            SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>();
            q.Add(null);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void TestAddNull2() {
            SynchronousQueue<int> q = new SynchronousQueue<int>();
            q.Add(0);
        }

        /// <summary>
        /// offer fails if no active taker
        /// </summary>
        [Test]
        public void TestOffer() {
            SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>();
            Assert.IsFalse(q.Offer(new TestReferenceType()));
        }

        /// <summary>
        /// offer fails if no active taker
        /// </summary>
        [Test]
        public void TestOffer2() {
            SynchronousQueue<int> q = new SynchronousQueue<int>();
            Assert.IsFalse(q.Offer(0));
        }

        /// <summary>
        /// add fails if no active taker
        /// </summary>
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void TestAdd() {
            SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>();
            Assert.That(q.RemainingCapacity, Is.EqualTo(0));
            q.Add(new TestReferenceType());
        }

        /// <summary>
        /// add fails if no active taker
        /// </summary>
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void TestAdd2() {
            SynchronousQueue<int> q = new SynchronousQueue<int>();
            Assert.That(q.RemainingCapacity, Is.EqualTo(0));
            q.Add(0);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestAddAllDefaultT() {
            SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>();
            q.AddAll(null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestAddAllDefaultT2() {
            SynchronousQueue<int> q = new SynchronousQueue<int>();
            q.AddAll(null);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void TestAddAllSelf() {
            SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>();
            q.AddAll(q);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void TestAddAllSelf2() {
            SynchronousQueue<int> q = new SynchronousQueue<int>();
            q.AddAll(q);
        }

        /// <summary>
        /// addAll of a collection with null elements fails
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestAddAllWithDefaultElements() {
            SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>();
            TestReferenceType[] test = new TestReferenceType[1];
            q.AddAll(test);
        }

        /// <summary>
        /// addAll of a collection with default value type elements fails
        /// </summary>
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void TestAddAllWithDefaultElements2() {
            SynchronousQueue<int> q = new SynchronousQueue<int>();
            int[] test = new int[1];
            q.AddAll(test);
        }

        /// <summary>
        /// addAll fails if no active taker
        /// </summary>
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void TestAddAllWithoutActiveTaker() {
            SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>();
            TestReferenceType[] test = new TestReferenceType[1];
            for(int i = 0; i < 1; ++i)
                test[i] = new TestReferenceType();
            q.AddAll(test);
        }

        /// <summary>
        /// put fails if no active taker
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TestPutDefault() {
            SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>();
            q.Put(null);
        }

        /// <summary>
        /// put blocks interruptibly if no active taker
        /// </summary>
        [Test]
        public void TestBlockingPut() {
            Thread t = new Thread(new ThreadStart(delegate {
                try {
                    SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>();
                    q.Put(new TestReferenceType());
                }
                catch(ThreadInterruptedException) { }
            }));
            t.Start();
            Thread.Sleep(50);
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// put blocks interruptibly if no active taker
        /// </summary>
        [Test]
        public void TestBlockingPut2() {
            Thread t = new Thread(new ThreadStart(delegate {
                try {
                    SynchronousQueue<int> q = new SynchronousQueue<int>();
                    q.Put(1);
                }
                catch(ThreadInterruptedException) { }
            }));
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// put blocks waiting for take
        /// </summary>
        [Test]
        public void TestPutWithTake() {
            SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>();
            Thread t = new Thread(new ThreadStart(delegate {
                int added = 0;
                try {
                    q.Put(new TestReferenceType());
                    ++added;
                    q.Put(new TestReferenceType());
                    ++added;
                    q.Put(new TestReferenceType());
                    ++added;
                    q.Put(new TestReferenceType());
                    ++added;
                }
                catch(ThreadInterruptedException) {
                    Assert.IsTrue(added >= 1);
                }
            }));
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            q.Take();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// put blocks waiting for take
        /// </summary>
        [Test]
        public void TestPutWithTake2() {
            SynchronousQueue<int> q = new SynchronousQueue<int>();
            Thread t = new Thread(new ThreadStart(delegate {
                int added = 0;
                try {
                    q.Put(1);
                    ++added;
                    q.Put(2);
                    ++added;
                    q.Put(3);
                    ++added;
                    q.Put(4);
                    ++added;
                }
                catch(ThreadInterruptedException) {
                    Assert.IsTrue(added >= 1);
                }
            }));
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            q.Take();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// timed offer times out if elements not taken
        /// </summary>
        [Test]
        public void TestTimedOffer() {
            SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>();
            Thread t = new Thread(new ThreadStart(delegate {
                try {
                    Assert.IsFalse(q.Offer(new TestReferenceType(), SHORT_DELAY_MS));
                    q.Offer(new TestReferenceType(), LONG_DELAY_MS);
                }
                catch(ThreadInterruptedException) { }
            }));

            t.Start();
            Thread.Sleep(SMALL_DELAY_MS);
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// timed offer times out if elements not taken
        /// </summary>
        [Test]
        public void TestTimedOffer2() {
            SynchronousQueue<int> q = new SynchronousQueue<int>();
            Thread t = new Thread(new ThreadStart(delegate {
                try {
                    Assert.IsFalse(q.Offer(1, SHORT_DELAY_MS));
                    q.Offer(2, LONG_DELAY_MS);
                }
                catch(ThreadInterruptedException) { }
            }));

            t.Start();
            Thread.Sleep(SMALL_DELAY_MS);
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// take blocks interruptibly when empty
        /// </summary>
        [Test]
        public void TestTakeFromEmpty() {
            SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>();
            Thread t = new Thread(new ThreadStart(delegate {
                try {
                    q.Take();
                }
                catch(ThreadInterruptedException) { }
            }));
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// take blocks interruptibly when empty
        /// </summary>
        [Test]
        public void TestTakeFromEmpty2() {
            SynchronousQueue<int> q = new SynchronousQueue<int>();
            Thread t = new Thread(new ThreadStart(delegate {
                try {
                    q.Take();
                }
                catch(ThreadInterruptedException) { }
            }));
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// put blocks interruptibly if no active taker
        /// </summary>
        [Test]
        public void TestFairBlockingPut() {
            Thread t = new Thread(new ThreadStart(delegate {
                try {
                    SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>(true);
                    q.Put(new TestReferenceType());
                }
                catch(ThreadInterruptedException) { }
            }));
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// put blocks interruptibly if no active taker
        /// </summary>
        [Test]
        public void TestFairBlockingPut2() {
            Thread t = new Thread(new ThreadStart(delegate {
                try {
                    SynchronousQueue<int> q = new SynchronousQueue<int>(true);
                    q.Put(1);
                }
                catch(ThreadInterruptedException) { }
            }));
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// put blocks waiting for take
        /// </summary>
        [Test]
        public void TestFairPutWithTake() {
            SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>(true);
            Thread t = new Thread(new ThreadStart(delegate {
                int added = 0;
                try {
                    q.Put(new TestReferenceType());
                    ++added;
                    q.Put(new TestReferenceType());
                    ++added;
                    q.Put(new TestReferenceType());
                    ++added;
                    q.Put(new TestReferenceType());
                    ++added;
                }
                catch(ThreadInterruptedException) {
                    Assert.IsTrue(added >= 1);
                }
            }));
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            q.Take();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// put blocks waiting for take
        /// </summary>
        [Test]
        public void TestFairPutWithTake2() {
            SynchronousQueue<int> q = new SynchronousQueue<int>(true);
            Thread t = new Thread(new ThreadStart(delegate {
                int added = 0;
                try {
                    q.Put(1);
                    ++added;
                    q.Put(2);
                    ++added;
                    q.Put(3);
                    ++added;
                    q.Put(4);
                    ++added;
                }
                catch(ThreadInterruptedException) {
                    Assert.IsTrue(added >= 1);
                }
            }));
            t.Start();
            Thread.Sleep(SHORT_DELAY_MS);
            q.Take();
            Thread.Sleep(SHORT_DELAY_MS);
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// timed offer times out if elements not taken
        /// </summary>
        [Test]
        public void TestFairTimedOffer() {
            SynchronousQueue<TestReferenceType> q = new SynchronousQueue<TestReferenceType>(true);
            Thread t = new Thread(new ThreadStart(delegate {
                try {
                    Assert.IsFalse(q.Offer(new TestReferenceType(), SHORT_DELAY_MS));
                    q.Offer(new TestReferenceType(), LONG_DELAY_MS);
                }
                catch(ThreadInterruptedException) { }
            }));

            t.Start();
            Thread.Sleep(SMALL_DELAY_MS);
            t.Interrupt();
            t.Join();
        }

        /// <summary>
        /// timed offer times out if elements not taken
        /// </summary>
        [Test]
        public void TestFairTimedOffer2() {
            SynchronousQueue<int> q = new SynchronousQueue<int>(true);
            Thread t = new Thread(new ThreadStart(delegate {
                try {
                    Assert.IsFalse(q.Offer(1, SHORT_DELAY_MS));
                    q.Offer(2, LONG_DELAY_MS);
                }
                catch(ThreadInterruptedException) { }
            }));

            t.Start();
            Thread.Sleep(SMALL_DELAY_MS);
            t.Interrupt();
            t.Join();
        }


        ///**
        // * take blocks interruptibly when empty
        // */
        //public void testFairTakeFromEmpty() {
        //    final SynchronousQueue q = new SynchronousQueue(true);
        //    Thread t = new Thread(new Runnable() {
        //            public void run() {
        //                try {
        //                    q.take();
        //        threadShouldThrow();
        //                } catch (InterruptedException success){ }
        //            }
        //        });
        //    try {
        //        t.start();
        //        Thread.sleep(SHORT_DELAY_MS);
        //        t.interrupt();
        //        t.join();
        //    } catch (Exception e){
        //        unexpectedException();
        //    }
        //}

        ///**
        // * poll fails unless active taker
        // */
        //public void testPoll() {
        //    SynchronousQueue q = new SynchronousQueue();
        //assertNull(q.poll());
        //}

        ///**
        // * timed pool with zero timeout times out if no active taker
        // */
        //public void testTimedPoll0() {
        //    try {
        //        SynchronousQueue q = new SynchronousQueue();
        //        assertNull(q.poll(0, TimeUnit.MILLISECONDS));
        //    } catch (InterruptedException e){
        //    unexpectedException();
        //}
        //}

        ///**
        // * timed pool with nonzero timeout times out if no active taker
        // */
        //public void testTimedPoll() {
        //    try {
        //        SynchronousQueue q = new SynchronousQueue();
        //        assertNull(q.poll(SHORT_DELAY_MS, TimeUnit.MILLISECONDS));
        //    } catch (InterruptedException e){
        //    unexpectedException();
        //}
        //}

        ///**
        // * Interrupted timed poll throws InterruptedException instead of
        // * returning timeout status
        // */
        //public void testInterruptedTimedPoll() {
        //    Thread t = new Thread(new Runnable() {
        //            public void run() {
        //                try {
        //                    SynchronousQueue q = new SynchronousQueue();
        //                    assertNull(q.poll(SHORT_DELAY_MS, TimeUnit.MILLISECONDS));
        //                } catch (InterruptedException success){
        //                }
        //            }});
        //    t.start();
        //    try {
        //       Thread.sleep(SHORT_DELAY_MS);
        //       t.interrupt();
        //       t.join();
        //    }
        //    catch (InterruptedException ie) {
        //    unexpectedException();
        //    }
        //}

        ///**
        // *  timed poll before a delayed offer fails; after offer succeeds;
        // *  on interruption throws
        // */
        //public void testTimedPollWithOffer() {
        //    final SynchronousQueue q = new SynchronousQueue();
        //    Thread t = new Thread(new Runnable() {
        //            public void run() {
        //                try {
        //                    threadAssertNull(q.poll(SHORT_DELAY_MS, TimeUnit.MILLISECONDS));
        //                    q.poll(LONG_DELAY_MS, TimeUnit.MILLISECONDS);
        //                    q.poll(LONG_DELAY_MS, TimeUnit.MILLISECONDS);
        //        threadShouldThrow();
        //                } catch (InterruptedException success) { }
        //            }
        //        });
        //    try {
        //        t.start();
        //        Thread.sleep(SMALL_DELAY_MS);
        //        assertTrue(q.offer(zero, SHORT_DELAY_MS, TimeUnit.MILLISECONDS));
        //        t.interrupt();
        //        t.join();
        //    } catch (Exception e){
        //        unexpectedException();
        //    }
        //}

        ///**
        // * Interrupted timed poll throws InterruptedException instead of
        // * returning timeout status
        // */
        //public void testFairInterruptedTimedPoll() {
        //    Thread t = new Thread(new Runnable() {
        //            public void run() {
        //                try {
        //                    SynchronousQueue q = new SynchronousQueue(true);
        //                    assertNull(q.poll(SHORT_DELAY_MS, TimeUnit.MILLISECONDS));
        //                } catch (InterruptedException success){
        //                }
        //            }});
        //    t.start();
        //    try {
        //       Thread.sleep(SHORT_DELAY_MS);
        //       t.interrupt();
        //       t.join();
        //    }
        //    catch (InterruptedException ie) {
        //    unexpectedException();
        //    }
        //}

        ///**
        // *  timed poll before a delayed offer fails; after offer succeeds;
        // *  on interruption throws
        // */
        //public void testFairTimedPollWithOffer() {
        //    final SynchronousQueue q = new SynchronousQueue(true);
        //    Thread t = new Thread(new Runnable() {
        //            public void run() {
        //                try {
        //                    threadAssertNull(q.poll(SHORT_DELAY_MS, TimeUnit.MILLISECONDS));
        //                    q.poll(LONG_DELAY_MS, TimeUnit.MILLISECONDS);
        //                    q.poll(LONG_DELAY_MS, TimeUnit.MILLISECONDS);
        //        threadShouldThrow();
        //                } catch (InterruptedException success) { }
        //            }
        //        });
        //    try {
        //        t.start();
        //        Thread.sleep(SMALL_DELAY_MS);
        //        assertTrue(q.offer(zero, SHORT_DELAY_MS, TimeUnit.MILLISECONDS));
        //        t.interrupt();
        //        t.join();
        //    } catch (Exception e){
        //        unexpectedException();
        //    }
        //}


        ///**
        // * peek returns null
        // */
        //public void testPeek() {
        //    SynchronousQueue q = new SynchronousQueue();
        //assertNull(q.peek());
        //}

        ///**
        // * element throws NSEE
        // */
        //public void testElement() {
        //    SynchronousQueue q = new SynchronousQueue();
        //    try {
        //        q.element();
        //        shouldThrow();
        //    }
        //    catch (NoSuchElementException success) {}
        //}

        ///**
        // * remove throws NSEE if no active taker
        // */
        //public void testRemove() {
        //    SynchronousQueue q = new SynchronousQueue();
        //    try {
        //        q.remove();
        //        shouldThrow();
        //    } catch (NoSuchElementException success){
        //}
        //}

        ///**
        // * remove(x) returns false
        // */
        //public void testRemoveElement() {
        //    SynchronousQueue q = new SynchronousQueue();
        //    assertFalse(q.remove(zero));
        //    assertTrue(q.isEmpty());
        //}

        ///**
        // * contains returns false
        // */
        //public void testContains() {
        //    SynchronousQueue q = new SynchronousQueue();
        //    assertFalse(q.contains(zero));
        //}

        ///**
        // * clear ensures isEmpty
        // */
        //public void testClear() {
        //    SynchronousQueue q = new SynchronousQueue();
        //    q.clear();
        //    assertTrue(q.isEmpty());
        //}

        ///**
        // * containsAll returns false unless empty
        // */
        //public void testContainsAll() {
        //    SynchronousQueue q = new SynchronousQueue();
        //    Integer[] empty = new Integer[0];
        //    assertTrue(q.containsAll(Arrays.asList(empty)));
        //    Integer[] ints = new Integer[1]; ints[0] = zero;
        //    assertFalse(q.containsAll(Arrays.asList(ints)));
        //}

        ///**
        // * retainAll returns false
        // */
        //public void testRetainAll() {
        //    SynchronousQueue q = new SynchronousQueue();
        //    Integer[] empty = new Integer[0];
        //    assertFalse(q.retainAll(Arrays.asList(empty)));
        //    Integer[] ints = new Integer[1]; ints[0] = zero;
        //    assertFalse(q.retainAll(Arrays.asList(ints)));
        //}

        ///**
        // * removeAll returns false
        // */
        //public void testRemoveAll() {
        //    SynchronousQueue q = new SynchronousQueue();
        //    Integer[] empty = new Integer[0];
        //    assertFalse(q.removeAll(Arrays.asList(empty)));
        //    Integer[] ints = new Integer[1]; ints[0] = zero;
        //    assertFalse(q.containsAll(Arrays.asList(ints)));
        //}


        ///**
        // * toArray is empty
        // */
        //public void testToArray() {
        //    SynchronousQueue q = new SynchronousQueue();
        //Object[] o = q.toArray();
        //    assertEquals(o.length, 0);
        //}

        ///**
        // * toArray(a) is nulled at position 0
        // */
        //public void testToArray2() {
        //    SynchronousQueue q = new SynchronousQueue();
        //Integer[] ints = new Integer[1];
        //    assertNull(ints[0]);
        //}

        ///**
        // * toArray(null) throws NPE
        // */
        //public void testToArray_BadArg() {
        //try {
        //        SynchronousQueue q = new SynchronousQueue();
        //    Object o[] = q.toArray(null);
        //    shouldThrow();
        //} catch(NullPointerException success){}
        //}


        ///**
        // * iterator does not traverse any elements
        // */
        //public void testIterator() {
        //    SynchronousQueue q = new SynchronousQueue();
        //Iterator it = q.iterator();
        //    assertFalse(it.hasNext());
        //    try {
        //        Object x = it.next();
        //        shouldThrow();
        //    }
        //    catch (NoSuchElementException success) {}
        //}

        ///**
        // * iterator remove throws ISE
        // */
        //public void testIteratorRemove() {
        //    SynchronousQueue q = new SynchronousQueue();
        //Iterator it = q.iterator();
        //    try {
        //        it.remove();
        //        shouldThrow();
        //    }
        //    catch (IllegalStateException success) {}
        //}

        ///**
        // * toString returns a non-null string
        // */
        //public void testToString() {
        //    SynchronousQueue q = new SynchronousQueue();
        //    String s = q.toString();
        //    assertNotNull(s);
        //}


        ///**
        // * offer transfers elements across Executor tasks
        // */
        //public void testOfferInExecutor() {
        //    final SynchronousQueue q = new SynchronousQueue();
        //    ExecutorService executor = Executors.newFixedThreadPool(2);
        //    final Integer one = new Integer(1);

        //    executor.execute(new Runnable() {
        //        public void run() {
        //            threadAssertFalse(q.offer(one));
        //            try {
        //                threadAssertTrue(q.offer(one, MEDIUM_DELAY_MS, TimeUnit.MILLISECONDS));
        //                threadAssertEquals(0, q.remainingCapacity());
        //            }
        //            catch (InterruptedException e) {
        //                threadUnexpectedException();
        //            }
        //        }
        //    });

        //    executor.execute(new Runnable() {
        //        public void run() {
        //            try {
        //                Thread.sleep(SMALL_DELAY_MS);
        //                threadAssertEquals(one, q.take());
        //            }
        //            catch (InterruptedException e) {
        //                threadUnexpectedException();
        //            }
        //        }
        //    });

        //    joinPool(executor);

        //}

        ///**
        // * poll retrieves elements across Executor threads
        // */
        //public void testPollInExecutor() {
        //    final SynchronousQueue q = new SynchronousQueue();
        //    ExecutorService executor = Executors.newFixedThreadPool(2);
        //    executor.execute(new Runnable() {
        //        public void run() {
        //            threadAssertNull(q.poll());
        //            try {
        //                threadAssertTrue(null != q.poll(MEDIUM_DELAY_MS, TimeUnit.MILLISECONDS));
        //                threadAssertTrue(q.isEmpty());
        //            }
        //            catch (InterruptedException e) {
        //                threadUnexpectedException();
        //            }
        //        }
        //    });

        //    executor.execute(new Runnable() {
        //        public void run() {
        //            try {
        //                Thread.sleep(SMALL_DELAY_MS);
        //                q.put(new Integer(1));
        //            }
        //            catch (InterruptedException e) {
        //                threadUnexpectedException();
        //            }
        //        }
        //    });

        //    joinPool(executor);
        //}

        ///**
        // * a deserialized serialized queue is usable
        // */
        //public void testSerialization() {
        //    SynchronousQueue q = new SynchronousQueue();
        //    try {
        //        ByteArrayOutputStream bout = new ByteArrayOutputStream(10000);
        //        ObjectOutputStream out = new ObjectOutputStream(new BufferedOutputStream(bout));
        //        out.writeObject(q);
        //        out.close();

        //        ByteArrayInputStream bin = new ByteArrayInputStream(bout.toByteArray());
        //        ObjectInputStream in = new ObjectInputStream(new BufferedInputStream(bin));
        //        SynchronousQueue r = (SynchronousQueue)in.readObject();
        //        assertEquals(q.size(), r.size());
        //        while (!q.isEmpty())
        //            assertEquals(q.remove(), r.remove());
        //    } catch(Exception e){
        //        e.printStackTrace();
        //        unexpectedException();
        //    }
        //}

        ///**
        // * drainTo(null) throws NPE
        // */
        //public void testDrainToNull() {
        //    SynchronousQueue q = new SynchronousQueue();
        //    try {
        //        q.drainTo(null);
        //        shouldThrow();
        //    } catch(NullPointerException success) {
        //    }
        //}

        ///**
        // * drainTo(this) throws IAE
        // */
        //public void testDrainToSelf() {
        //    SynchronousQueue q = new SynchronousQueue();
        //    try {
        //        q.drainTo(q);
        //        shouldThrow();
        //    } catch(IllegalArgumentException success) {
        //    }
        //}

        ///**
        // * drainTo(c) of empty queue doesn't transfer elements
        // */
        //public void testDrainTo() {
        //    SynchronousQueue q = new SynchronousQueue();
        //    ArrayList l = new ArrayList();
        //    q.drainTo(l);
        //    assertEquals(q.size(), 0);
        //    assertEquals(l.size(), 0);
        //}

        ///**
        // * drainTo empties queue, unblocking a waiting put.
        // */
        //public void testDrainToWithActivePut() {
        //    final SynchronousQueue q = new SynchronousQueue();
        //    Thread t = new Thread(new Runnable() {
        //            public void run() {
        //                try {
        //                    q.put(new Integer(1));
        //                } catch (InterruptedException ie){
        //                    threadUnexpectedException();
        //                }
        //            }
        //        });
        //    try {
        //        t.start();
        //        ArrayList l = new ArrayList();
        //        Thread.sleep(SHORT_DELAY_MS);
        //        q.drainTo(l);
        //        assertTrue(l.size() <= 1);
        //        if (l.size() > 0)
        //            assertEquals(l.get(0), new Integer(1));
        //        t.join();
        //        assertTrue(l.size() <= 1);
        //    } catch(Exception e){
        //        unexpectedException();
        //    }
        //}

        ///**
        // * drainTo(null, n) throws NPE
        // */
        //public void testDrainToNullN() {
        //    SynchronousQueue q = new SynchronousQueue();
        //    try {
        //        q.drainTo(null, 0);
        //        shouldThrow();
        //    } catch(NullPointerException success) {
        //    }
        //}

        ///**
        // * drainTo(this, n) throws IAE
        // */
        //public void testDrainToSelfN() {
        //    SynchronousQueue q = new SynchronousQueue();
        //    try {
        //        q.drainTo(q, 0);
        //        shouldThrow();
        //    } catch(IllegalArgumentException success) {
        //    }
        //}

        ///**
        // * drainTo(c, n) empties up to n elements of queue into c
        // */
        //public void testDrainToN() {
        //    final SynchronousQueue q = new SynchronousQueue();
        //    Thread t1 = new Thread(new Runnable() {
        //            public void run() {
        //                try {
        //                    q.put(one);
        //                } catch (InterruptedException ie){
        //                    threadUnexpectedException();
        //                }
        //            }
        //        });
        //    Thread t2 = new Thread(new Runnable() {
        //            public void run() {
        //                try {
        //                    q.put(two);
        //                } catch (InterruptedException ie){
        //                    threadUnexpectedException();
        //                }
        //            }
        //        });

        //    try {
        //        t1.start();
        //        t2.start();
        //        ArrayList l = new ArrayList();
        //        Thread.sleep(SHORT_DELAY_MS);
        //        q.drainTo(l, 1);
        //        assertTrue(l.size() == 1);
        //        q.drainTo(l, 1);
        //        assertTrue(l.size() == 2);
        //        assertTrue(l.contains(one));
        //        assertTrue(l.contains(two));
        //        t1.join();
        //        t2.join();
        //    } catch(Exception e){
        //        unexpectedException();
        //    }
        //}


    }
}