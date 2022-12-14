using AutoFixture.Kernel;
using Moq;

namespace Budgetty.TestHelpers
{
    internal class MockSynchronizerRelay : ISpecimenBuilder
    {
        private readonly IMockCreationSynchronizer _mockCreationSynchronizer;
        private readonly ISpecimenBuilder _nextRelay;

        public MockSynchronizerRelay(IMockCreationSynchronizer mockCreationSynchronizer, ISpecimenBuilder nextRelay)
        {
            _mockCreationSynchronizer = mockCreationSynchronizer;
            _nextRelay = nextRelay;
        }

        public object Create(object request, ISpecimenContext context)
        {
            var obj = _nextRelay.Create(request, context);

            if (request is Type type && (type.IsAbstract || type.IsInterface))
            {
                var mock = MockHelper.GetMockFromMockObject(obj, type);

                if (mock != null)
                {
                    _mockCreationSynchronizer.SynchronizeMock(type, mock);
                }
            }

            return obj;
        }
    }

    internal interface IMockCreationSynchronizer
    {
        void SynchronizeMock(Type type, Mock mock);
    }
}
