using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests.Config
{
    public static class AssertExtensions
    {
        public static void Equal(DateTime? expected, object actual, TimeSpan precision)
        {
            if (expected is null || actual is null)
            {
                Assert.Equal(expected, actual);
            }
            else
            {
                if (actual is DateTime)
                {
                    Assert.Equal(expected.Value, (DateTime)actual, precision);
                }
                else if (actual is DateTime?)
                {
                    Assert.Equal(expected.Value, ((DateTime?)actual).Value, precision);
                }
                else
                {
                    throw new Xunit.Sdk.EqualException(expected, actual);
                }
            }
        }
    }
}
