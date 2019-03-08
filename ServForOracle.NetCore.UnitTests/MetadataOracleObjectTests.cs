using ServForOracle.NetCore.Cache;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using ServForOracle.NetCore.Extensions;
using System.Data;
using System.Threading.Tasks;
using ServForOracle.NetCore.Wrapper;
using Moq;

namespace ServForOracle.NetCore.UnitTests
{
    public class MetadataOracleObjectTests
    {
        #region Test classes

        public class TestClass
        {
        }

        public class SimpleTestClass
        {
            public string Prop1 { get; set; }
        }

        public class ArrayTestClass
        {
            public string[] Prop1 { get; set; }
        }

        public class ComplexTestClass
        {
            public SimpleTestClass ObjectProp { get; set; }
        }

        public class MultiplePropertiesTestClass
        {
            public string Prop1 { get; set; }
            public string Prop2 { get; set; }
        }

        public class CollectionPropertyTestClass
        {
            //The library does not support arrays of native types yet.
            //public string[] Prop1 { get; set; }
            public SimpleTestClass[] Prop2 { get; set; }
        }

        #endregion Test classes

        private void CompareOracleTypeNetMetadata(MetadataOracleTypePropertyDefinition[] expected, MetadataOraclePropertyNetTypeDefinition[] actual)
        {
            Assert.NotEmpty(actual);
            Assert.Collection(actual,
                c =>
                {
                    Assert.NotNull(c);
                    Assert.Equal(c.Name, expected[0].Name, ignoreCase: true);
                    Assert.Equal(c.Order, expected[0].Order);
                },
                c =>
                {
                    Assert.NotNull(c);
                    Assert.Equal(c.Name, expected[1].Name, ignoreCase: true);
                    Assert.Equal(c.Order, expected[1].Order);
                },
                c =>
                {
                    Assert.NotNull(c);
                    Assert.Equal(c.Name, expected[2].Name, ignoreCase: true);
                    Assert.Equal(c.Order, expected[2].Order);
                }
              );
        }

        #region Constructor

        [Fact]
        internal void MetadataOracleObject_Constructor_NullParameter_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("oracleNetTypeDefinition", () => new MetadataOracleObject<TestClass>(null, null));
        }

        [Theory, CustomAutoData]
        internal void MetadataOracleObject_Constructor_Object(ServForOracleCache cache, MetadataOracleTypeDefinition metadataOracleType, UdtPropertyNetPropertyMap[] customProperties, bool fuzzyNameMatch)
        {
            var typedef = new MetadataOracleNetTypeDefinition(cache, typeof(TestClass), metadataOracleType, customProperties, fuzzyNameMatch);
            var metadata = new MetadataOracleObject<TestClass>(typedef, new MetadataOracleCommon());

            Assert.NotNull(metadata);
            Assert.NotNull(metadata.OracleTypeNetMetadata);
            Assert.NotNull(metadata.OracleTypeNetMetadata.Properties);
            CompareOracleTypeNetMetadata(metadataOracleType.Properties.ToArray(), metadata.OracleTypeNetMetadata.Properties.ToArray());
            Assert.Equal(metadataOracleType.UDTInfo, metadata.OracleTypeNetMetadata.UDTInfo);
        }

        [Theory, CustomAutoData]
        internal void MetadataOracleObject_Constructor_Collection(ServForOracleCache cache, MetadataOracleTypeDefinition metadataOracleType, UdtPropertyNetPropertyMap[] customProperties, bool fuzzyNameMatch)
        {
            var typedef = new MetadataOracleNetTypeDefinition(cache, typeof(TestClass[]).GetCollectionUnderType(), metadataOracleType, customProperties, fuzzyNameMatch);
            var metadata = new MetadataOracleObject<TestClass[]>(typedef, new MetadataOracleCommon());

            Assert.NotNull(metadata);
            Assert.NotNull(metadata.OracleTypeNetMetadata);
            Assert.NotNull(metadata.OracleTypeNetMetadata.Properties);
            CompareOracleTypeNetMetadata(metadataOracleType.Properties.ToArray(), metadata.OracleTypeNetMetadata.Properties.ToArray());
            Assert.Equal(metadataOracleType.UDTInfo, metadata.OracleTypeNetMetadata.UDTInfo);
        }

