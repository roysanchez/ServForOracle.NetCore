using ServForOracle.NetCore.Parameters;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class ParamTests
    {
        [Theory, CustomAutoData]
        public void Create_Boolean(bool value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var boolean = Assert.IsType<ParamBoolean>(param);
            Assert.NotNull(boolean);
            Assert.Equal(value, boolean.Value);
            Assert.Equal(direction, boolean.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_string(string value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<string>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_int(int value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<int>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_int_nullable(int? value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<int?>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_byte(byte value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<byte>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_byte_nullable(byte? value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<byte?>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_short(short value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<short>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_short_nullable(short? value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<short?>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_long(long value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<long>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_long_nullable(long? value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<long?>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_char(char value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<char>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_char_nullable(char? value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<char?>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_float(float value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<float>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_float_nullable(float? value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<float?>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_double(double value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<double>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_double_nullable(double? value, ParameterDirection direction)
        { 
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<double?>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_decimal(decimal value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<decimal>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_decimal_nullable(decimal? value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<decimal?>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_DateTime(DateTime value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<DateTime>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Theory, CustomAutoData]
        public void Create_ClrType_DateTime_nullable(DateTime? value, ParameterDirection direction)
        {
            var param = Param.Create(value, direction);

            var clrParam = Assert.IsType<ParamClrType<DateTime?>>(param);
            Assert.NotNull(clrParam);
            Assert.Equal(value, clrParam.Value);
            Assert.Equal(direction, clrParam.Direction);
        }

        [Fact]
        public void Create_InvalidType_Throws_Argument()
        {
            var message = $"The type {typeof(Guid?).FullName} is not supported by the library as a direct map to oracle.";

            var exception = Assert.Throws<ArgumentException>(() => Param.Create<Guid?>(null, ParameterDirection.Input));

            Assert.Equal(message, exception.Message);
        }

        [Theory, CustomAutoData]
        public void Create_Object(object obj, ParameterDirection direction)
        {
            var param = Param.Create<object>(obj, direction);

            var paramObj = Assert.IsType<ParamObject<object>>(param);
            Assert.NotNull(paramObj);
            Assert.Equal(obj, paramObj.Value);
            Assert.Equal(direction, paramObj.Direction);
        }

        [Theory, CustomAutoData]
        public void Input(object obj)
        {
            var param = Param.Input(obj);

            Assert.Equal(ParameterDirection.Input, param.Direction);
            Assert.Equal(obj, param.Value);
        }

        [Fact]
        public void Output()
        {
            var param = Param.Output<object>();

            Assert.Equal(ParameterDirection.Output, param.Direction);
        }

        [Theory, CustomAutoData]
        public void InputOutput(object obj)
        {
            var param = Param.InputOutput(obj);

            Assert.Equal(ParameterDirection.InputOutput, param.Direction);
            Assert.Equal(obj, param.Value);
        }

        [Fact]
        public void Type()
        {
            var param = Param.Output<object>();

            Assert.Equal(typeof(object), param.Type);
        }
    }
}
