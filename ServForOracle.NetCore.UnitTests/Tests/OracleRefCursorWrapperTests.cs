using Oracle.ManagedDataAccess.Types;
using ServForOracle.NetCore.Wrapper;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class OracleRefCursorWrapperTests
    {
        [Fact]
        public void Constructor_NullArgument_Throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OracleRefCursorWrapper(null));
        }

        [Fact]
        public void GetDataReader_Throws()
        {
            var refCursor = (OracleRefCursor)FormatterServices.GetUninitializedObject(typeof(OracleRefCursor));
            var wrapper = new OracleRefCursorWrapper(refCursor);
            Assert.ThrowsAny<Exception>(() => wrapper.GetDataReader());
        }
    }
}