        #endregion Constructor

        #region BuildQueryConstructorString

        [Theory, CustomAutoData]
        internal void BuildQueryConstructorString_Object_NoMatch_AllPropertiesNull(ServForOracleCache cache, MetadataOracleTypeDefinition metadataOracleType, UdtPropertyNetPropertyMap[] customProperties, bool fuzzyNameMatch, TestClass model, string name, int startNumber)
        {
            var typedef = new MetadataOracleNetTypeDefinition(cache, typeof(TestClass), metadataOracleType, customProperties, fuzzyNameMatch);
            var metadata = new MetadataOracleObject<TestClass>(typedef, new MetadataOracleCommon());

            var (constructor, lastNumber) = metadata.BuildQueryConstructorString(model, name, startNumber);

            Assert.NotNull(constructor);
            var expectedConstructor = $"{name} := {metadataOracleType.UDTInfo.FullObjectName}({string.Join(',', metadataOracleType.Properties.OrderBy(c => c.Order).Select(c => $"{c.Name}=>null"))});" + Environment.NewLine;
            Assert.Equal(expectedConstructor, constructor);
            Assert.Equal(startNumber, lastNumber);
        }

        [Theory, CustomAutoData]
        internal void BuildQueryConstructorString_Object_SimpleNetProperty(ServForOracleCache cache, MetadataOracleTypeDefinition metadataOracleType, UdtPropertyNetPropertyMap[] customProperties, bool fuzzyNameMatch, SimpleTestClass model, string name, int startNumber)
        {
            var prop = metadataOracleType.Properties.OrderBy(c => c.Order).First();

            customProperties[0] = new UdtPropertyNetPropertyMap(nameof(SimpleTestClass.Prop1), prop.Name);
            var typedef = new MetadataOracleNetTypeDefinition(cache, typeof(SimpleTestClass), metadataOracleType, customProperties, fuzzyNameMatch);
            var metadata = new MetadataOracleObject<SimpleTestClass>(typedef, new MetadataOracleCommon());

            var (constructor, lastNumber) = metadata.BuildQueryConstructorString(model, name, startNumber);

            Assert.NotNull(constructor);
            var expectedConstructor = $"{name} := {metadataOracleType.UDTInfo.FullObjectName}({prop.Name}=>:{startNumber++},"
                +
                $"{string.Join(',', metadataOracleType.Properties.OrderBy(c => c.Order).Where(c => c.Name != prop.Name).Select(c => $"{c.Name}=>null"))});" + Environment.NewLine;
            Assert.Equal(expectedConstructor, constructor);
            Assert.Equal(startNumber, lastNumber);
        }

        [Theory, CustomAutoData]
        internal void BuildQueryConstructorString_Object_SimpleNetProperty_WithMetadata(MetadataOracleNetTypeDefinition typedef, MetadataOracleNetTypeDefinition metTypeDef, SimpleTestClass model, string name, int startNumber)
        {
            var prop = typedef.Properties.OrderBy(c => c.Order).First();
            prop.NETProperty = typeof(SimpleTestClass).GetProperty(nameof(SimpleTestClass.Prop1));
            prop.PropertyMetadata = metTypeDef;

            var metadata = new MetadataOracleObject<SimpleTestClass>(typedef, new MetadataOracleCommon());

            var (constructor, lastNumber) = metadata.BuildQueryConstructorString(model, name, startNumber);

            Assert.NotNull(constructor);
            var expectedConstructor = new StringBuilder();
            expectedConstructor.AppendLine($"{name}_0 := {metTypeDef.UDTInfo.FullObjectName}({string.Join(',', metTypeDef.Properties.OrderBy(c => c.Order).Select(c => $"{c.Name}=>null"))});");

            expectedConstructor.Append($"{name} := {typedef.UDTInfo.FullObjectName}({prop.Name}=>{name}_0,");
            expectedConstructor.Append(string.Join(',', typedef.Properties.OrderBy(c => c.Order).Where(c => c.Name != prop.Name).Select(c => $"{c.Name}=>null")));
            expectedConstructor.AppendLine(");");
            Assert.Equal(expectedConstructor.ToString(), constructor);
            Assert.Equal(startNumber, lastNumber);
        }

