using AutoFixture;
using Moq;
using ServForOracle.NetCore.Parameters;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class FunctionExtensionsTests
    {
        private void SetParamValues(IParam[] parameters, params object[] values)
        {
            if(parameters is null || parameters.Length != values.Length)
            {
                throw new ArgumentException("Invalid arguments");
            }

            for(int i = 0; i < parameters.Length; i++)
            {
                foreach (var prop in parameters[i].GetType().GetProperties()
                    .Where(p => p.Name == "Value"))
                {
                    prop.SetValue(parameters[i], values[i]);
                }
            }
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_OneRefParameter<T, A>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();

            var expectedA = fixture.Create<A>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A>(serviceMock.Object, function, ref valueA);

            Assert.Equal(expectedA, valueA);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_TwoRefParameter<T, A, B>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();

            var expectedA = fixture.Create<A>();
            var expectedB = fixture.Create<B>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, expectedB))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B>(serviceMock.Object, function, ref valueA, ref valueB);

            Assert.Equal(expectedA, valueA);
            Assert.Equal(expectedB, valueB);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_ThreeRefParameter<T, A, B, C>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();

            var expectedA = fixture.Create<A>();
            var expectedB = fixture.Create<B>();
            var expectedC = fixture.Create<C>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, expectedB, expectedC))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C>(serviceMock.Object, function, ref valueA, ref valueB, ref valueC);

            Assert.Equal(expectedA, valueA);
            Assert.Equal(expectedB, valueB);
            Assert.Equal(expectedC, valueC);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_FourRefParameter<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
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
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, expectedB, expectedC, expectedD))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C, D>(serviceMock.Object, function, ref valueA, ref valueB, ref valueC, ref valueD);

            Assert.Equal(expectedA, valueA);
            Assert.Equal(expectedB, valueB);
            Assert.Equal(expectedC, valueC);
            Assert.Equal(expectedD, valueD);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_OneParameter<T, A>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();

            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>()))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A>(serviceMock.Object, function, valueA);

            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_TwoParameter<T, A, B>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();

            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B>(serviceMock.Object, function, valueA, valueB);

            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_ThreeParameter<T, A, B, C>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();

            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C>(serviceMock.Object, function, valueA, valueB, valueC);

            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_FourParameter<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C, D>(serviceMock.Object, function, valueA, valueB, valueC, valueD);

            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_BRefAParameter<T, A, B>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();

            var expectedB = fixture.Create<B>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, expectedB))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B>(serviceMock.Object, function, valueA, ref valueB);

            Assert.Equal(valueB, expectedB);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_BRefACParameter<T, A, B, C>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();

            var expectedB = fixture.Create<B>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, expectedB, valueC))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C>(serviceMock.Object, function, valueA, ref valueB, valueC);

            Assert.Equal(valueB, expectedB);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_CRefABParameter<T, A, B, C>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();

            var expectedC = fixture.Create<C>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, valueB, expectedC))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C>(serviceMock.Object, function, valueA, valueB, ref valueC);

            Assert.Equal(valueC, expectedC);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_BCRefAParameter<T, A, B, C>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();

            var expectedB = fixture.Create<B>();
            var expectedC = fixture.Create<C>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, expectedB, expectedC))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C>(serviceMock.Object, function, valueA, ref valueB, ref valueC);

            Assert.Equal(valueB, expectedB);
            Assert.Equal(valueC, expectedC);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_ARefBCDParameter<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedA = fixture.Create<A>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, valueB, valueC, valueD))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C, D>(serviceMock.Object, function, ref valueA, valueB, valueC, valueD);

            Assert.Equal(valueA, expectedA);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_ABRefCDParameter<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedA = fixture.Create<A>();
            var expectedB = fixture.Create<B>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, expectedB, valueC, valueD))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C, D>(serviceMock.Object, function, ref valueA, ref valueB, valueC, valueD);

            Assert.Equal(valueA, expectedA);
            Assert.Equal(valueB, expectedB);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_ABCRefDParameter<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedA = fixture.Create<A>();
            var expectedB = fixture.Create<B>();
            var expectedC = fixture.Create<C>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, expectedB, expectedC, valueD))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C, D>(serviceMock.Object, function, ref valueA, ref valueB, ref valueC, valueD);

            Assert.Equal(valueA, expectedA);
            Assert.Equal(valueB, expectedB);
            Assert.Equal(valueC, expectedC);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_ABDRefCParameter<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedA = fixture.Create<A>();
            var expectedB = fixture.Create<B>();
            var expectedD = fixture.Create<D>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, expectedB, valueC, expectedD))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C, D>(serviceMock.Object, function, ref valueA, ref valueB, valueC, ref valueD);

            Assert.Equal(valueA, expectedA);
            Assert.Equal(valueB, expectedB);
            Assert.Equal(valueD, expectedD);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_ACRefBDParameter<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedA = fixture.Create<A>();
            var expectedC = fixture.Create<C>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, valueB, expectedC, valueD))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C, D>(serviceMock.Object, function, ref valueA, valueB, ref valueC, valueD);

            Assert.Equal(valueA, expectedA);
            Assert.Equal(valueC, expectedC);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_ACDRefBParameter<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedA = fixture.Create<A>();
            var expectedC = fixture.Create<C>();
            var expectedD = fixture.Create<D>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, valueB, expectedC, expectedD))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C, D>(serviceMock.Object, function, ref valueA, valueB, ref valueC, ref valueD);

            Assert.Equal(valueA, expectedA);
            Assert.Equal(valueC, expectedC);
            Assert.Equal(valueD, expectedD);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_ADRefBCParameter<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedA = fixture.Create<A>();
            var expectedD = fixture.Create<D>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, expectedA, valueB, valueC, expectedD))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C, D>(serviceMock.Object, function, ref valueA, valueB, valueC, ref valueD);

            Assert.Equal(valueA, expectedA);
            Assert.Equal(valueD, expectedD);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_BRefACDParameter<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedB = fixture.Create<B>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, expectedB, valueC, valueD))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C, D>(serviceMock.Object, function, valueA, ref valueB, valueC, valueD);

            Assert.Equal(valueB, expectedB);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_BCRefADParameter<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedB = fixture.Create<B>();
            var expectedC = fixture.Create<C>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, expectedB, expectedC, valueD))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C, D>(serviceMock.Object, function, valueA, ref valueB, ref valueC, valueD);

            Assert.Equal(expectedB, valueB);
            Assert.Equal(expectedC, valueC);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_BDRefACParameter<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedB = fixture.Create<B>();
            var expectedD = fixture.Create<D>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, expectedB, valueC, expectedD))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C, D>(serviceMock.Object, function, valueA, ref valueB, valueC, ref valueD);

            Assert.Equal(expectedB, valueB);
            Assert.Equal(expectedD, valueD);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_BCDRefAParameter<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedB = fixture.Create<B>();
            var expectedC = fixture.Create<C>();
            var expectedD = fixture.Create<D>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, expectedB, expectedC, expectedD))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C, D>(serviceMock.Object, function, valueA, ref valueB, ref valueC, ref valueD);

            Assert.Equal(expectedB, valueB);
            Assert.Equal(expectedC, valueC);
            Assert.Equal(expectedD, valueD);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_CRefABDParameter<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedC = fixture.Create<C>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, valueB, expectedC, valueD))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C, D>(serviceMock.Object, function, valueA, valueB, ref valueC, valueD);

            Assert.Equal(expectedC, valueC);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_CDRefABParameter<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedC = fixture.Create<C>();
            var expectedD = fixture.Create<D>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, valueB, expectedC, expectedD))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C, D>(serviceMock.Object, function, valueA, valueB, ref valueC, ref valueD);

            Assert.Equal(expectedC, valueC);
            Assert.Equal(expectedD, valueD);
            Assert.Equal(expectedValue, actual);
        }

        [Theory, CustomAutoData]
        public void ExecuteFunction_Works_DRefABCParameter<T, A, B, C, D>(Mock<IServiceForOracle> serviceMock, string function)
        {
            var fixture = new Fixture();
            var valueA = fixture.Create<A>();
            var valueB = fixture.Create<B>();
            var valueC = fixture.Create<C>();
            var valueD = fixture.Create<D>();

            var expectedD = fixture.Create<D>();
            var expectedValue = fixture.Create<T>();

            serviceMock.Setup(s => s.ExecuteFunction<T>(function, It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>(), It.IsAny<IParam>()))
                .Callback((string f, IParam[] p) => SetParamValues(p, valueA, valueB, valueC, expectedD))
                .Returns(expectedValue);

            var actual = FunctionExtensions.ExecuteFunction<T, A, B, C, D>(serviceMock.Object, function, valueA, valueB, valueC, ref valueD);

            Assert.Equal(expectedD, valueD);
            Assert.Equal(expectedValue, actual);
        }
    }
}
