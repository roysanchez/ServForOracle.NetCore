using AutoFixture;
using Moq;
using ServForOracle.NetCore.Parameters;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class FunctionAsyncExtensionsTests
    {
        [Theory, CustomAutoData]
        public async Task ExecuteFunctionAsync_Works_OneParameter<T, A>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var value = fixture.Create<A>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunctionAsync<T>(function, It.IsAny<IParam>()))
                .ReturnsAsync(expectedValue);

            var actual = await FunctionAsyncExtensions.ExecuteFunctionAsync<T,A>(serviceMock.Object, function, value);

            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public async Task ExecuteFunctionAsync_Works_TwoParameters<T, A, B>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunctionAsync<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>()))
                .ReturnsAsync(expectedValue);

            var actual = await FunctionAsyncExtensions.ExecuteFunctionAsync<T, A, B>(serviceMock.Object, function, valueA, valueB);

            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public async Task ExecuteFunctionAsync_Works_ThreeParameters<T, A, B, C>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunctionAsync<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .ReturnsAsync(expectedValue);

            var actual = await FunctionAsyncExtensions.ExecuteFunctionAsync<T, A, B, C>(serviceMock.Object, function, valueA, valueB, valueC);

            Assert.Equal(expectedValue, actual);
        }
        [Theory, CustomAutoData]
        public async Task ExecuteFunctionAsync_Works_FourParameters<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunctionAsync<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .ReturnsAsync(expectedValue);

            var actual = await FunctionAsyncExtensions.ExecuteFunctionAsync<T, A, B, C, D>(serviceMock.Object, function, valueA, valueB, valueC, valueD);

            Assert.Equal(expectedValue, actual);
        }
    }
}
