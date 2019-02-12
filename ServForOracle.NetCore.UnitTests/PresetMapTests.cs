using System;
using Xunit;
using ServForOracle.NetCore.Config;
using ServForOracle.NetCore.OracleAbstracts;
using AutoFixture.Xunit2;

namespace ServForOracle.NetCore.UnitTests
{
    public class PresetMapTests
    {
        private readonly UDTInfoTests UDTInfoTests;
        public PresetMapTests()
        {
            UDTInfoTests = new UDTInfoTests();
        }
        public class TestClass
        {
            public string Roy { get; set; }
        }

        [Theory, AutoData]
        public void PresetConstructorAllParametersExceptProperties(string objectSchema, string objectName, string collectionSchema, string collectionName)
        {
            var PresetConfiguration = new ConfigurePresetMappings();
            var test = new PresetMap<TestClass>(objectSchema, objectName, collectionSchema, collectionName);

            Assert.NotNull(test.Info);
            Assert.Equal(typeof(TestClass), test.Type);

            UDTInfoTests.AssertUDTInfo(test.Info, objectSchema, objectName, collectionSchema, collectionName);
        }

        [Theory, AutoData]
        public void PresetConstructorAllParameters_CollectionSchemaNull_Throws(string objectSchema, string objectName, string collectionName)
        {
            var PresetConfiguration = new ConfigurePresetMappings();

            Assert.Throws<ArgumentNullException>(() => new PresetMap<TestClass>(objectSchema, objectName, (String)null, collectionName));
        }

        [Theory, AutoData]
        public void PresetConstructorAllParametersWithProperties(string objectSchema, string objectName, string collectionSchema, string collectionName, string propertyName)
        {
            var PresetConfiguration = new ConfigurePresetMappings();
            var test = new PresetMap<TestClass>(objectSchema, objectName, collectionSchema, collectionName, (c => c.Roy, propertyName));

            Assert.NotNull(test);
            Assert.NotNull(test.Type);
            Assert.Equal(typeof(TestClass), test.Type);

            UDTInfoTests.AssertUDTInfo(test.Info, objectSchema, objectName, collectionSchema, collectionName);
            UDTInfoTests.AssertReplacedProperties(test.ReplacedProperties, nameof(TestClass.Roy), propertyName);
        }

        [Theory, AutoData]
        public void PresetConstructorTwoParametersExceptProperties(string objectSchema, string objectName)
        {
            var PresetConfiguration = new ConfigurePresetMappings();
            var test = new PresetMap<TestClass>(objectSchema, objectName);

            Assert.NotNull(test);
            Assert.NotNull(test.Type);
            Assert.Equal(typeof(TestClass), test.Type);

            UDTInfoTests.AssertUDTInfo(test.Info, objectSchema, objectName);
        }

        [Theory, AutoData]
        public void PresetConstructorTwoParametersSchemaNull_Throws(string objectSchema)
        {
            var PresetConfiguration = new ConfigurePresetMappings();
            Assert.Throws<ArgumentNullException>(() => new PresetMap<TestClass>(objectSchema, (string)null));
        }

        [Theory, AutoData]
        public void PresetConstructorTwoParametersObjectNameNull_Throws(string objectName)
        {
            var PresetConfiguration = new ConfigurePresetMappings();
            Assert.Throws<ArgumentNullException>(() => new PresetMap<TestClass>((string)null, objectName));
        }

        [Theory, AutoData]
        public void PresetConstructorTwoParametersWithProperties(string objectSchema, string objectName, string propertyName)
        {
            var PresetConfiguration = new ConfigurePresetMappings();
            var test = new PresetMap<TestClass>(objectSchema, objectName, (c => c.Roy, propertyName));

            Assert.NotNull(test);
            Assert.NotNull(test.Type);
            Assert.Equal(typeof(TestClass), test.Type);

            UDTInfoTests.AssertUDTInfo(test.Info, objectSchema, objectName);
            UDTInfoTests.AssertReplacedProperties(test.ReplacedProperties, nameof(TestClass.Roy), propertyName);
        }

        [Theory, AutoData]
        public void PresetConstructorThreeParametersExceptProperties(string objectSchema, string objectName, string collectonName)
        {
            var PresetConfiguration = new ConfigurePresetMappings();
            var test = new PresetMap<TestClass>(objectSchema, objectName, collectonName);

            Assert.NotNull(test);
            Assert.NotNull(test.Type);
            Assert.Equal(typeof(TestClass), test.Type);

            UDTInfoTests.AssertUDTInfo(test.Info, objectSchema, objectName, collectonName);
        }

        [Theory, AutoData]
        public void PresetConstructorThreeParametersWithProperties(string objectSchema, string objectName, string collectionName, string propertyName)
        {
            var PresetConfiguration = new ConfigurePresetMappings();
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
            var PresetConfiguration = new ConfigurePresetMappings();

            Assert.Throws<ArgumentNullException>(() => new PresetMap<TestClass>(objectSchema, objectName, (string)null));
        }

        [Theory, AutoData]
        public void PresetConstructorOneParameterWithoutCollectionExceptProperties(string objectSchema, string objectName)
        {
            var PresetConfiguration = new ConfigurePresetMappings();
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
            var PresetConfiguration = new ConfigurePresetMappings();
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
            var PresetConfiguration = new ConfigurePresetMappings();
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
            var PresetConfiguration = new ConfigurePresetMappings();
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
            var PresetConfiguration = new ConfigurePresetMappings();
            Assert.Throws<ArgumentNullException>(() => new PresetMap<TestClass>((string)null));
        }

        [Theory, AutoData]
        public void PresetConstructorOneParameterWithoutCollectionNoSchema_Throws(string objectName)
        {
            var PresetConfiguration = new ConfigurePresetMappings();
            var fullName = $".{objectName}";

            Assert.Throws<ArgumentException>(() => new PresetMap<TestClass>(fullName));
        }

        [Theory, AutoData]
        public void PresetConstructorOneParameterWithoutCollectionNoObjectName_Throws(string objectSchema)
        {
            var PresetConfiguration = new ConfigurePresetMappings();
            var fullName = $"{objectSchema}.";

            Assert.Throws<ArgumentException>(() => new PresetMap<TestClass>(fullName));
        }

        [Theory, AutoData]
        public void PresetConstructorOneParameterWithCollectionNoSchemaForCollection_Throws(string objectSchema, string objectName,string collectionName)
        {
            var PresetConfiguration = new ConfigurePresetMappings();
            var fullName = $"{objectSchema}.{objectName}|.{collectionName}";

            Assert.Throws<ArgumentException>(() => new PresetMap<TestClass>(fullName));
        }

        [Theory, AutoData]
        public void PresetConstructorOneParameterWithCollectionNoNameForCollection_Throws(string objectSchema, string objectName)
        {
            var PresetConfiguration = new ConfigurePresetMappings();
            var fullName = $"{objectSchema}.{objectName}|{objectSchema}.";

            Assert.Throws<ArgumentException>(() => new PresetMap<TestClass>(fullName));
        }
    }
}