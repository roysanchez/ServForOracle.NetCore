using AutoFixture;
using ServForOracle.NetCore.Cache;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.OracleAbstracts;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class MetadataOracleNetTypeDefinitionTests
    {
        public class TestClass
        {
        }

        public class SimpleClass
        {
            public string Prop1 { get; set; }
            public int Prop2 { get; set; }
            public double Prop3 { get; set; }
            public string[] Prop4 { get; set; }
        }

        [OracleUdt("prueba1", "prueba2")]
        public class ClassWithAttribute
        {
            [OracleUdtProperty("Test")]
            public string Prop1 { get; set; }
        }

        [Theory, CustomAutoData]
        internal void MetadataOracleNetTypeDefinition_NullBase_ThrowsArgumentNull(ServForOracleCache cache, Type type, UdtPropertyNetPropertyMap[] presetProperties, bool fuzzyNameMatch)
        {
            Assert.Throws<ArgumentNullException>("baseMetadataDefinition", () => new MetadataOracleNetTypeDefinition(cache, type, null, presetProperties, fuzzyNameMatch));
        }

        [Theory, CustomAutoData]
        internal void MetadataOracleNetTypeDefinition_NoTypeProperties(ServForOracleCache cache, MetadataOracleTypeDefinition baseMetadataDefinition, UdtPropertyNetPropertyMap[] presetProperties, bool fuzzyNameMatch)
        {
            var typedef = new MetadataOracleNetTypeDefinition(cache, typeof(TestClass), baseMetadataDefinition, presetProperties, fuzzyNameMatch);

            Assert.NotNull(typedef);
            Assert.Same(baseMetadataDefinition.UDTInfo, typedef.UDTInfo);
            Assert.NotNull(typedef.Properties);
            Assert.NotEmpty(typedef.Properties);
            Assert.Equal(baseMetadataDefinition.Properties.Count(), typedef.Properties.Count());
            Assert.All(typedef.Properties, c => Assert.Contains(baseMetadataDefinition.Properties, d => d.Name == c.Name));
            Assert.All(typedef.Properties, c => Assert.Null(c.NETProperty));
        }

        [Theory, CustomAutoData]
        internal void MetadataOracleNetTypeDefinition_Properties_WithMap(ServForOracleCache cache, MetadataOracleTypeDefinition baseMetadataDefinition, bool fuzzyNameMatch)
        {
            var props = baseMetadataDefinition.Properties.ToArray();
            var type = typeof(SimpleClass);
            var presetProperties = new UdtPropertyNetPropertyMap[]
            {
                new UdtPropertyNetPropertyMap(nameof(SimpleClass.Prop1), props[0].Name),
                new UdtPropertyNetPropertyMap(nameof(SimpleClass.Prop2), props[1].Name),
                new UdtPropertyNetPropertyMap(nameof(SimpleClass.Prop3), props[2].Name)
            };

            var typedef = new MetadataOracleNetTypeDefinition(cache, type, baseMetadataDefinition, presetProperties, fuzzyNameMatch);

            Assert.NotNull(typedef);
            Assert.Same(baseMetadataDefinition.UDTInfo, typedef.UDTInfo);
            Assert.NotNull(typedef.Properties);
            Assert.NotEmpty(typedef.Properties);
            Assert.Equal(baseMetadataDefinition.Properties.Count(), typedef.Properties.Count());
            Assert.All(typedef.Properties, c => Assert.Contains(baseMetadataDefinition.Properties, d => d.Name == c.Name && d.Order == c.Order));
            Assert.Collection(typedef.Properties,
                c => Assert.Equal(type.GetProperty(nameof(SimpleClass.Prop1)), c.NETProperty),
                c => Assert.Equal(type.GetProperty(nameof(SimpleClass.Prop2)), c.NETProperty),
                c => Assert.Equal(type.GetProperty(nameof(SimpleClass.Prop3)), c.NETProperty)
            );
        }

        [Theory, CustomAutoData]
        internal void MetadataOracleNetTypeDefinition_Properties_WithMap_WithMetadata(ServForOracleCache cache, MetadataOracleTypeDefinition baseMetadataDefinition, bool fuzzyNameMatch, MetadataOracleTypeSubTypeDefinition subTypeDef,
            MetadataOracleTypeSubTypeDefinition colSubTypeDef)
        {
            var props = baseMetadataDefinition.Properties.ToArray();
            var type = typeof(SimpleClass);
            var presetProperties = new UdtPropertyNetPropertyMap[]
            {
                new UdtPropertyNetPropertyMap(nameof(SimpleClass.Prop1), subTypeDef.Name),
                new UdtPropertyNetPropertyMap(nameof(SimpleClass.Prop2), props[1].Name),
                new UdtPropertyNetPropertyMap(nameof(SimpleClass.Prop3), props[2].Name),
                new UdtPropertyNetPropertyMap(nameof(SimpleClass.Prop4), colSubTypeDef.Name)
            };

            baseMetadataDefinition.Properties = new MetadataOracleTypePropertyDefinition[]
            {
                subTypeDef,
                props[1],
                props[2],
                colSubTypeDef
            };

            var typedef = new MetadataOracleNetTypeDefinition(cache, type, baseMetadataDefinition, presetProperties, fuzzyNameMatch);

            Assert.NotNull(typedef);
            Assert.Same(baseMetadataDefinition.UDTInfo, typedef.UDTInfo);
            Assert.NotNull(typedef.Properties);
            Assert.NotEmpty(typedef.Properties);
            Assert.Equal(4, typedef.Properties.Count());
            Assert.Collection(typedef.Properties,
                c =>
                {
                    Assert.Equal(subTypeDef.Name, c.Name, ignoreCase: true);
                    Assert.Equal(subTypeDef.Order, c.Order);
                    Assert.NotNull(c.PropertyMetadata);
                    Assert.NotNull(c.PropertyMetadata.Properties);
                    Assert.NotEmpty(c.PropertyMetadata.Properties);

                    Assert.All(c.PropertyMetadata.Properties, d => Assert.Contains(subTypeDef.MetadataOracleType.Properties, e => e.Name == d.Name && e.Order == d.Order));
                },
                c =>
                {
                    Assert.Equal(props[1].Name, c.Name, ignoreCase: true);
                    Assert.Equal(props[1].Order, c.Order);
                },
                c =>
                {
                    Assert.Equal(props[2].Name, c.Name, ignoreCase: true);
                    Assert.Equal(props[2].Order, c.Order);
                },
                c =>
                {
                    Assert.Equal(colSubTypeDef.Name, c.Name, ignoreCase: true);
                    Assert.Equal(colSubTypeDef.Order, c.Order);

                    Assert.NotNull(c.PropertyMetadata);
                    Assert.NotNull(c.PropertyMetadata.Properties);
                    Assert.NotEmpty(c.PropertyMetadata.Properties);

                    Assert.All(c.PropertyMetadata.Properties, d => Assert.Contains(colSubTypeDef.MetadataOracleType.Properties, e => e.Name == d.Name && e.Order == d.Order));
                }
            );
            Assert.Collection(typedef.Properties,
                c => Assert.Equal(type.GetProperty(nameof(SimpleClass.Prop1)), c.NETProperty),
                c => Assert.Equal(type.GetProperty(nameof(SimpleClass.Prop2)), c.NETProperty),
                c => Assert.Equal(type.GetProperty(nameof(SimpleClass.Prop3)), c.NETProperty),
                c => Assert.Equal(type.GetProperty(nameof(SimpleClass.Prop4)), c.NETProperty)
            );
        }

        [Theory, CustomAutoData]
        internal void MetadataOracleNetTypeDefinition_Properties_WithMap_WithAttribute(ServForOracleCache cache, MetadataOracleTypeDefinition baseMetadataDefinition, bool fuzzyNameMatch)
        {
            var props = baseMetadataDefinition.Properties.ToArray();
            var type = typeof(ClassWithAttribute);
            props[0].Name = "Test";
            var presetProperties = new UdtPropertyNetPropertyMap[]
            {
                new UdtPropertyNetPropertyMap(nameof(ClassWithAttribute.Prop1), props[0].Name)
            };

            var typedef = new MetadataOracleNetTypeDefinition(cache, type, baseMetadataDefinition, presetProperties, fuzzyNameMatch);

            Assert.NotNull(typedef);
            Assert.Same(baseMetadataDefinition.UDTInfo, typedef.UDTInfo);
            Assert.NotNull(typedef.Properties);
            Assert.NotEmpty(typedef.Properties);
            Assert.Equal(baseMetadataDefinition.Properties.Count(), typedef.Properties.Count());

            Assert.Collection(typedef.Properties,
                c =>
                {
                    Assert.Equal(type.GetProperty(nameof(ClassWithAttribute.Prop1)), c.NETProperty);
                    Assert.Equal("Test", c.Name, ignoreCase: true);
                    Assert.Equal(props[0].Order, c.Order);
                    Assert.Null(c.PropertyMetadata);
                },
                c =>
                {
                    Assert.Equal(props[1].Name, c.Name, ignoreCase: true);
                    Assert.Equal(props[1].Order, c.Order);
                    Assert.Null(c.NETProperty);
                    Assert.Null(c.PropertyMetadata);
                },
                c =>
                {
                    Assert.Equal(props[2].Name, c.Name, ignoreCase: true);
                    Assert.Equal(props[2].Order, c.Order);
                    Assert.Null(c.NETProperty);
                    Assert.Null(c.PropertyMetadata);
                }
            );
        }
    }
}
