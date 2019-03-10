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
    public class ProcedureAsyncExtensionsTests
    {
        [Theory, CustomAutoData]
        public async Task ExecuteProcedureAsync_OneParameter<A>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var value = fixture.Create<A>();

            serviceMock.Setup(s => s.ExecuteProcedureAsync(procedure, It.IsAny<IParam>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await ProcedureAsyncExtensions.ExecuteProcedureAsync<A>(serviceMock.Object, procedure, value);

            serviceMock.Verify();
        }

        [Theory, CustomAutoData]
        public async Task ExecuteProcedureAsync_TwoParameters<A, B>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();

            serviceMock.Setup(s => s.ExecuteProcedureAsync(procedure, It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await ProcedureAsyncExtensions.ExecuteProcedureAsync<A, B>(serviceMock.Object, procedure, valueA, valueB);

            serviceMock.Verify();
        }

        [Theory, CustomAutoData]
        public async Task ExecuteProcedureAsync_ThreeParameters<A, B, C>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();

            serviceMock.Setup(s => s.ExecuteProcedureAsync(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await ProcedureAsyncExtensions.ExecuteProcedureAsync<A, B, C>(serviceMock.Object, procedure, valueA, valueB, valueC);

            serviceMock.Verify();
        }
        [Theory, CustomAutoData]
        public async Task ExecuteProcedureAsync_FourParameters<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            serviceMock.Setup(s => s.ExecuteProcedureAsync(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await ProcedureAsyncExtensions.ExecuteProcedureAsync<A, B, C, D>(serviceMock.Object, function, valueA, valueB, valueC, valueD);

            serviceMock.Verify();
        }
    }
}
