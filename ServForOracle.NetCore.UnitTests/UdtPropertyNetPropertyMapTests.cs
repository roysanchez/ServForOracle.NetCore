using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class UdtPropertyNetPropertyMapTests
    {
        [Fact]
        public void Constructor_NullParameters_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("netPropertyName", () => new UdtPropertyNetPropertyMap(null, "test"));
            Assert.Throws<ArgumentNullException>("udtPropertyName", () => new UdtPropertyNetPropertyMap("test", null));
        }

        [Theory, CustomAutoData]
        public void Constructor_PropertiesSavedInUpperCase(string netProperty, string udtProperty)
        {
            var map = new UdtPropertyNetPropertyMap(netProperty, udtProperty);

            Assert.Equal(netProperty.ToUpper(), map.NetPropertyName);
            Assert.Equal(udtProperty.ToUpper(), map.UDTPropertyName);
        }

        [Theory, CustomAutoData]
        internal void IEquitable_Works(UdtPropertyNetPropertyMap x, UdtPropertyNetPropertyMap y)
        {
            Assert.True(x.Equals(x));
            Assert.False(x.Equals(y));
            Assert.False(x.Equals(null));
        }
    }
}
