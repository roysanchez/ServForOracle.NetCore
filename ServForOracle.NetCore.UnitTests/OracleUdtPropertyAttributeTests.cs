using ServForOracle.NetCore.OracleAbstracts;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class OracleUdtPropertyAttributeTests
    {
        [Fact]
        public void Constructor_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>("propertyName", () => new OracleUdtPropertyAttribute(null));
        }
    }
}
