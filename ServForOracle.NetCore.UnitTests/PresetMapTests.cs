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
        public void PresetConstructorThreeParametersWithoutCollectionWithProperties(string objectSchema, string objectName, string propertyName)
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
    }
}
