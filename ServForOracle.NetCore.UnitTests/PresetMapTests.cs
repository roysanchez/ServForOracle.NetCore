using System;
using Xunit;
using ServForOracle.NetCore.Config;
using ServForOracle.NetCore.OracleAbstracts;
using AutoFixture.Xunit2;
using System.Linq.Expressions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Microsoft.Extensions.Logging;
using ServForOracle.NetCore.Cache;

namespace ServForOracle.NetCore.UnitTests
{
    public class PresetMapTests
    {
        private readonly UDTInfoTests UDTInfoTests;
        private readonly Mock<ServForOracleCache> _memoryCacheMoq = new Mock<ServForOracleCache>(new Mock<IMemoryCache>().Object);
        private readonly Mock<ILogger<ConfigurePresetMappings>> _loggerMoq = new Mock<ILogger<ConfigurePresetMappings>>();

        public PresetMapTests()
        {
            UDTInfoTests = new UDTInfoTests();
        }

        private readonly (Expression<Func<TestClass, object>> property, string UDTPropertyName)[] NullReplacedProperties = null;

        private readonly (Expression<Func<TestClass, object>> property, string UDTPropertyName) ReplacedPropertiesNullNewName = (c => c.Roy, null);

        public class TestClass
        {
            public string Roy { get; set; }
            public SubClass SubClass { get; set; }
        }
        public class SubClass
        {
            public string Roy2 { get; set; }
        }

        [Theory, AutoData]
        public void PresetConstructorAllParametersExceptProperties(string objectSchema, string objectName, string collectionSchema, string collectionName)
        {
            
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var test = new PresetMap<TestClass>(objectSchema, objectName, collectionSchema, collectionName);

            Assert.NotNull(test.Info);
            Assert.Equal(typeof(TestClass), test.Type);

            UDTInfoTests.AssertUDTInfo(test.Info, objectSchema, objectName, collectionSchema, collectionName);
        }

        [Theory, AutoData]
        public void PresetConstructorAllParametersWithProperties(string objectSchema, string objectName, string collectionSchema, string collectionName, string propertyName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var test = new PresetMap<TestClass>(objectSchema, objectName, collectionSchema, collectionName, (c => c.Roy, propertyName));

            Assert.NotNull(test);
            Assert.NotNull(test.Type);
            Assert.Equal(typeof(TestClass), test.Type);

            UDTInfoTests.AssertUDTInfo(test.Info, objectSchema, objectName, collectionSchema, collectionName);
            UDTInfoTests.AssertReplacedProperties(test.ReplacedProperties, nameof(TestClass.Roy), propertyName);
        }

        [Theory, AutoData]
        public void PresetConstructorAllParameters_CollectionSchemaNull_Throws(string objectSchema, string objectName, string collectionName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);

            Assert.Throws<ArgumentNullException>(() => new PresetMap<TestClass>(objectSchema, objectName, (String)null, collectionName));
        }

        [Theory, AutoData]
        public void PresetConstructorAllParametersWithNullProperties_Throws(string objectSchema, string objectName, string collectionSchema, string collectionName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);

