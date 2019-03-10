using ServForOracle.NetCore.Extensions;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class TypeExtensionsTests
    {
        class TestClass
        {
            public TestClass(string test)
            {

            }
        }

        [Fact]
        public void IsBoolean_ReturnsTrue()
        {
            Assert.True(TypeExtensions.IsBoolean(typeof(bool)));
        }

        [Fact]
        public void IsBoolean_ReturnsFalse()
        {
            Assert.False(TypeExtensions.IsBoolean(typeof(string)));
        }

        [Fact]
        public void IsClrType_ReturnsTrue()
        {
            Assert.True(TypeExtensions.IsClrType(typeof(string)));
            Assert.True(TypeExtensions.IsClrType(typeof(int)));
        }

        [Fact]
        public void IsClrType_ReturnsFalse()
        {
            Assert.False(TypeExtensions.IsClrType(typeof(object)));
        }

        [Fact]
        public void IsCollection_ReturnsTrue()
        {
            Assert.True(TypeExtensions.IsCollection(typeof(object[])));
            Assert.True(TypeExtensions.IsCollection(typeof(IEnumerable<object>)));
            Assert.True(TypeExtensions.IsCollection(typeof(List<object>)));
            Assert.True(TypeExtensions.IsCollection(typeof(Collection<object>)));
            Assert.True(TypeExtensions.IsCollection(typeof(IList<object>)));
        }

        [Fact]
        public void IsCollection_ReturnsFalse()
        {
            Assert.False(TypeExtensions.IsCollection(typeof(object)));
            Assert.False(TypeExtensions.IsCollection(typeof(IEqualityComparer<object>)));
        }

        [Fact]
        public void GetCollectionUnderType_ReturnsGenericType()
        {
            var type = typeof(object);
            Assert.Equal(type, TypeExtensions.GetCollectionUnderType(typeof(object[])));
            Assert.Equal(type, TypeExtensions.GetCollectionUnderType(typeof(IEnumerable<object>)));
            Assert.Equal(type, TypeExtensions.GetCollectionUnderType(typeof(List<object>)));
            Assert.Equal(type, TypeExtensions.GetCollectionUnderType(typeof(Collection<object>)));
            Assert.Equal(type, TypeExtensions.GetCollectionUnderType(typeof(IList<object>)));
        }

        [Fact]
        public void GetCollectionUnderType_NotCollection_ReturnsNull()
        {
            Assert.Null(TypeExtensions.GetCollectionUnderType(typeof(int)));
            Assert.Null(TypeExtensions.GetCollectionUnderType(typeof(string)));
        }

        [Fact]
        public void GetCollectionUnderType_Invalid_ReturnsNull()
        {
            Assert.Null(TypeExtensions.GetCollectionUnderType(typeof(int)));
            Assert.Null(TypeExtensions.GetCollectionUnderType(typeof(string)));
        }

        [Theory, CustomAutoData]
        public void CreateInstance_Works(string ctorParameter)
        {
            Assert.IsType<List<object>>(TypeExtensions.CreateInstance(typeof(List<object>)));
            Assert.IsType<object[]>(TypeExtensions.CreateInstance(typeof(object[])));
            Assert.IsType<TestClass>(TypeExtensions.CreateInstance(typeof(TestClass), ctorParameter));
            Assert.IsType<int>(TypeExtensions.CreateInstance(typeof(int)));
        }

        [Fact]
        public void CreateListType_Works()
        {
            Assert.Equal(typeof(List<object>),TypeExtensions.CreateListType(typeof(object)));
            Assert.Equal(typeof(List<List<object>>),TypeExtensions.CreateListType(typeof(List<object>)));
            Assert.Equal(typeof(List<object[]>),TypeExtensions.CreateListType(typeof(object[])));
        }

        [Fact]
        public void CanMapToOracle_Works()
        {
            Assert.True(TypeExtensions.CanMapToOracle(typeof(string)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(char)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(char?)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(short)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(short?)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(byte)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(byte?)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(int)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(int?)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(long)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(long?)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(float)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(float?)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(double)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(double?)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(decimal)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(decimal?)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(DateTime)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(DateTime?)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(bool)));
            Assert.True(TypeExtensions.CanMapToOracle(typeof(bool?)));
        }

        [Fact]
        public void CanMapToOracle_Fails()
        {
            Assert.False(TypeExtensions.CanMapToOracle(typeof(sbyte)));
            Assert.False(TypeExtensions.CanMapToOracle(typeof(sbyte?)));
            Assert.False(TypeExtensions.CanMapToOracle(typeof(uint)));
            Assert.False(TypeExtensions.CanMapToOracle(typeof(uint?)));
            Assert.False(TypeExtensions.CanMapToOracle(typeof(ulong)));
            Assert.False(TypeExtensions.CanMapToOracle(typeof(ulong?)));
            Assert.False(TypeExtensions.CanMapToOracle(typeof(Guid)));
            Assert.False(TypeExtensions.CanMapToOracle(typeof(Guid?)));
            Assert.False(TypeExtensions.CanMapToOracle(typeof(TimeSpan)));
            Assert.False(TypeExtensions.CanMapToOracle(typeof(TimeSpan?)));
            Assert.False(TypeExtensions.CanMapToOracle(typeof(DateTimeOffset)));
            Assert.False(TypeExtensions.CanMapToOracle(typeof(DateTimeOffset?)));
        }
    }
}
