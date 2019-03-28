using ServForOracle.NetCore.Wrapper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests.Tests
{
    public class OracleRefCursorWrapperFactoryTests
    {
        [Fact]
        public void CallsOracleRefCursorWrapper()
        {
            var factory = new OracleRefCursorWrapperFactory();

            Assert.Throws<ArgumentNullException>("refCursor", () => factory.Create(null));
        }
    }
}
