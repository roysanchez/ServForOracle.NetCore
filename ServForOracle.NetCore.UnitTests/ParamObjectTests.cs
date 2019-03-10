using Moq;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.OracleAbstracts;
using ServForOracle.NetCore.Parameters;
using ServForOracle.NetCore.UnitTests.Config;
using ServForOracle.NetCore.Wrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class ParamObjectTests
    {
        public class TestClass
        {

        }

        [Theory, CustomAutoData]
        public void Constructor_TwoParameters(TestClass model, ParameterDirection direction)
        {
            var param = new ParamObject<TestClass>(model, direction);

            Assert.Equal(model, param.Value);
            Assert.Equal(direction, param.Direction);
        }

        [Theory, CustomAutoData]
        public void Constructor_ThreeParameters(TestClass model, ParameterDirection direction, OracleUdtInfo info)
        {
            var param = new ParamObject<TestClass>(model, direction, info);

            Assert.Equal(model, param.Value);
            Assert.Equal(direction, param.Direction);
        }

        [Theory, CustomAutoData]
        public void AbstractBase_PropertiesSet(TestClass model, ParameterDirection direction, OracleUdtInfo info)
        {
            var param = new ParamObject<TestClass>(model, direction, info);

            ParamObject paramObjBase = param;

            Assert.NotNull(paramObjBase);
            Assert.Equal(info, paramObjBase.UDTInfo);
        }

        [Theory, CustomAutoData]
        public void Constructor_ThreeParameters_UdtNull_ThrowsArgumentNull (TestClass model, ParameterDirection direction)
        {
            Assert.Throws<ArgumentNullException>("udtInfo", () => new ParamObject<TestClass>(model, direction, null));
        }

        [Theory, CustomAutoData]
        public void Constructor_ThreeParameters_CollectionType_InvalidUdt_ThrowsArgument(TestClass[] model, ParameterDirection direction, string schema, string objName)
        {
            var info = new OracleUdtInfo(schema, objName);
            var message = $"For the type {typeof(TestClass[]).FullName} array you must especify the UDT collection name" + Environment.NewLine + "Parameter name: udtInfo";

            var exception = Assert.Throws<ArgumentException>("udtInfo", () => new ParamObject<TestClass[]>(model, direction, info));

            Assert.Equal(message, exception.Message);
        }

        [Theory, CustomAutoData]
        internal void LoadObjectMetadata_Works(Mock<MetadataBuilder> builder, TestClass model, ParameterDirection direction, OracleUdtInfo info, MetadataOracleObject<TestClass> metadataObject)
        {
            builder.Setup(b => b.GetOrRegisterMetadataOracleObject<TestClass>(info))
                .Returns(metadataObject)
                .Verifiable();


            var param = new ParamObject<TestClass>(model, direction, info);

            param.LoadObjectMetadata(builder.Object);

            builder.Verify();
            Assert.True(param.MetadataLoaded);
        }

        [Theory, CustomAutoData]
        internal async Task LoadObjectMetadataAsync_Works(Mock<MetadataBuilder> builder, TestClass model, ParameterDirection direction, OracleUdtInfo info, MetadataOracleObject<TestClass> metadataObject)
        {
            builder.Setup(b => b.GetOrRegisterMetadataOracleObjectAsync<TestClass>(info))
                .ReturnsAsync(metadataObject)
                .Verifiable();


            var param = new ParamObject<TestClass>(model, direction, info);

            await param.LoadObjectMetadataAsync(builder.Object);

            builder.Verify();
            Assert.True(param.MetadataLoaded);
        }

        [Theory, CustomAutoData]
        internal void SetParameterName_Works(Mock<MetadataBuilder> builder, TestClass model, ParameterDirection direction, OracleUdtInfo info, Mock<MetadataOracleObject<TestClass>> metadataObject, string name, string line)
        {
            builder.Setup(b => b.GetOrRegisterMetadataOracleObject<TestClass>(info))
                .Returns(metadataObject.Object);

            metadataObject.Setup(b => b.GetDeclareLine(typeof(TestClass), name, metadataObject.Object.OracleTypeNetMetadata.UDTInfo))
                .Returns(line)
                .Verifiable();


            var param = new ParamObject<TestClass>(model, direction, info);

            param.LoadObjectMetadata(builder.Object);

            param.SetParameterName(name);

            var result = param.GetDeclareLine();

            metadataObject.Verify();
        }

        [Theory, CustomAutoData]
        internal void GetDeclareLine_Works(Mock<MetadataBuilder> builder, TestClass model, ParameterDirection direction, OracleUdtInfo info, Mock<MetadataOracleObject<TestClass>> metadataObject, string name, string line)
        {
            builder.Setup(b => b.GetOrRegisterMetadataOracleObject<TestClass>(info))
                .Returns(metadataObject.Object);

            metadataObject.Setup(b => b.GetDeclareLine(typeof(TestClass), name, metadataObject.Object.OracleTypeNetMetadata.UDTInfo))
                .Returns(line)
                .Verifiable();


            var param = new ParamObject<TestClass>(model, direction, info);

            param.LoadObjectMetadata(builder.Object);

            param.SetParameterName(name);

            var result = param.GetDeclareLine();

            metadataObject.Verify();
            Assert.Equal(line, result);
        }

        [Theory, CustomAutoData]
        internal async Task SetOutputValueAsync_Works(Mock<MetadataBuilder> builder, TestClass model, ParameterDirection direction, OracleUdtInfo info, Mock<MetadataOracleObject<TestClass>> metadataObject)
        {
            var refCursor = FormatterServices.GetUninitializedObject(
            typeof(OracleRefCursor));

            builder.Setup(b => b.GetOrRegisterMetadataOracleObject<TestClass>(info))
                .Returns(metadataObject.Object);

            metadataObject.Setup(b => b.GetValueFromRefCursorAsync(typeof(TestClass), It.IsAny<OracleRefCursorWrapper>()))
                .ReturnsAsync(model)
                .Verifiable();


            var param = new ParamObject<TestClass>(model, direction, info);

            param.LoadObjectMetadata(builder.Object);

            await param.SetOutputValueAsync(refCursor);

            metadataObject.Verify();
            Assert.Equal(model, param.Value);
        }

        [Theory, CustomAutoData]
        internal void SetOutputValue_Works(Mock<MetadataBuilder> builder, TestClass model, ParameterDirection direction, OracleUdtInfo info, Mock<MetadataOracleObject<TestClass>> metadataObject)
        {
            var refCursor = FormatterServices.GetUninitializedObject(
            typeof(OracleRefCursor));

            builder.Setup(b => b.GetOrRegisterMetadataOracleObject<TestClass>(info))
                .Returns(metadataObject.Object);

            metadataObject.Setup(b => b.GetValueFromRefCursor(typeof(TestClass), It.IsAny<OracleRefCursorWrapper>()))
                .Returns(model)
                .Verifiable();


            var param = new ParamObject<TestClass>(model, direction, info);

            param.LoadObjectMetadata(builder.Object);

            param.SetOutputValue(refCursor);

            metadataObject.Verify();
            Assert.Equal(model, param.Value);
        }

        [Theory, CustomAutoData]
        internal void GetOracleParameters_Works(Mock<MetadataBuilder> builder, TestClass model, ParameterDirection direction, OracleUdtInfo info, Mock<MetadataOracleObject<TestClass>> metadataObject, OracleParameter[] parameters, int startNumber)
        {
            builder.Setup(b => b.GetOrRegisterMetadataOracleObject<TestClass>(info))
                .Returns(metadataObject.Object);

            metadataObject.Setup(b => b.GetOracleParameters(model, startNumber))
                .Returns(parameters)
                .Verifiable();

            var param = new ParamObject<TestClass>(model, direction, info);

            param.LoadObjectMetadata(builder.Object);

            var actual = param.GetOracleParameters(startNumber);

            metadataObject.Verify();
            Assert.Equal(parameters, actual);
        }

        [Theory, CustomAutoData]
        internal void BuildQueryConstructorString_Works(Mock<MetadataBuilder> builder, TestClass model, ParameterDirection direction, OracleUdtInfo info, Mock<MetadataOracleObject<TestClass>> metadataObject, int lastNumber, string constructor, string name, int startNumber)
        {
            builder.Setup(b => b.GetOrRegisterMetadataOracleObject<TestClass>(info))
                .Returns(metadataObject.Object);

            metadataObject.Setup(b => b.BuildQueryConstructorString(model, name, startNumber))
                .Returns((constructor, lastNumber))
                .Verifiable();

            var param = new ParamObject<TestClass>(model, direction, info);

            param.LoadObjectMetadata(builder.Object);
            param.SetParameterName(name);

            var actual = param.BuildQueryConstructorString(startNumber);

            metadataObject.Verify();
            Assert.Equal((constructor, lastNumber), actual);
        }

        [Theory, CustomAutoData]
        internal void PrepareOutputParameter_Works(Mock<MetadataBuilder> builder, TestClass model, ParameterDirection direction, OracleUdtInfo info, Mock<MetadataOracleObject<TestClass>> metadataObject, int startNumber, string query, OracleParameter oracleParameter)
        {
            builder.Setup(b => b.GetOrRegisterMetadataOracleObject<TestClass>(info))
                .Returns(metadataObject.Object);

            metadataObject.Setup(m => m.GetRefCursorQuery(startNumber, null))
                .Returns(query)
                .Verifiable();

            metadataObject.Setup(b => b.GetOracleParameterForRefCursor(startNumber))
                .Returns(oracleParameter)
                .Verifiable();

            var param = new ParamObject<TestClass>(model, direction, info);

            param.LoadObjectMetadata(builder.Object);

            var actual = param.PrepareOutputParameter(startNumber);

            metadataObject.Verify();
            Assert.NotNull(actual);
            Assert.Equal(param, actual.Parameter);
            Assert.Equal(query, actual.OutputString);
            Assert.Equal(oracleParameter, actual.OracleParameter);
        }
    }
}