        [Theory, CustomAutoData]
        internal void BuildQueryConstructorString_Object_SimpleNetProperty_NullPropertyValue(MetadataOracleNetTypeDefinition typedef, string name, int startNumber)
        {
            var prop = typedef.Properties.OrderBy(c => c.Order).First();
            prop.NETProperty = typeof(SimpleTestClass).GetProperty(nameof(SimpleTestClass.Prop1));

            var metadata = new MetadataOracleObject<SimpleTestClass>(typedef, new MetadataOracleCommon());

            var (constructor, lastNumber) = metadata.BuildQueryConstructorString(new SimpleTestClass(), name, startNumber);

            Assert.NotNull(constructor);
            var expectedConstructor = $"{name} := {typedef.UDTInfo.FullObjectName}({string.Join(',', typedef.Properties.OrderBy(c => c.Order).Select(c => $"{c.Name}=>null"))});" + Environment.NewLine;
            Assert.Equal(expectedConstructor, constructor);
            Assert.Equal(startNumber, lastNumber);
        }

        [Theory, CustomAutoData]
        internal void BuildQueryConstructorString_Array(MetadataOracleNetTypeDefinition typedef, SimpleTestClass[] model, string name, int startNumber)
        {
            var metadata = new MetadataOracleObject<SimpleTestClass[]>(typedef, new MetadataOracleCommon());

            var (constructor, lastNumber) = metadata.BuildQueryConstructorString(model, name, startNumber);

            Assert.NotNull(constructor);
            var expectedConstructor = new StringBuilder();
            for (var i = 0; i < model.Length; i++)
            {
                expectedConstructor.AppendLine($"{name}.extend;");
                expectedConstructor.Append($"{name}({name}.last) := {typedef.UDTInfo.FullObjectName}(");
                expectedConstructor.Append(string.Join(',', typedef.Properties.OrderBy(c => c.Order).Select(c => $"{c.Name}=>null")));
                expectedConstructor.AppendLine(");");
            }

            Assert.Equal(expectedConstructor.ToString(), constructor);
            Assert.Equal(startNumber, lastNumber);
        }

        #endregion BuildQueryConstructorString

        #region GetOracleParameters

        [Theory, CustomAutoData]
        internal void GetOracleParameters_Object_NoMatch_ReturnsEmpty(MetadataOracleNetTypeDefinition typedef, SimpleTestClass model, int startNumber)
        {
            var metadata = new MetadataOracleObject<SimpleTestClass>(typedef, new MetadataOracleCommon());

            var actual = metadata.GetOracleParameters(model, startNumber);

            Assert.NotNull(actual);
            Assert.Empty(actual);
        }

        [Theory, CustomAutoData]
        internal void GetOracleParameters_Object_NetProperty_NoMetadata_ReturnsParameters(MetadataOracleNetTypeDefinition typedef, SimpleTestClass model, int startNumber)
        {
            var prop = typedef.Properties.OrderBy(c => c.Order).First();
            prop.NETProperty = typeof(SimpleTestClass).GetProperty(nameof(SimpleTestClass.Prop1));

            var metadata = new MetadataOracleObject<SimpleTestClass>(typedef, new MetadataOracleCommon());

            var actual = metadata.GetOracleParameters(model, startNumber);

            Assert.NotNull(actual);
            var oraProp = Assert.Single(actual);

            Assert.NotNull(oraProp);
            Assert.Equal($":{startNumber}", oraProp.ParameterName);
            Assert.Equal(ParameterDirection.Input, oraProp.Direction);
            Assert.Equal(model.Prop1, oraProp.Value);
        }

