using System;
using System.Reflection;
using NUnit.CommonFixtures.Threading;
using NUnit.Framework;

namespace Spring.Threading
{
    /// <summary>
    /// Test cases for <see cref="LogicalThreadContextCarrierFactory"/>
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture] public class LogicalThreadContextCarrierFactoryTest
    {
        private static readonly Func<string, object> _getData;
        private static readonly Action<string, object> _setData;
        private static readonly Action<string> _freeNamedDataSlot;

        static LogicalThreadContextCarrierFactoryTest()
        {
            Assembly.LoadFrom("Spring.Core.dll");
            var type = Type.GetType("Spring.Threading.LogicalThreadContext, Spring.Core");
            var getDataMethod = type.GetMethod("GetData", BindingFlags.Static | BindingFlags.Public);
            _getData = (Func<string, object>)Delegate.CreateDelegate(typeof(Func<string, object>), getDataMethod);
            var setDataMethod = type.GetMethod("SetData", BindingFlags.Static | BindingFlags.Public);
            _setData = (Action<string, object>)Delegate.CreateDelegate(typeof(Action<string, object>), setDataMethod);
            var freeNamedDataSlot = type.GetMethod("FreeNamedDataSlot", BindingFlags.Static | BindingFlags.Public);
            _freeNamedDataSlot = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), freeNamedDataSlot);
        }

        TestThreadManager ThreadManager { get; set; }
        private LogicalThreadContextCarrierFactory _sut;
        const string SlotA = "A", SlotB = "B";
        private readonly object _valueA = new object(), _valueB = new object();

        [SetUp] public void SetUp()
        {
            _sut = new LogicalThreadContextCarrierFactory();
            ThreadManager = new TestThreadManager();
            _freeNamedDataSlot(SlotA);
            _freeNamedDataSlot(SlotB);
            //LogicalThreadContext.FreeNamedDataSlot(SlotA);
            //LogicalThreadContext.FreeNamedDataSlot(SlotB);
        }

        [Test] public void CreateContextCarrierCopiesSpecifiedContextToOtherThread()
        {
            _sut.Names = new[] {SlotA};
            Assert.That(_sut.Names, Is.Not.Null);
            Assert.That(_getData(SlotA), Is.Null);
            _setData(SlotA, _valueA);
            _setData(SlotB, _valueB);
            Assert.That(_getData(SlotA), Is.EqualTo(_valueA));
            Assert.That(_getData(SlotB), Is.EqualTo(_valueB));

            var carrier = _sut.CreateContextCarrier();

            ThreadManager.StartAndAssertRegistered("T",
                delegate
                    {
                        Assert.That(_getData(SlotA), Is.Null);
                        carrier.Restore();
                        Assert.That(_getData(SlotA), Is.EqualTo(_valueA));
                        Assert.That(_getData(SlotB), Is.Null);
                    });

            ThreadManager.JoinAndVerify();
        }

        [Test] public void CreateContextCarrierNopWhenNoDataSlotSpecified()
        {
            _sut.Names = null;
            Assert.That(_sut.Names, Is.Null);
            _setData(SlotA, _valueA);
            _setData(SlotB, _valueB);
            Assert.That(_getData(SlotA), Is.EqualTo(_valueA));
            Assert.That(_getData(SlotB), Is.EqualTo(_valueB));

            var carrier = _sut.CreateContextCarrier();

            ThreadManager.StartAndAssertRegistered("T",
                delegate
                    {
                        carrier.Restore();
                        Assert.That(_getData(SlotA), Is.Null);
                        Assert.That(_getData(SlotB), Is.Null);
                    });

            ThreadManager.JoinAndVerify();
        }

        [Test] public void CreateContextCarrierNopToSameThread()
        {
            _sut.Names = new[] {SlotB};
            Assert.That(_getData(SlotB), Is.Null);
            _setData(SlotB, _valueB);
            Assert.That(_getData(SlotB), Is.EqualTo(_valueB));

            var carrier = _sut.CreateContextCarrier();

            _setData(SlotB, _valueA);

            carrier.Restore();

            Assert.That(_getData(SlotB), Is.EqualTo(_valueA));
        }
    }
}
