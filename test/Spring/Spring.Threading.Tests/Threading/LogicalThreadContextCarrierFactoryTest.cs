using NUnit.Framework;

namespace Spring.Threading
{
    /// <summary>
    /// Test cases for <see cref="LogicalThreadContextCarrierFactory"/>
    /// </summary>
    /// <author>Kenneth Xu</author>
    [TestFixture] public class LogicalThreadContextCarrierFactoryTest
    {
        TestThreadManager ThreadManager { get; set; }
        private LogicalThreadContextCarrierFactory _sut;
        const string SlotA = "A", SlotB = "B";
        private readonly object _valueA = new object(), _valueB = new object();

        [SetUp] public void SetUp()
        {
            _sut = new LogicalThreadContextCarrierFactory();
            ThreadManager = new TestThreadManager();
            LogicalThreadContext.FreeNamedDataSlot(SlotA);
            LogicalThreadContext.FreeNamedDataSlot(SlotB);
        }

        [Test] public void CreateContextCarrierCopiesSpecifiedContextToOtherThread()
        {
            _sut.Names = new[] {SlotA};
            Assert.That(_sut.Names, Is.Not.Null);
            Assert.That(LogicalThreadContext.GetData(SlotA), Is.Null);
            LogicalThreadContext.SetData(SlotA, _valueA);
            LogicalThreadContext.SetData(SlotB, _valueB);
            Assert.That(LogicalThreadContext.GetData(SlotA), Is.EqualTo(_valueA));
            Assert.That(LogicalThreadContext.GetData(SlotB), Is.EqualTo(_valueB));

            var carrier = _sut.CreateContextCarrier();

            ThreadManager.StartAndAssertRegistered("T",
                delegate
                    {
                        Assert.That(LogicalThreadContext.GetData(SlotA), Is.Null);
                        carrier.Restore();
                        Assert.That(LogicalThreadContext.GetData(SlotA), Is.EqualTo(_valueA));
                        Assert.That(LogicalThreadContext.GetData(SlotB), Is.Null);
                    });

            ThreadManager.JoinAndVerify();
        }

        [Test] public void CreateContextCarrierNopWhenNoDataSlotSpecified()
        {
            _sut.Names = null;
            Assert.That(_sut.Names, Is.Null);
            LogicalThreadContext.SetData(SlotA, _valueA);
            LogicalThreadContext.SetData(SlotB, _valueB);
            Assert.That(LogicalThreadContext.GetData(SlotA), Is.EqualTo(_valueA));
            Assert.That(LogicalThreadContext.GetData(SlotB), Is.EqualTo(_valueB));

            var carrier = _sut.CreateContextCarrier();

            ThreadManager.StartAndAssertRegistered("T",
                delegate
                    {
                        carrier.Restore();
                        Assert.That(LogicalThreadContext.GetData(SlotA), Is.Null);
                        Assert.That(LogicalThreadContext.GetData(SlotB), Is.Null);
                    });

            ThreadManager.JoinAndVerify();
        }

        [Test] public void CreateContextCarrierNopToSameThread()
        {
            _sut.Names = new[] {SlotB};
            Assert.That(LogicalThreadContext.GetData(SlotB), Is.Null);
            LogicalThreadContext.SetData(SlotB, _valueB);
            Assert.That(LogicalThreadContext.GetData(SlotB), Is.EqualTo(_valueB));

            var carrier = _sut.CreateContextCarrier();

            LogicalThreadContext.SetData(SlotB, _valueA);

            carrier.Restore();

            Assert.That(LogicalThreadContext.GetData(SlotB), Is.EqualTo(_valueA));
        }
    }
}