        [Theory, CustomAutoData]
        internal void GetOracleParameters_Object_Metadata_ReturnsSubPropertyParameters(MetadataOracleNetTypeDefinition typedef, ComplexTestClass model, MetadataOracleNetTypeDefinition metaTypeDef, int startNumber)
        {
            var prop = typedef.Properties.OrderBy(c => c.Order).First();
            prop.NETProperty = typeof(ComplexTestClass).GetProperty(nameof(ComplexTestClass.ObjectProp));
            prop.PropertyMetadata = metaTypeDef;

            var subProp = metaTypeDef.Properties.OrderBy(c => c.Order).First();
            subProp.NETProperty = typeof(SimpleTestClass).GetProperty(nameof(SimpleTestClass.Prop1));

            var metadata = new MetadataOracleObject<ComplexTestClass>(typedef, new MetadataOracleCommon());

            var actual = metadata.GetOracleParameters(model, startNumber);

            Assert.NotNull(actual);
            var oraProp = Assert.Single(actual);

            Assert.NotNull(oraProp);
            Assert.Equal($":{startNumber}", oraProp.ParameterName);
            Assert.Equal(ParameterDirection.Input, oraProp.Direction);
            Assert.Equal(model.ObjectProp.Prop1, oraProp.Value);
        }

        [Theory, CustomAutoData]
        internal void GetOracleParameters_Object_Metadata_CollectionProperty_ObjectProperty_ReturnsSubPropertyParameters(MetadataOracleNetTypeDefinition typedef, CollectionPropertyTestClass model, MetadataOracleNetTypeDefinition metaTypeDef, int startNumber)
        {
            var prop = typedef.Properties.OrderBy(c => c.Order).First();
            prop.NETProperty = typeof(CollectionPropertyTestClass).GetProperty(nameof(CollectionPropertyTestClass.Prop2));
            prop.PropertyMetadata = metaTypeDef;

            var subProp = metaTypeDef.Properties.OrderBy(c => c.Order).First();
            subProp.NETProperty = typeof(SimpleTestClass).GetProperty(nameof(SimpleTestClass.Prop1));

            var metadata = new MetadataOracleObject<CollectionPropertyTestClass>(typedef, new MetadataOracleCommon());

            var actual = metadata.GetOracleParameters(model, startNumber);

            Assert.NotNull(actual);
            Assert.All(actual, c => Assert.Equal(ParameterDirection.Input, c.Direction));

            Assert.Collection(actual,
                (c) =>
                {
                    Assert.Equal($":{startNumber++}", c.ParameterName);
                    Assert.Equal(model.Prop2[0].Prop1, c.Value);
                },
                (c) =>
                {
                    Assert.Equal($":{startNumber++}", c.ParameterName);
                    Assert.Equal(model.Prop2[1].Prop1, c.Value);
                },
                (c) =>
                {
                    Assert.Equal($":{startNumber}", c.ParameterName);
                    Assert.Equal(model.Prop2[2].Prop1, c.Value);
                });
        }

        [Theory, CustomAutoData]
        internal void GetOracleParameters_Array_NoMatch_ReturnsEmpty(MetadataOracleNetTypeDefinition typedef, SimpleTestClass[] model, int startNumber)
        {
            var metadata = new MetadataOracleObject<SimpleTestClass[]>(typedef, new MetadataOracleCommon());

            var actual = metadata.GetOracleParameters(model, startNumber);

            Assert.NotNull(actual);
            Assert.Empty(actual);
        }

        #endregion GetOracleParameters

        #region GetRefCursorQuery

