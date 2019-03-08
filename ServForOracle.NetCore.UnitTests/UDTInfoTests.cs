using AutoFixture.Xunit2;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.OracleAbstracts;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class UDTInfoTests
    {
        internal void AssertUDTInfo(OracleUdtInfo info, string objectSchema, string objectName)
        {
            Assert.NotNull(info);
            Assert.Equal(objectSchema?.ToUpper(), info.ObjectSchema);
            Assert.Equal(objectName?.ToUpper(), info.ObjectName);

            Assert.Equal($"{objectSchema}.{objectName}".ToUpper(), info.FullObjectName);
        }

        internal void AssertUDTInfo(OracleUdtInfo info, string objectSchema, string objectName, string collectionName)
        {
            Assert.NotNull(info);
            Assert.Equal(objectSchema?.ToUpper(), info.ObjectSchema);
            Assert.Equal(objectName?.ToUpper(), info.ObjectName);
            Assert.Equal(collectionName?.ToUpper(), info.CollectionName);

            Assert.Equal($"{objectSchema}.{objectName}".ToUpper(), info.FullObjectName);
            Assert.True(info.IsCollectionValid);
        }

        internal void AssertUDTInfo(OracleUdtInfo info, string objectName)
        {
            Assert.NotNull(info);
            Assert.NotNull(objectName);

            void ValidateObjectSchema(string objectSchema, string expectedSchema, string expectedName)
            {
                Assert.NotNull(objectSchema);
                var objectParts = objectSchema.Split(".");
                Assert.NotEmpty(objectParts);
                Assert.Equal(2, objectParts.Length);
                Assert.Equal(expectedSchema, objectParts[0]);
                Assert.Equal(expectedName, objectParts[1]);
            }

            if(objectName.Contains("|"))
            {
                var parts = objectName.Split("|");
                Assert.NotEmpty(parts);
                Assert.Equal(2, parts.Length);
                ValidateObjectSchema(parts[0], info.ObjectSchema, info.ObjectName);
                ValidateObjectSchema(parts[1], info.CollectionSchema, info.CollectionName);
                Assert.True(info.IsCollectionValid);
            }
            else
            {
                ValidateObjectSchema(objectName, info.ObjectSchema, info.ObjectName);
            }
        }

        internal void AssertUDTInfo(OracleUdtInfo info, string objectSchema, string objectName, string collectionSchema, string collectionName)
        {
            Assert.NotNull(info);
            Assert.Equal(objectSchema?.ToUpper(), info.ObjectSchema);
            Assert.Equal(objectName?.ToUpper(), info.ObjectName);
            Assert.Equal(collectionSchema?.ToUpper(), info.CollectionSchema);
            Assert.Equal(collectionName?.ToUpper(), info.CollectionName);

            Assert.Equal($"{objectSchema}.{objectName}".ToUpper(), info.FullObjectName);
            Assert.Equal($"{collectionSchema}.{collectionName}".ToUpper(), info.FullCollectionName);

            Assert.True(info.IsCollectionValid);
        }

        internal void AssertReplacedProperties(UdtPropertyNetPropertyMap[] replacedProperties, string propertyName, string replacedProperty)
        {
            Assert.NotNull(replacedProperties);
            Assert.NotEmpty(replacedProperties);
            Assert.NotNull(propertyName);
            Assert.NotNull(replacedProperty);
            Assert.Equal(propertyName.ToUpper(), replacedProperties[0].NetPropertyName);
            Assert.Equal(replacedProperty.ToUpper(), replacedProperties[0].UDTPropertyName);
        }

        [Fact]
        public void Constructor_OneParameter_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("objectName", () => new OracleUdtInfo(null));
        }

        [Theory, CustomAutoData]
        public void Constructor_OneParameter_Invalid_ThrowsArgument(string objectName)
        {
            var expected = $"The object {objectName} is invalid, it needs to have the format SCHEMA.OBJECT_NAME or" +
                $" SCHEMA.OBJECTNAME|SCHEMA.COLLECTIONNAME{Environment.NewLine}Parameter name: name";

            var exception = Assert.Throws<ArgumentException>("name", () => new OracleUdtInfo(objectName));
            Assert.Equal(expected, exception.Message);
        }
    }
}
