
using NUnit.Framework;
using Rhino.Mocks;

public class BaseMockTestCase
{
    protected MockRepository _repository;

    [SetUp]
    public void Init()
    {
        _repository = new MockRepository();
    }

    [TearDown]
    public void Destory()
    {
        _repository.VerifyAll();
    }
}