        [Theory, CustomAutoData]
        internal void GetRefCursorQuery_NoMetadata_ReturnsDummyQuery(MetadataOracleNetTypeDefinition typedef, int startNumber, string fieldName)
        {
            var metadata = new MetadataOracleObject<SimpleTestClass>(typedef, new MetadataOracleCommon());

            var actual = metadata.GetRefCursorQuery(startNumber, fieldName);

            Assert.NotNull(actual);
            Assert.Equal($"open :{startNumber} for select 1 dummy{Environment.NewLine} from dual;", actual);
        }

        [Theory, CustomAutoData]
        internal void GetRefCursorQuery_NetProperty_ReturnsRootQuery(MetadataOracleNetTypeDefinition typedef, int startNumber, string fieldName)
        {
            var prop = typedef.Properties.OrderBy(c => c.Order).First();
            prop.NETProperty = typeof(SimpleTestClass).GetProperty(nameof(SimpleTestClass.Prop1));

            var metadata = new MetadataOracleObject<SimpleTestClass>(typedef, new MetadataOracleCommon());

            var actual = metadata.GetRefCursorQuery(startNumber, fieldName);

            Assert.NotNull(actual);
            Assert.Equal($"open :{startNumber} for select value({fieldName}).{prop.Name} {prop.NETProperty.Name}{Environment.NewLine} from dual;", actual);
        }

        [Theory, CustomAutoData]
        internal void GetRefCursorQuery_NetProperty_MultipleProperties(MetadataOracleNetTypeDefinition typedef, int startNumber, string fieldName)
        {
            var prop = typedef.Properties.OrderBy(c => c.Order).First();
            prop.NETProperty = typeof(MultiplePropertiesTestClass).GetProperty(nameof(MultiplePropertiesTestClass.Prop1));

            var anotherProp = typedef.Properties.Where(c => c != prop).OrderBy(c => c.Order).First();
            anotherProp.NETProperty = typeof(MultiplePropertiesTestClass).GetProperty(nameof(MultiplePropertiesTestClass.Prop2));

            var metadata = new MetadataOracleObject<SimpleTestClass>(typedef, new MetadataOracleCommon());

            var actual = metadata.GetRefCursorQuery(startNumber, fieldName);

            Assert.NotNull(actual);
            var expected = $"open :{startNumber} for select value({fieldName}).{prop.Name} {prop.NETProperty.Name},"
                + Environment.NewLine
                + $"value({fieldName}).{anotherProp.Name} {anotherProp.NETProperty.Name}"
                + $"{Environment.NewLine} from dual;";
            Assert.Equal(expected, actual);
        }

        [Theory, CustomAutoData]
        internal void GetRefCursorQuery_WithMetadata_ReturnsXMLElement(MetadataOracleNetTypeDefinition typedef, MetadataOracleNetTypeDefinition metaTypeProp, int startNumber, string fieldName)
        {
            var prop = typedef.Properties.OrderBy(c => c.Order).First();
            prop.NETProperty = typeof(ComplexTestClass).GetProperty(nameof(ComplexTestClass.ObjectProp));
            prop.PropertyMetadata = metaTypeProp;

            var subProp = metaTypeProp.Properties.OrderBy(c => c.Order).First();
            subProp.NETProperty = typeof(SimpleTestClass).GetProperty(nameof(SimpleTestClass.Prop1));

            var metadata = new MetadataOracleObject<ComplexTestClass>(typedef, new MetadataOracleCommon());

            var actual = metadata.GetRefCursorQuery(startNumber, fieldName);

            Assert.NotNull(actual);

            var expected = $"open :{startNumber} for select (select xmlelement( \"{prop.NETProperty.Name}\", xmlconcat( "
                + $"XmlElement(\"{subProp.NETProperty.Name}\", value({fieldName}).{prop.Name}.{subProp.Name})) ) from dual) "
                + $"{prop.NETProperty.Name}{Environment.NewLine} from dual;";

            Assert.Equal(expected, actual);
        }

