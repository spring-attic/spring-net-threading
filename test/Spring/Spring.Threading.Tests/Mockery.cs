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

        public static T GeneratePartialMultiMock<T>(params Type[] extraTypes)
            where T : class
        {
            return MakeAaaMock(m => m.PartialMultiMock<T>(extraTypes));
        }

        public static T GeneratePartialMultiMock<T>(Type[] extraTypes, params object[] argumentsForConstructor)
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

        public static T GenerateStrickMock<T>(params object[] argumentForConstructor)
        {
            return MakeAaaMock(m => m.StrictMock<T>(argumentForConstructor));
        }

        public static T GenerateStricMultikMock<T>(Type[] extraTypes, params object[] argumentForConstructor)
        {
            return MakeAaaMock(m => m.StrictMultiMock<T>(extraTypes, argumentForConstructor));
        }

        private static T MakeAaaMock<T>(Converter<MockRepository, T> creator)
        {
            var mockery = new MockRepository();
            var mock = creator(mockery);
            mockery.Replay(mock);
            return mock;
        }
    }
}