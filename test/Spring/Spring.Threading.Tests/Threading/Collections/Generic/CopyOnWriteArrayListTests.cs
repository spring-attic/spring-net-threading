using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace Spring.Threading.Collections.Generic {
    [TestFixture]
    public class CopyOnWriteArrayListTests : BaseThreadingTestCase {
        internal static CopyOnWriteArrayList<int> populatedArray(int n) {
            CopyOnWriteArrayList<int> a = new CopyOnWriteArrayList<int>();
            Assert.IsTrue(a.IsEmpty);
            for(int i = 0; i < n; ++i) {
                a.Add(i);
            }
            Assert.IsFalse(a.IsEmpty);
            Assert.AreEqual(n, a.Count);
            return a;
        }

        [Test]
        public void Constructor() {
            CopyOnWriteArrayList<int> a = new CopyOnWriteArrayList<int>();
   
            Assert.IsTrue(a.IsEmpty);
        }

        [Test]
        public void Constructor2() {
            Int32[] ints = new Int32[DEFAULT_COLLECTION_SIZE];
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE - 1; ++i) {
                ints[i] = i;
            }

            CopyOnWriteArrayList<int> a = new CopyOnWriteArrayList<int>(ints);
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
                Assert.AreEqual(ints[i], a[i]);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorCollectionNull() {
            new CopyOnWriteArrayList<int>(null);
        }

        [Test]
        public void Constructor3() {
            Int32[] ints = new Int32[DEFAULT_COLLECTION_SIZE];
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE - 1; ++i) {
                ints[i] = i;
            }

            CopyOnWriteArrayList<int> a = new CopyOnWriteArrayList<int>(new List<int>(ints));
            for(int i = 0; i < DEFAULT_COLLECTION_SIZE; ++i) {
                Assert.AreEqual(ints[i], a[i]);
            }
        }

        [Test]
        public void AddAll() {
            CopyOnWriteArrayList<int> full = populatedArray(3);
            ICollection<int> v = new List<int>();
            v.Add(three);
            v.Add(four);
            v.Add(five);
            full.AddAll(v);
            Assert.AreEqual(6, full.Count);
        }

        [Test]
        public void AddAllAbsent() {
            CopyOnWriteArrayList<int> full = populatedArray(3);
            ICollection<int> v = new List<int>();
            v.Add(three);
            v.Add(four);
            v.Add(one); // will not add this element
            full.AddAllAbsent(v);
            Assert.AreEqual(5, full.Count);
        }

        [Test]
        public void AddIfAbsent() {
            CopyOnWriteArrayList<int> full = populatedArray(DEFAULT_COLLECTION_SIZE);
            full.AddIfAbsent(one);
            Assert.AreEqual(DEFAULT_COLLECTION_SIZE, full.Count);
        }

        [Test]
        public void AddIfAbsent2() {
            CopyOnWriteArrayList<int> full = populatedArray(DEFAULT_COLLECTION_SIZE);
            full.AddIfAbsent(three);
            Assert.IsTrue(full.Contains(three));
        }

        [Test]
        public void Clear() {
            CopyOnWriteArrayList<int> full = populatedArray(DEFAULT_COLLECTION_SIZE);
            full.Clear();
            Assert.AreEqual(0, full.Count);
        }

        [Test]
        public void Clone() {
            CopyOnWriteArrayList<int> l1 = populatedArray(DEFAULT_COLLECTION_SIZE);
            CopyOnWriteArrayList<int> l2 = (CopyOnWriteArrayList<int>)(l1.Clone());
            Assert.AreEqual(l1, l2);
            l1.Clear();
            Assert.IsFalse(l1.Equals(l2));
        }

        [Test]
        public void Contains() {
            CopyOnWriteArrayList<int> full = populatedArray(3);
            Assert.IsTrue(full.Contains(one));
            Assert.IsFalse(full.Contains(five));
        }

        [Test]
        public void AddIndex() {
            CopyOnWriteArrayList<int> full = populatedArray(3);
            full.Insert(0, m1);
            Assert.AreEqual(4, full.Count);
            Assert.AreEqual(m1, full[0]);
            Assert.AreEqual(zero, full[1]);

            full.Insert(2, m2);
            Assert.AreEqual(5, full.Count);
            Assert.AreEqual(m2, full[2]);
            Assert.AreEqual(two, full[4]);
        }

        [Test]
        public void Equals() {
            CopyOnWriteArrayList<int> a = populatedArray(3);
            CopyOnWriteArrayList<int> b = populatedArray(3);
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(b.Equals(a));
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            a.Add(m1);
            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(b.Equals(a));
            b.Add(m1);
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(b.Equals(a));
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void ContainsAll() {
            CopyOnWriteArrayList<int> full = populatedArray(3);
            ICollection<int> v = new List<int>();
            v.Add(one);
            v.Add(two);
            Assert.IsTrue(full.ContainsAll(v));
            v.Add(six);
            Assert.IsFalse(full.ContainsAll(v));
        }

        [Test]
        public void Get() {
            CopyOnWriteArrayList<int> full = populatedArray(3);
            Assert.AreEqual(0, full[0]);
        }

        [Test]
        public void IndexOf() {
            CopyOnWriteArrayList<int> full = populatedArray(3);
            Assert.AreEqual(1, full.IndexOf(one));
            Assert.AreEqual(-1, full.IndexOf(four));
        }

        [Test]
        public void IndexOf2() {
            CopyOnWriteArrayList<int> full = populatedArray(3);
            Assert.AreEqual(1, full.IndexOf(one, 0));
            Assert.AreEqual(-1, full.IndexOf(one, 2));
        }

        [Test]
        public void IsEmpty() {
            CopyOnWriteArrayList<int> empty = new CopyOnWriteArrayList<int>();
            CopyOnWriteArrayList<int> full = populatedArray(DEFAULT_COLLECTION_SIZE);
            Assert.IsTrue(empty.IsEmpty);
            Assert.IsFalse(full.IsEmpty);
        }

        [Test]
        public void Iterator() {
            CopyOnWriteArrayList<int> full = populatedArray(DEFAULT_COLLECTION_SIZE);
            IEnumerator i = full.GetEnumerator();
            int j;

            for(j = 0; i.MoveNext(); j++) {
                Assert.AreEqual(j, ((Int32)i.Current));
            }
            Assert.AreEqual(DEFAULT_COLLECTION_SIZE, j);
        }

        [Test]
        public void CopyOnWriteArrayListToString() {
            CopyOnWriteArrayList<int> full = populatedArray(3);
            String s = full.ToString();
            for(int i = 0; i < 3; ++i) {
                Assert.IsTrue(s.IndexOf(Convert.ToString(i)) >= 0);
            }
        }

        [Test]
        public void LastIndexOf1() {
            CopyOnWriteArrayList<int> full = populatedArray(3);
            full.Add(one);
            full.Add(three);
            Assert.AreEqual(3, full.LastIndexOf(one));
            Assert.AreEqual(-1, full.LastIndexOf(six));
        }

        [Test]
        public void lastIndexOf2() {
            CopyOnWriteArrayList<int> full = populatedArray(3);
            full.Add(one);
            full.Add(three);
            Assert.AreEqual(3, full.LastIndexOf(one, 4));
            Assert.AreEqual(-1, full.LastIndexOf(three, 3));
        }

        [Test]
        public void ListIterator1() {
            CopyOnWriteArrayList<int> full = populatedArray(DEFAULT_COLLECTION_SIZE);
            IEnumerator i = full.GetEnumerator();
            int j;

            for(j = 0; i.MoveNext(); j++) {
                Assert.AreEqual(j, ((Int32)i.Current));
            }
            Assert.AreEqual(DEFAULT_COLLECTION_SIZE, j);
        }

        [Test]
        public void Remove() {
            CopyOnWriteArrayList<int> full = populatedArray(3);
            full.Remove(2);
            Assert.AreEqual(2, full.Count);
        }

        [Test]
        public void RemoveAll() {
            CopyOnWriteArrayList<int> full = populatedArray(3);
            ICollection<int> v = new List<int>();
            v.Add(one);
            v.Add(two);
            full.RemoveAll(v);
            Assert.AreEqual(1, full.Count);
        }

        [Test]
        public void Set() {
            CopyOnWriteArrayList<int> full = populatedArray(3);
            full[2] = four;
            Assert.AreEqual(4, full[2]);
        }

        [Test]
        public void Size() {
            CopyOnWriteArrayList<int> empty = new CopyOnWriteArrayList<int>();
            CopyOnWriteArrayList<int> full = populatedArray(DEFAULT_COLLECTION_SIZE);
            Assert.AreEqual(DEFAULT_COLLECTION_SIZE, full.Count);
            Assert.AreEqual(0, empty.Count);
        }

        [Test]
        public void ToArray() {
            CopyOnWriteArrayList<int> full = populatedArray(3);
            int[] o = full.ToArray();
            Assert.AreEqual(3, o.Length);
            Assert.AreEqual(0, o[0]);
            Assert.AreEqual(1, o[1]);
            Assert.AreEqual(2, o[2]);
        }

        [Test]
        public void ToArray2() {
            CopyOnWriteArrayList<int> full = populatedArray(3);
            int[] i = new int[3];

            i = full.ToArray(i);
            Assert.AreEqual(3, i.Length);
            Assert.AreEqual(0, i[0]);
            Assert.AreEqual(1, i[1]);
            Assert.AreEqual(2, i[2]);
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Get1_IndexOutOfBoundsException() {
            CopyOnWriteArrayList<int> c = new CopyOnWriteArrayList<int>();
            Object generatedAux = c[-1];
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Get2_IndexOutOfBoundsException() {
            CopyOnWriteArrayList<string> c = new CopyOnWriteArrayList<string>();
            c.Add("asdasd");
            c.Add("asdad");
            Object generatedAux = c[100];
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Set1_IndexOutOfBoundsException() {
            CopyOnWriteArrayList<string> c = new CopyOnWriteArrayList<string>();
            c[-1] = "qwerty";
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Set2() {
            CopyOnWriteArrayList<string> c = new CopyOnWriteArrayList<string>();
            c.Add("asdasd");
            c.Add("asdad");
            c[100] = "qwerty";
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Add1_IndexOutOfBoundsException() {
            CopyOnWriteArrayList<string> c = new CopyOnWriteArrayList<string>();
            c.Insert(-1, "qwerty");
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Add2_IndexOutOfBoundsException() {
            CopyOnWriteArrayList<string> c = new CopyOnWriteArrayList<string>();
            c.Add("asdasd");
            c.Add("asdasdasd");
            c.Insert(100, "qwerty");
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Remove1_IndexOutOfBounds() {
            CopyOnWriteArrayList<string> c = new CopyOnWriteArrayList<string>();
            c.RemoveAt(-1);
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Remove2_IndexOutOfBounds() {
            CopyOnWriteArrayList<string> c = new CopyOnWriteArrayList<string>();
            c.Add("asdasd");
            c.Add("adasdasd");
            c.RemoveAt(100);
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void AddAll1_IndexOutOfBoundsException() {
            CopyOnWriteArrayList<string> c = new CopyOnWriteArrayList<string>();
            c.AddAll(-1, new List<string>());
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void AddAll2_IndexOutOfBoundsException() {
            CopyOnWriteArrayList<string> c = new CopyOnWriteArrayList<string>();
            c.Add("asdasd");
            c.Add("asdasdasd");
            c.AddAll(100, new List<string>());
        }

        [Test]
        public void Serialization() {
            CopyOnWriteArrayList<int> q = populatedArray(DEFAULT_COLLECTION_SIZE);
            MemoryStream bout = new MemoryStream(10000);

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(bout, q);

            MemoryStream bin = new MemoryStream(bout.ToArray());
            BinaryFormatter formatter2 = new BinaryFormatter();
            CopyOnWriteArrayList<int> r = (CopyOnWriteArrayList<int>)formatter2.Deserialize(bin);

            Assert.AreEqual(q.Count, r.Count);
            Assert.IsTrue(q.Equals(r));
            Assert.IsTrue(r.Equals(q));
        }
    }
}