        [Theory, CustomAutoData]
        internal void GetRefCursorQuery_WithMetadata_CollectionProperty_ReturnsXMLElement(MetadataOracleNetTypeDefinition typedef, MetadataOracleNetTypeDefinition metaTypeProp, int startNumber, string fieldName)
        {
            var prop = typedef.Properties.OrderBy(c => c.Order).First();
            prop.NETProperty = typeof(CollectionPropertyTestClass).GetProperty(nameof(CollectionPropertyTestClass.Prop2));
            prop.PropertyMetadata = metaTypeProp;

            var subProp = metaTypeProp.Properties.OrderBy(c => c.Order).First();
            subProp.NETProperty = typeof(SimpleTestClass).GetProperty(nameof(SimpleTestClass.Prop1));

            var metadata = new MetadataOracleObject<CollectionPropertyTestClass>(typedef, new MetadataOracleCommon());

            var actual = metadata.GetRefCursorQuery(startNumber, fieldName);

            Assert.NotNull(actual);

            var expected = $"open :{startNumber} for select (select xmlelement( \"{prop.NETProperty.Name}\", xmlagg( xmlconcat( xmlelement( \"{prop.NETProperty.Name}\", "
                + $"XmlElement(\"{subProp.NETProperty.Name}\", d1.{subProp.Name})"
                + $") ) ) ) from table(value({fieldName}).{prop.Name}) d1)"
                + $" {prop.Name}{Environment.NewLine} from dual;";

            Assert.Equal(expected, actual);
        }

        [Theory, CustomAutoData]
        internal void GetRefCursorQuery_WithMetadata_Array_ReturnsXMLElement_WithDummy(MetadataOracleNetTypeDefinition typedef, MetadataOracleNetTypeDefinition metaTypeProp, int startNumber, string fieldName)
        {
            var prop = typedef.Properties.OrderBy(c => c.Order).First();
            prop.NETProperty = typeof(SimpleTestClass).GetProperty(nameof(SimpleTestClass.Prop1));
            prop.PropertyMetadata = metaTypeProp;

            var metadata = new MetadataOracleObject<SimpleTestClass[]>(typedef, new MetadataOracleCommon());

            var actual = metadata.GetRefCursorQuery(startNumber, fieldName);

            Assert.NotNull(actual);

            var expected = $"open :{startNumber} for select (select xmlelement( \"{prop.NETProperty.Name}\", xmlconcat( 1 dummy) "
                + $") from dual) {prop.NETProperty.Name}{Environment.NewLine} from table({fieldName}) c;";

            Assert.Equal(expected, actual);
        }

        [Theory, CustomAutoData]
        internal void GetRefCursorQuery_WithMetadata_Array_ReturnsXMLElement(MetadataOracleNetTypeDefinition typedef, MetadataOracleNetTypeDefinition metaTypeProp, int startNumber, string fieldName)
        {
            var prop = typedef.Properties.OrderBy(c => c.Order).First();
            prop.NETProperty = typeof(ComplexTestClass).GetProperty(nameof(ComplexTestClass.ObjectProp));
            prop.PropertyMetadata = metaTypeProp;

            var subProp = metaTypeProp.Properties.OrderBy(c => c.Order).First();
            subProp.NETProperty = typeof(SimpleTestClass).GetProperty(nameof(SimpleTestClass.Prop1));

            var metadata = new MetadataOracleObject<ComplexTestClass[]>(typedef, new MetadataOracleCommon());

            var actual = metadata.GetRefCursorQuery(startNumber, fieldName);

            Assert.NotNull(actual);

            var expected = $"open :{startNumber} for select (select xmlelement( \"{prop.NETProperty.Name}\", "
                + $"xmlconcat( XmlElement(\"{subProp.NETProperty.Name}\", value(c).{prop.Name}.{subProp.Name})) "
                + $") from dual) {prop.NETProperty.Name}{Environment.NewLine} from table({fieldName}) c;";

            Assert.Equal(expected, actual);
        }