            Assert.Throws<ArgumentNullException>(() => new PresetMap<TestClass>(objectSchema, objectName, collectionSchema, collectionName, null));
        }

        [Theory, AutoData]
        public void PresetConstructorAllParametersWithPropertiesNullNewName_Throws(string objectSchema, string objectName, string collectionSchema, string collectionName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);

            Assert.Throws<ArgumentException>(() => new PresetMap<TestClass>(objectSchema, objectName, collectionSchema, collectionName, ReplacedPropertiesNullNewName));
        }

        [Theory, AutoData]
        public void PresetConstructorTwoParametersExceptProperties(string objectSchema, string objectName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var test = new PresetMap<TestClass>(objectSchema, objectName);

            Assert.NotNull(test);
            Assert.NotNull(test.Type);
            Assert.Equal(typeof(TestClass), test.Type);

            UDTInfoTests.AssertUDTInfo(test.Info, objectSchema, objectName);
        }

        [Theory, AutoData]
        public void PresetConstructorTwoParametersWithProperties(string objectSchema, string objectName, string propertyName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var test = new PresetMap<TestClass>(objectSchema, objectName, (c => c.Roy,propertyName));

            Assert.NotNull(test);
            Assert.NotNull(test.Type);
            Assert.Equal(typeof(TestClass), test.Type);

            UDTInfoTests.AssertUDTInfo(test.Info, objectSchema, objectName);
            UDTInfoTests.AssertReplacedProperties(test.ReplacedProperties, nameof(TestClass.Roy), propertyName);
        }

        [Theory, AutoData]
        public void PresetConstructorTwoParametersSchemaNull_Throws(string objectSchema)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            Assert.Throws<ArgumentNullException>(() => new PresetMap<TestClass>(objectSchema, (string)null));
        }

        [Theory, AutoData]
        public void PresetConstructorTwoParametersObjectNameNull_Throws(string objectName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            Assert.Throws<ArgumentNullException>(() => new PresetMap<TestClass>((string)null, objectName));
        }

        [Theory, AutoData]
        public void PresetConstructorTwoParametersWithNullProperties_Throws(string objectSchema, string objectName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);

            Assert.Throws<ArgumentNullException>(() => new PresetMap<TestClass>(objectSchema, objectName, NullReplacedProperties));
        }

        [Theory, AutoData]
        public void PresetConstructorTwoParametersWithPropertiesNullNewName_Throws(string objectSchema, string objectName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);

            Assert.Throws<ArgumentException>(() => new PresetMap<TestClass>(objectSchema, objectName, ReplacedPropertiesNullNewName));
        }

        [Theory, AutoData]
        public void PresetConstructorThreeParametersExceptProperties(string objectSchema, string objectName, string collectonName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var test = new PresetMap<TestClass>(objectSchema, objectName, collectonName);

            Assert.NotNull(test);
            Assert.NotNull(test.Type);
            Assert.Equal(typeof(TestClass), test.Type);

            UDTInfoTests.AssertUDTInfo(test.Info, objectSchema, objectName, collectonName);
        }

        [Theory, AutoData]
        public void PresetConstructorThreeParametersWithProperties(string objectSchema, string objectName, string collectionName, string propertyName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var test = new PresetMap<TestClass>(objectSchema, objectName, collectionName, (c => c.Roy, propertyName));

            Assert.NotNull(test);
            Assert.NotNull(test.Type);
            Assert.Equal(typeof(TestClass), test.Type);

            UDTInfoTests.AssertUDTInfo(test.Info, objectSchema, objectName, collectionName);
            UDTInfoTests.AssertReplacedProperties(test.ReplacedProperties, nameof(TestClass.Roy), propertyName);
        }

        [Theory, AutoData]
        public void PresetConstructorThreeParametersCollectionNameNull_Throws(string objectSchema, string objectName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);

            Assert.Throws<ArgumentNullException>(() => new PresetMap<TestClass>(objectSchema, objectName, (string)null));
        }

        [Theory, AutoData]
        public void PresetConstructorThreeParametersWithNullProperties_Throws(string objectSchema, string objectName, string collectionName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);

            Assert.Throws<ArgumentNullException>(() => new PresetMap<TestClass>(objectSchema, objectName, collectionName, NullReplacedProperties));
        }

        [Theory, AutoData]
        public void PresetConstructorThreeParametersWithPropertiesNullNewName_Throws(string objectSchema, string objectName, string collectionName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);

            Assert.Throws<ArgumentException>(() => new PresetMap<TestClass>(objectSchema, objectName, collectionName, ReplacedPropertiesNullNewName));
        }


        [Theory, AutoData]
        public void PresetConstructorOneParameterWithoutCollectionExceptProperties(string objectSchema, string objectName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var fullName = $"{objectSchema}.{objectName}";
            var test = new PresetMap<TestClass>(fullName);

            Assert.NotNull(test);
            Assert.NotNull(test.Type);
            Assert.Equal(typeof(TestClass), test.Type);

            UDTInfoTests.AssertUDTInfo(test.Info, fullName);
        }

        [Theory, AutoData]
        public void PresetConstructorOneParametersWithoutCollectionWithProperties(string objectSchema, string objectName, string propertyName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var fullName = $"{objectSchema}.{objectName}";
            var test = new PresetMap<TestClass>(fullName, (c => c.Roy, propertyName));

            Assert.NotNull(test);
            Assert.NotNull(test.Type);
            Assert.Equal(typeof(TestClass), test.Type);

            UDTInfoTests.AssertUDTInfo(test.Info, fullName);
            UDTInfoTests.AssertReplacedProperties(test.ReplacedProperties, nameof(TestClass.Roy), propertyName);
        }

        [Theory, AutoData]
        public void PresetConstructorOneParameterWithCollectionExceptProperties(string objectSchema, string collectionName, string objectName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var fullName = $"{objectSchema}.{objectName}|{objectSchema}.{collectionName}";
            var test = new PresetMap<TestClass>(fullName);

            Assert.NotNull(test);
            Assert.NotNull(test.Type);
            Assert.Equal(typeof(TestClass), test.Type);

            UDTInfoTests.AssertUDTInfo(test.Info, fullName);
        }

        [Theory, AutoData]
        public void PresetConstructorOneParametersWithCollectionWithProperties(string objectSchema, string objectName, string collectionName, string propertyName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var fullName = $"{objectSchema}.{objectName}|{objectSchema}.{collectionName}";
            var test = new PresetMap<TestClass>(fullName, (c => c.Roy, propertyName));

            Assert.NotNull(test);
            Assert.NotNull(test.Type);
            Assert.Equal(typeof(TestClass), test.Type);

            UDTInfoTests.AssertUDTInfo(test.Info, fullName);
            UDTInfoTests.AssertReplacedProperties(test.ReplacedProperties, nameof(TestClass.Roy), propertyName);
        }

        [Fact]
        public void PresetConstructorOneParametersNullValue_Throws()
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            Assert.Throws<ArgumentNullException>(() => new PresetMap<TestClass>((string)null));
        }

        [Theory, AutoData]
        public void PresetConstructorOneParameterWithoutCollectionNoSchema_Throws(string objectName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var fullName = $".{objectName}";

            Assert.Throws<ArgumentException>(() => new PresetMap<TestClass>(fullName));
        }

        [Theory, AutoData]
        public void PresetConstructorOneParameterWithoutCollectionNoObjectName_Throws(string objectSchema)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var fullName = $"{objectSchema}.";

            Assert.Throws<ArgumentException>(() => new PresetMap<TestClass>(fullName));
        }

        [Theory, AutoData]
        public void PresetConstructorOneParameterWithCollectionNoSchemaForCollection_Throws(string objectSchema, string objectName,string collectionName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var fullName = $"{objectSchema}.{objectName}|.{collectionName}";

            Assert.Throws<ArgumentException>(() => new PresetMap<TestClass>(fullName));
        }

        [Theory, AutoData]
        public void PresetConstructorOneParameterWithCollectionNoNameForCollection_Throws(string objectSchema, string objectName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var fullName = $"{objectSchema}.{objectName}|{objectSchema}.";

            Assert.Throws<ArgumentException>(() => new PresetMap<TestClass>(fullName));
        }

        [Theory, AutoData]
        public void PresetConstructorOneParametersWithNullProperties_Throws(string objectSchema, string objectName, string collectionName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var fullName = $"{objectSchema}.{objectName}|{objectSchema}.{collectionName}";

            Assert.Throws<ArgumentNullException>(() => new PresetMap<TestClass>(fullName, NullReplacedProperties));
        }

        [Theory, AutoData]
        public void PresetConstructorOneParametersWithPropertiesNullNewName_Throws(string objectSchema, string objectName, string collectionName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var fullName = $"{objectSchema}.{objectName}|{objectSchema}.{collectionName}";

            Assert.Throws<ArgumentException>(() => new PresetMap<TestClass>(fullName, ReplacedPropertiesNullNewName));
        }

        [Theory, AutoData]
        public void PresetConstructorOracleUdtInfoExceptProperties(string objectSchema, string objectName, string collectionName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var info = new OracleUdtInfo(objectSchema, objectName, collectionName);

            var test = new PresetMap<TestClass>(info);

            Assert.NotNull(test);
            Assert.NotNull(test.Type);
            Assert.Equal(typeof(TestClass), test.Type);

            Assert.Equal(info, test.Info);
        }

        [Theory, AutoData]
        public void PresetConstructorOracleUdtInfWithProperties(string objectSchema, string objectName, string collectionName, string propertyName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var info = new OracleUdtInfo(objectSchema, objectName, collectionName);

            var test = new PresetMap<TestClass>(info, (c => c.Roy, propertyName));

            Assert.NotNull(test);
            Assert.NotNull(test.Type);
            Assert.Equal(typeof(TestClass), test.Type);

            Assert.Equal(info, test.Info);
            UDTInfoTests.AssertReplacedProperties(test.ReplacedProperties, nameof(TestClass.Roy), propertyName);
        }

        [Theory, AutoData]
        public void Preset_PropertiesMappedCorrectly(string objectSchema, string objectName, string collectionName, string propertyName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var info = new OracleUdtInfo(objectSchema, objectName, collectionName);

            var test = new PresetMap<TestClass>(info, (c => c.Roy, propertyName));

            Assert.NotNull(test);
            Assert.NotNull(test.ReplacedProperties);
            Assert.Single(test.ReplacedProperties);

            var replaced = test.ReplacedProperties[0];
            Assert.NotNull(replaced);
            Assert.Equal(nameof(TestClass.Roy).ToUpper(), replaced.NetPropertyName);
            Assert.Equal(propertyName.ToUpper(), replaced.UDTPropertyName);
        }

        [Theory, AutoData]
        public void Preset_PropertiesMap_IncorrectValues_ThrowsErrors(string objectSchema, string objectName, string collectionName, string propertyName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var info = new OracleUdtInfo(objectSchema, objectName, collectionName);

            Assert.Throws<ArgumentException>(() => new PresetMap<TestClass>(info, (c => null, propertyName)));
            Assert.Throws<ArgumentException>(() => new PresetMap<TestClass>(info, (c => new TestClass(), propertyName)));
        }

        [Theory, AutoData]
        public void Preset_PropertiesMap_BoxedValues_HandlesCorrectly(string objectSchema, string objectName, string collectionName, string propertyName)
        {
            var PresetConfiguration = new ConfigurePresetMappings(_loggerMoq.Object, _memoryCacheMoq.Object);
            var info = new OracleUdtInfo(objectSchema, objectName, collectionName);

            var test = new PresetMap<TestClass>(info, (c => (object)c.Roy, propertyName));

            Assert.NotNull(test);
            Assert.NotNull(test.ReplacedProperties);
            Assert.Single(test.ReplacedProperties);

            var replaced = test.ReplacedProperties[0];
            Assert.NotNull(replaced);
            Assert.Equal(nameof(TestClass.Roy).ToUpper(), replaced.NetPropertyName);
            Assert.Equal(propertyName.ToUpper(), replaced.UDTPropertyName);
        }
    }
}