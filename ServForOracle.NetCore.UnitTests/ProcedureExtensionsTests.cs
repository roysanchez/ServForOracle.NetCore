using AutoFixture;
using Moq;
using ServForOracle.NetCore.Parameters;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class ProcedureExtensionsTests
    {
        private void SetParamValues(IParam[] parameters, params object[] values)
        {
            if (parameters is null || parameters.Length != values.Length)
            {
                throw new ArgumentException("Invalid arguments");
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                foreach (var prop in parameters[i].GetType().GetProperties()
                    .Where(p => p.Name == "Value"))
                {
                    prop.SetValue(parameters[i], values[i]);
                }
            }
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_OneRefParameter<A>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();

            var expectedA = fixture.Create<A>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>()))
                .Callback((string pro, IParam[] p) => SetParamValues(p, expectedA));

            ProcedureExtensions.ExecuteProcedure<A>(serviceMock.Object, procedure, ref valueA);

            Assert.Equal(expectedA, valueA);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_TwoRefParameter<A, B>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();

            var expectedA = fixture.Create<A>();
            var expectedB = fixture.Create<B>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, expectedB));

            ProcedureExtensions.ExecuteProcedure<A, B>(serviceMock.Object, procedure, ref valueA, ref valueB);

            Assert.Equal(expectedA, valueA);
            Assert.Equal(expectedB, valueB);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_ThreeRefParameter<A, B, C>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();

            var expectedA = fixture.Create<A>();
            var expectedB = fixture.Create<B>();
            var expectedC = fixture.Create<C>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, expectedB, expectedC));

            ProcedureExtensions.ExecuteProcedure<A, B, C>(serviceMock.Object, procedure, ref valueA, ref valueB, ref valueC);

            Assert.Equal(expectedA, valueA);
            Assert.Equal(expectedB, valueB);
            Assert.Equal(expectedC, valueC);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_FourRefParameter<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedA = fixture.Create<A>();
            var expectedB = fixture.Create<B>();
            var expectedC = fixture.Create<C>();
            var expectedD = fixture.Create<D>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, expectedB, expectedC, expectedD));

            ProcedureExtensions.ExecuteProcedure<A, B, C, D>(serviceMock.Object, procedure, ref valueA, ref valueB, ref valueC, ref valueD);

            Assert.Equal(expectedA, valueA);
            Assert.Equal(expectedB, valueB);
            Assert.Equal(expectedC, valueC);
            Assert.Equal(expectedD, valueD);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_OneParameter<A>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>())).Verifiable();

            ProcedureExtensions.ExecuteProcedure<A>(serviceMock.Object, procedure, valueA);

            serviceMock.Verify();
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_TwoParameter<A, B>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>())).Verifiable();

            ProcedureExtensions.ExecuteProcedure<A, B>(serviceMock.Object, procedure, valueA, valueB);

            serviceMock.Verify();
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_ThreeParameter<A, B, C>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()));

            ProcedureExtensions.ExecuteProcedure<A, B, C>(serviceMock.Object, procedure, valueA, valueB, valueC);

            serviceMock.Verify();
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_FourParameter<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Verifiable();

            ProcedureExtensions.ExecuteProcedure<A, B, C, D>(serviceMock.Object, procedure, valueA, valueB, valueC, valueD);

            serviceMock.Verify();
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_BRefAParameter<A, B>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();

            var expectedB = fixture.Create<B>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, expectedB));

            ProcedureExtensions.ExecuteProcedure<A, B>(serviceMock.Object, procedure, valueA, ref valueB);

            Assert.Equal(valueB, expectedB);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_BRefACParameter<A, B, C>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();

            var expectedB = fixture.Create<B>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, expectedB, valueC));

            ProcedureExtensions.ExecuteProcedure<A, B, C>(serviceMock.Object, procedure, valueA, ref valueB, valueC);

            Assert.Equal(valueB, expectedB);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_CRefABParameter<A, B, C>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();

            var expectedC = fixture.Create<C>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, valueB, expectedC));

            ProcedureExtensions.ExecuteProcedure<A, B, C>(serviceMock.Object, procedure, valueA, valueB, ref valueC);

            Assert.Equal(valueC, expectedC);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_BCRefAParameter<A, B, C>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();

            var expectedB = fixture.Create<B>();
            var expectedC = fixture.Create<C>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, expectedB, expectedC));

            ProcedureExtensions.ExecuteProcedure<A, B, C>(serviceMock.Object, procedure, valueA, ref valueB, ref valueC);

            Assert.Equal(valueB, expectedB);
            Assert.Equal(valueC, expectedC);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_ARefBCDParameter<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedA = fixture.Create<A>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, valueB, valueC, valueD));

            ProcedureExtensions.ExecuteProcedure<A, B, C, D>(serviceMock.Object, procedure, ref valueA, valueB, valueC, valueD);

            Assert.Equal(valueA, expectedA);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_ABRefCDParameter<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedA = fixture.Create<A>();
            var expectedB = fixture.Create<B>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, expectedB, valueC, valueD));

            ProcedureExtensions.ExecuteProcedure<A, B, C, D>(serviceMock.Object, procedure, ref valueA, ref valueB, valueC, valueD);

            Assert.Equal(valueA, expectedA);
            Assert.Equal(valueB, expectedB);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_ABCRefDParameter<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedA = fixture.Create<A>();
            var expectedB = fixture.Create<B>();
            var expectedC = fixture.Create<C>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, expectedB, expectedC, valueD));

            ProcedureExtensions.ExecuteProcedure<A, B, C, D>(serviceMock.Object, procedure, ref valueA, ref valueB, ref valueC, valueD);

            Assert.Equal(valueA, expectedA);
            Assert.Equal(valueB, expectedB);
            Assert.Equal(valueC, expectedC);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_ABDRefCParameter<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedA = fixture.Create<A>();
            var expectedB = fixture.Create<B>();
            var expectedD = fixture.Create<D>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, expectedB, valueC, expectedD));

            ProcedureExtensions.ExecuteProcedure<A, B, C, D>(serviceMock.Object, procedure, ref valueA, ref valueB, valueC, ref valueD);

            Assert.Equal(valueA, expectedA);
            Assert.Equal(valueB, expectedB);
            Assert.Equal(valueD, expectedD);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_ACRefBDParameter<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedA = fixture.Create<A>();
            var expectedC = fixture.Create<C>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, valueB, expectedC, valueD));

            ProcedureExtensions.ExecuteProcedure<A, B, C, D>(serviceMock.Object, procedure, ref valueA, valueB, ref valueC, valueD);

            Assert.Equal(valueA, expectedA);
            Assert.Equal(valueC, expectedC);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_ACDRefBParameter<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedA = fixture.Create<A>();
            var expectedC = fixture.Create<C>();
            var expectedD = fixture.Create<D>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, valueB, expectedC, expectedD));

            ProcedureExtensions.ExecuteProcedure<A, B, C, D>(serviceMock.Object, procedure, ref valueA, valueB, ref valueC, ref valueD);

            Assert.Equal(valueA, expectedA);
            Assert.Equal(valueC, expectedC);
            Assert.Equal(valueD, expectedD);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_ADRefBCParameter<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedA = fixture.Create<A>();
            var expectedD = fixture.Create<D>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, valueB, valueC, expectedD));

            ProcedureExtensions.ExecuteProcedure<A, B, C, D>(serviceMock.Object, procedure, ref valueA, valueB, valueC, ref valueD);

            Assert.Equal(valueA, expectedA);
            Assert.Equal(valueD, expectedD);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_BRefACDParameter<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedB = fixture.Create<B>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, expectedB, valueC, valueD));

            ProcedureExtensions.ExecuteProcedure<A, B, C, D>(serviceMock.Object, procedure, valueA, ref valueB, valueC, valueD);

            Assert.Equal(valueB, expectedB);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_BCRefADParameter<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string procedure)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedB = fixture.Create<B>();
            var expectedC = fixture.Create<C>();

            serviceMock.Setup(s => s.ExecuteProcedure(procedure, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, expectedB, expectedC, valueD));

            ProcedureExtensions.ExecuteProcedure<A, B, C, D>(serviceMock.Object, procedure, valueA, ref valueB, ref valueC, valueD);

            Assert.Equal(expectedB, valueB);
            Assert.Equal(expectedC, valueC);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_BDRefACParameter<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedB = fixture.Create<B>();
            var expectedD = fixture.Create<D>();

            serviceMock.Setup(s => s.ExecuteProcedure(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, expectedB, valueC, expectedD));

            ProcedureExtensions.ExecuteProcedure<A, B, C, D>(serviceMock.Object, function, valueA, ref valueB, valueC, ref valueD);

            Assert.Equal(expectedB, valueB);
            Assert.Equal(expectedD, valueD);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_BCDRefAParameter<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedB = fixture.Create<B>();
            var expectedC = fixture.Create<C>();
            var expectedD = fixture.Create<D>();

            serviceMock.Setup(s => s.ExecuteProcedure(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, expectedB, expectedC, expectedD));

            ProcedureExtensions.ExecuteProcedure<A, B, C, D>(serviceMock.Object, function, valueA, ref valueB, ref valueC, ref valueD);

            Assert.Equal(expectedB, valueB);
            Assert.Equal(expectedC, valueC);
            Assert.Equal(expectedD, valueD);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_CRefABDParameter<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedC = fixture.Create<C>();

            serviceMock.Setup(s => s.ExecuteProcedure(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, valueB, expectedC, valueD));

            ProcedureExtensions.ExecuteProcedure<A, B, C, D>(serviceMock.Object, function, valueA, valueB, ref valueC, valueD);

            Assert.Equal(expectedC, valueC);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_CDRefABParameter<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedC = fixture.Create<C>();
            var expectedD = fixture.Create<D>();

            serviceMock.Setup(s => s.ExecuteProcedure(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, valueB, expectedC, expectedD));

            ProcedureExtensions.ExecuteProcedure<A, B, C, D>(serviceMock.Object, function, valueA, valueB, ref valueC, ref valueD);

            Assert.Equal(expectedC, valueC);
            Assert.Equal(expectedD, valueD);
        }

        [Theory, CustomAutoData]
        public void ExecuteProcedure_DRefABCParameter<A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedD = fixture.Create<D>();

            serviceMock.Setup(s => s.ExecuteProcedure(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, valueB, valueC, expectedD));

            ProcedureExtensions.ExecuteProcedure<A, B, C, D>(serviceMock.Object, function, valueA, valueB, valueC, ref valueD);

            Assert.Equal(expectedD, valueD);
        }
    }
}