        #endregion GetRefCursorQuery

        #region GetValueFromRefCursorAsync

        [Theory, CustomAutoData]
        internal async Task GetValueFromRefCursorAsync_Object(MetadataOracleNetTypeDefinition typedef, Mock<IOracleRefCursorWrapper> refCursorWrapper,
            Mock<IOracleDataReaderWrapper> dataReaderWrapper)
        {
            var metadata = new MetadataOracleObject<SimpleTestClass>(typedef, new MetadataOracleCommon());

            refCursorWrapper.Setup(r => r.GetDataReader()).Returns(dataReaderWrapper.Object);
            dataReaderWrapper.SetupSequence(d => d.ReadAsync())
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            var result = await metadata.GetValueFromRefCursorAsync(typeof(SimpleTestClass), refCursorWrapper.Object);

            Assert.NotNull(result);
            var actual = Assert.IsType<SimpleTestClass>(result);
            Assert.Null(actual.Prop1);
        }

        [Theory, CustomAutoData]
        internal async Task GetValueFromRefCursorAsync_Object_NetProperty(MetadataOracleNetTypeDefinition typedef, Mock<IOracleRefCursorWrapper> refCursorWrapper,
    Mock<IOracleDataReaderWrapper> dataReaderWrapper, Mock<MetadataOracleCommon> common, object oracleValue, string oracleValueStr)
        {
            var prop = typedef.Properties.OrderBy(c => c.Order).First();
            prop.NETProperty = typeof(SimpleTestClass).GetProperty(nameof(SimpleTestClass.Prop1));


            refCursorWrapper.Setup(r => r.GetDataReader()).Returns(dataReaderWrapper.Object);
            dataReaderWrapper.SetupSequence(d => d.ReadAsync())
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            dataReaderWrapper.Setup(d => d.GetOracleValue(0))
                .Returns(oracleValue);
            common.Setup(c => c.ConvertOracleParameterToBaseType(typeof(string), oracleValue))
                .Returns(oracleValueStr);


            var metadata = new MetadataOracleObject<SimpleTestClass>(typedef, common.Object);

            var result = await metadata.GetValueFromRefCursorAsync(typeof(SimpleTestClass), refCursorWrapper.Object);

            Assert.NotNull(result);
            var actual = Assert.IsType<SimpleTestClass>(result);
            Assert.Equal(oracleValueStr, actual.Prop1);
        }

        [Theory, CustomAutoData]
        internal async Task GetValueFromRefCursorAsync_Object_WithMetadata(MetadataOracleNetTypeDefinition typedef, MetadataOracleNetTypeDefinition subTypedef, Mock<IOracleRefCursorWrapper> refCursorWrapper,
Mock<IOracleDataReaderWrapper> dataReaderWrapper, Mock<MetadataOracleCommon> common, SimpleTestClass oracleValue)
        {
            var prop = typedef.Properties.OrderBy(c => c.Order).First();
            prop.NETProperty = typeof(ComplexTestClass).GetProperty(nameof(ComplexTestClass.ObjectProp));
            prop.PropertyMetadata = subTypedef;

            refCursorWrapper.Setup(r => r.GetDataReader()).Returns(dataReaderWrapper.Object);
            dataReaderWrapper.SetupSequence(d => d.ReadAsync())
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            common.Setup(c => c.GetValueFromOracleXML(typeof(SimpleTestClass), null))
                .Returns(oracleValue);


            var metadata = new MetadataOracleObject<ComplexTestClass>(typedef, common.Object);

            var result = await metadata.GetValueFromRefCursorAsync(typeof(ComplexTestClass), refCursorWrapper.Object);

            Assert.NotNull(result);
            var actual = Assert.IsType<ComplexTestClass>(result);
            Assert.NotNull(actual.ObjectProp);
            Assert.Equal(oracleValue, actual.ObjectProp);
        }

        #endregion GetValueFromRefCursorAsync
    }
}