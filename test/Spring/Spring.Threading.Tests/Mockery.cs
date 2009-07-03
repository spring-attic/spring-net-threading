using System;
using Rhino.Mocks;

namespace Spring
{
    class Mockery : MockRepository
    {
        public static T GeneratePartialMock<T>(params object[] argumentsForConstructor)
            where T : class
        {
            return MakeAaaMock(m=>m.PartialMock<T>(argumentsForConstructor));
        }

        public static T GenerateMultiPartialMock<T>(params Type[] extraTypes)
            where T : class
        {
            return MakeAaaMock(m => m.PartialMultiMock<T>(extraTypes));
        }

        public static T GenerateMultiPartialMock<T>(Type[] extraTypes, params object[] argumentsForConstructor)
            where T : class
        {
            return MakeAaaMock(m => m.PartialMultiMock<T>(extraTypes, argumentsForConstructor));
        }

        public static T GenerateMultiMock<T>(Type[] extraTypes, params object[] argumentForConstructor)
        {
            return MakeAaaMock(m=>m.DynamicMultiMock<T>(extraTypes, argumentForConstructor));
        }

        public static T GenerateMultiMock<T>(params Type[] extraTypes)
        {
            return MakeAaaMock(m => m.DynamicMultiMock<T>(extraTypes, new object[0]));
        }

        public static T StrickMock<T>(params object[] argumentForConstructor)
        {
            return MakeAaaMock(m => m.StrictMock<T>(argumentForConstructor));
        }

        private static T MakeAaaMock<T>(Converter<MockRepository, T> creator)
        {
            var mockery = new MockRepository();
            var mock = creator(mockery);
            mockery.Replay(mock);
            return mock;
        }

        public void a()
        {
            var s = "test string";
            var mock = StrickMock<System.Collections.Generic.IList<string>>();
            mock.Expect(l => l.Add(s));
            mock.Expect(l => l.Remove(s));

            mock.Add(s);
            mock.VerifyAllExpectations();
        }
    }
}