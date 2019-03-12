using System;
using System.Collections;
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

        public static void All(IEnumerable collection, Action<object> action)
        {
            foreach(var c in collection)
            {
                action(c);
            }
        }

        public static void Collection(IEnumerable collection, params Action<object>[] elementInspectors)
        {
            int expectedCount = elementInspectors.Length;

            int idx = 0;
            foreach(var el in collection)
            {
                try
                {
                    elementInspectors[idx++](el);
                }
                catch(Exception ex)
                {
                    throw new Xunit.Sdk.CollectionException(collection, expectedCount, 0, idx, ex);
                }
            }

            if (expectedCount != idx)
                throw new Xunit.Sdk.CollectionException(collection, expectedCount, idx);
        }

        public static void Length(IEnumerable collection, int expectedCount)
        {
            int idx = 0;
            foreach(var el in collection)
            {
                idx++;
            }
            
            if(idx != expectedCount)
            {
                throw new Xunit.Sdk.AssertCollectionCountException(expectedCount, idx);
            }
        }
    }
}
