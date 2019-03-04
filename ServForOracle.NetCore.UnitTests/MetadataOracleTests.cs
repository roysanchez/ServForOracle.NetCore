using AutoFixture;
using Moq;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.UnitTests.Config;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using Xunit;

namespace ServForOracle.NetCore.UnitTests
{
    public class MetadataOracleTests
    {
        public class TestClass
        {

        }

        public DateTime? Truncate(DateTime? dateTime)
        {
            if (!dateTime.HasValue) return null;
            return Truncate(dateTime.Value);
        }
        public DateTime Truncate(DateTime dateTime)
        {
            var timeSpan = TimeSpan.FromSeconds(1);
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue) return dateTime;
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }

        #region ConvertOracleParameterToBaseType

        [Fact]
        public void ConvertOracleParameterToBaseType_OracleParamIsNull()
        {
            var type = typeof(TestClass);
            var metadata = new MetadataOracle();

            Assert.Null(metadata.ConvertOracleParameterToBaseType(type, null));
        }

        [Theory, CustomAutoData]
        public void ConvertOracleParameterToBaseType_OracleParamIsSameAsReturn(TestClass test)
        {
            var type = typeof(TestClass);
            var metadata = new MetadataOracle();

            Assert.Equal(test, metadata.ConvertOracleParameterToBaseType(type, test));
        }

        [Fact]
        public void ConvertOracleParameterToBaseType_OracleParamDbNull_ReturnsNull()
        {
            var oracleParameter = new OracleParameter
            {
                Value = DBNull.Value
            };
            var type = typeof(TestClass);
            var metadata = new MetadataOracle();

            Assert.Null(metadata.ConvertOracleParameterToBaseType(type, oracleParameter));
        }

        [Fact]
        public void ConvertOracleParameterToBaseType_OracleDecimal_IsNull_ReturnsNull()
        {
            var oracleParameter = new OracleParameter
            {
                Value = new OracleDecimal()
            };
            var type = typeof(TestClass);
            var metadata = new MetadataOracle();

            Assert.Null(metadata.ConvertOracleParameterToBaseType(typeof(TestClass), oracleParameter));
            Assert.Null(metadata.ConvertOracleParameterToBaseType(typeof(int?), oracleParameter));
            Assert.Null(metadata.ConvertOracleParameterToBaseType(typeof(string), oracleParameter));
        }

        [Fact]
        public void ConvertOracleParameterToBaseType_OracleDecimal_IsNull_ThrowsCastError()
        {
            var oracleParameter = new OracleParameter
            {
                Value = new OracleDecimal()
            };
            var metadata = new MetadataOracle();
            var type = typeof(int);

            var exception = Assert.Throws<InvalidCastException>(() => metadata.ConvertOracleParameterToBaseType(type, oracleParameter));
            Assert.Equal($"Can't cast a null value to {type.FullName}", exception.Message);
        }

        [Theory, CustomAutoData]
        public void ConvertOracleParameterToBaseType_OracleDecimal_Returns(int p1, int? p2, float p3, float? p4, double p5, double? p6, decimal p7, decimal? p8, byte p9, byte? p10)
        {
            var metadata = new MetadataOracle();
            var nul = new OracleDecimal();
            string p11 = p1.ToString();

            Assert.Equal(p1, metadata.ConvertOracleParameterToBaseType(p1.GetType(), new OracleParameter { Value = new OracleDecimal(p1) }));
            Assert.Equal(p2, metadata.ConvertOracleParameterToBaseType(p2.GetType(), new OracleParameter { Value = p2.HasValue ? new OracleDecimal(p2.Value.ToString()) : nul }));
            Assert.Equal(p3, metadata.ConvertOracleParameterToBaseType(p3.GetType(), new OracleParameter { Value = new OracleDecimal(p3) }));
            Assert.Equal(p4, metadata.ConvertOracleParameterToBaseType(p4.GetType(), new OracleParameter { Value = p4.HasValue ? new OracleDecimal(p4.Value.ToString()) : nul }));
            Assert.Equal(p5, metadata.ConvertOracleParameterToBaseType(p5.GetType(), new OracleParameter { Value = new OracleDecimal(p5) }));
            Assert.Equal(p6, metadata.ConvertOracleParameterToBaseType(p6.GetType(), new OracleParameter { Value = p6.HasValue ? new OracleDecimal(p6.Value.ToString()) : nul }));
            Assert.Equal(p7, metadata.ConvertOracleParameterToBaseType(p7.GetType(), new OracleParameter { Value = new OracleDecimal(p7) }));
            Assert.Equal(p8, metadata.ConvertOracleParameterToBaseType(p8.GetType(), new OracleParameter { Value = p8.HasValue ? new OracleDecimal(p8.Value.ToString()) : nul }));
            Assert.Equal(p9, metadata.ConvertOracleParameterToBaseType(p9.GetType(), new OracleParameter { Value = new OracleDecimal(p9) }));
            Assert.Equal(p10, metadata.ConvertOracleParameterToBaseType(p10.GetType(), new OracleParameter { Value = p10.HasValue ? new OracleDecimal(p10.Value.ToString()) : nul }));
            Assert.Equal(p11, metadata.ConvertOracleParameterToBaseType(p11.GetType(), new OracleParameter { Value = new OracleDecimal(p11) }));
        }

        [Theory, CustomAutoData]
        public void ConvertOracleParameterToBaseType_OracleDecimal_CantCastToReturnType_ThrowsCastError(decimal dec)
        {
            var oracleParameter = new OracleParameter
            {
                Value = new OracleDecimal(dec)
            };
            var metadata = new MetadataOracle();
            var type = typeof(TestClass);

            var exception = Assert.Throws<InvalidCastException>(() => metadata.ConvertOracleParameterToBaseType(type, oracleParameter));
            Assert.Equal(exception.Message, $"Can't cast an OracleDecimal to {type.FullName}, received val:" + dec.ToString());
        }

        [Fact]
        public void ConvertOracleParameterToBaseType_OracleString_IsNull_ReturnsNull()
        {
            var oracleParameter = new OracleParameter
            {
                Value = new OracleString()
            };
            var metadata = new MetadataOracle();

            Assert.Null(metadata.ConvertOracleParameterToBaseType(typeof(string), oracleParameter));
        }

        [Theory, CustomAutoData]
        public void ConvertOracleParameterToBaseType_OracleString_Returns(string str)
        {
            var oracleParameter = new OracleParameter
            {
                Value = new OracleString(str)
            };
            var metadata = new MetadataOracle();

            var actual = metadata.ConvertOracleParameterToBaseType(typeof(string), oracleParameter);

            Assert.Equal(str, actual);
        }

        [Fact]
        public void ConvertOracleParameterToBaseType_OracleClob_Throws_IsSealed()
        {
            //Clob is a sealed class
            var clob = (OracleClob)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(OracleClob));

            var oracleParameter = new OracleParameter
            {
                Value = clob
            };
            var metadata = new MetadataOracle();

            Assert.Throws<OracleNullValueException>(() => metadata.ConvertOracleParameterToBaseType(typeof(string), oracleParameter));
        }

        [Fact]
        public void ConvertOracleParameterToBaseType_OracleBFile_Throws_IsSealed()
        {
            //bfile is a sealed class
            var bfile = (OracleBFile)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(OracleBFile));

            var oracleParameter = new OracleParameter
            {
                Value = bfile
            };
            var metadata = new MetadataOracle();

            Assert.Throws<OracleNullValueException>(() => metadata.ConvertOracleParameterToBaseType(typeof(byte[]), oracleParameter));
        }

        [Fact]
        public void ConvertOracleParameterToBaseType_OracleBlob_Throws_IsSealed()
        {
            //blob is a sealed class
            var bfile = (OracleBlob)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(OracleBlob));

            var oracleParameter = new OracleParameter
            {
                Value = bfile
            };
            var metadata = new MetadataOracle();

            Assert.Throws<OracleNullValueException>(() => metadata.ConvertOracleParameterToBaseType(typeof(byte[]), oracleParameter));
        }

        [Theory, CustomAutoData]
        public void ConvertOracleParameterToBaseType_OracleDate(DateTime d1, DateTime? d2)
        {
            var metadata = new MetadataOracle();
            var nul = new OracleDate();


            Assert.Equal(Truncate(d1), metadata.ConvertOracleParameterToBaseType(d1.GetType(), new OracleParameter { Value = new OracleDate(d1) }));
            Assert.Equal(Truncate(d2), metadata.ConvertOracleParameterToBaseType(d2.GetType(), new OracleParameter { Value = d2.HasValue ? new OracleDate(d2.Value.ToString("MM'/'dd'/'yyyy HH:mm:ss")) : nul }));
        }

        [Theory, CustomAutoData]
        public void ConvertOracleParameterToBaseType_OracleDate_Null_Throws(DateTime d1)
        {
            var oracleParameter = new OracleParameter
            {
                Value = new OracleDate()
            };
            var metadata = new MetadataOracle();


            var exception = Assert.Throws<InvalidCastException>(() => metadata.ConvertOracleParameterToBaseType(d1.GetType(), oracleParameter));

            Assert.Equal($"Can't cast a null value to {d1.GetType().FullName}", exception.Message);
        }

        [Theory, CustomAutoData]
        public void ConvertOracleParameterToBaseType_OracleIntervalDS(TimeSpan t1, TimeSpan? t2)
        {
            var metadata = new MetadataOracle();
            var nul = new OracleIntervalDS();

            Assert.Equal(t1, metadata.ConvertOracleParameterToBaseType(t1.GetType(), new OracleParameter { Value = new OracleIntervalDS(t1) }));
            Assert.Equal(t2, metadata.ConvertOracleParameterToBaseType(t2.GetType(), new OracleParameter { Value = t2.HasValue ? new OracleIntervalDS(t2.Value < TimeSpan.Zero ? "-" : "" + t2.Value.ToString("'0'dddddddd' 'hh':'mm':'ss'.'fffffff'00'")) : nul }));
        }

        [Theory, CustomAutoData]
        public void ConvertOracleParameterToBaseType_OracleIntervalDS_Null_Throws(TimeSpan t1)
        {
            var oracleParameter = new OracleParameter
            {
                Value = new OracleIntervalDS()
            };
            var metadata = new MetadataOracle();


            var exception = Assert.Throws<InvalidCastException>(() => metadata.ConvertOracleParameterToBaseType(t1.GetType(), oracleParameter));

            Assert.Equal($"Can't cast a null value to {t1.GetType().FullName}", exception.Message);
        }

        [Theory, CustomAutoData]
        public void ConvertOracleParameterToBaseType_OracleIntervalYM(long p1, long? p2, float p3, float? p4, double p5, double? p6)
        {
            var metadata = new MetadataOracle();
            var nul = new OracleIntervalYM();

            Assert.Equal(p1, metadata.ConvertOracleParameterToBaseType(p1.GetType(), new OracleParameter { Value = new OracleIntervalYM(p1) }));
            Assert.Equal(p2, metadata.ConvertOracleParameterToBaseType(p2.GetType(), new OracleParameter { Value = p2.HasValue ? new OracleIntervalYM($"{p2.Value / 12:d9}-{p2.Value % 12:d2}") : nul }));
            Assert.Equal(p3, metadata.ConvertOracleParameterToBaseType(p3.GetType(), new OracleParameter { Value = new OracleIntervalYM(p3) }));
            Assert.Equal(p4, metadata.ConvertOracleParameterToBaseType(p4.GetType(), new OracleParameter { Value = p4.HasValue ? new OracleIntervalYM($"{(int)p4:D9}-{(int)Math.Ceiling((double)(p4 - (int)p4) * 12.0):D2}") : nul }));
            Assert.Equal(p5, metadata.ConvertOracleParameterToBaseType(p5.GetType(), new OracleParameter { Value = new OracleIntervalYM(p5) }));
            Assert.Equal(p6, metadata.ConvertOracleParameterToBaseType(p6.GetType(), new OracleParameter { Value = p6.HasValue ? new OracleIntervalYM($"{(int)p6:D9}-{(int)Math.Ceiling((double)(p6 - (int)p6) * 12.0):D2}") : nul }));
        }

        [Fact]
        public void ConvertOracleParameterToBaseType_OracleIntervalYM_Null_Throws()
        {
            var oracleParameter = new OracleParameter
            {
                Value = new OracleIntervalYM()
            };
            var metadata = new MetadataOracle();
            var type = typeof(long);

            var exception = Assert.Throws<InvalidCastException>(() => metadata.ConvertOracleParameterToBaseType(type, oracleParameter));

            Assert.Equal($"Can't cast a null value to {type.FullName}", exception.Message);
        }

        [Theory, CustomAutoData]
        public void ConvertOracleParameterToBaseType_OracleBinary(byte[] bytes)
        {
            var oracleParameter = new OracleParameter
            {
                Value = new OracleBinary(bytes)
            };
            var metadata = new MetadataOracle();

            var actual = metadata.ConvertOracleParameterToBaseType(typeof(byte[]), oracleParameter);

            Assert.Equal(bytes, actual);
        }

        [Theory, CustomAutoData]
        public void ConvertOracleParameterToBaseType_OracleTimeStamp(DateTime d1, DateTime? d2)
        {
            var metadata = new MetadataOracle();
            var nul = new OracleTimeStamp();

            Assert.Equal(d1, metadata.ConvertOracleParameterToBaseType(d1.GetType(), new OracleParameter { Value = new OracleTimeStamp(d1) }));
            Assert.Equal(d2, metadata.ConvertOracleParameterToBaseType(d2.GetType(), new OracleParameter { Value = d2.HasValue ? new OracleTimeStamp(d2.Value.ToString("MM'/'dd'/'yyyy HH:mm:ss.fffffff")) : nul }));
        }

        [Fact]
        public void ConvertOracleParameterToBaseType_OracleTimeStamp_Null_Throws()
        {
            var oracleParameter = new OracleParameter
            {
                Value = new OracleTimeStamp()
            };
            var metadata = new MetadataOracle();
            var type = typeof(DateTime);

            var exception = Assert.Throws<InvalidCastException>(() => metadata.ConvertOracleParameterToBaseType(type, oracleParameter));

            Assert.Equal($"Can't cast a null value to {type.FullName}", exception.Message);
        }

        [Fact]
        public void ConvertOracleParameterToBaseType_OracleTimeStamp_Null_AndNullable_Throws()
        {
            var oracleParameter = new OracleParameter
            {
                Value = new OracleTimeStamp()
            };
            var metadata = new MetadataOracle();

            var type = typeof(DateTime?);

            var exception = Assert.Throws<InvalidCastException>(() => metadata.ConvertOracleParameterToBaseType(type, oracleParameter));

            Assert.Equal($"Can't cast a null value to {type.FullName}", exception.Message);
        }

        [Theory, CustomAutoData]
        public void ConvertOracleParameterToBaseType_OracleTimeStampLTZ(DateTime d1, DateTime? d2)
        {
            var metadata = new MetadataOracle();
            var nul = new OracleTimeStampLTZ();

            Assert.Equal(d1, metadata.ConvertOracleParameterToBaseType(d1.GetType(), new OracleParameter { Value = new OracleTimeStamp(d1) }));
            Assert.Equal(d2, metadata.ConvertOracleParameterToBaseType(d2.GetType(), new OracleParameter { Value = d2.HasValue ? new OracleTimeStampLTZ(d2.Value.ToString("MM'/'dd'/'yyyy HH:mm:ss.fffffff")) : nul }));
        }

        [Fact]
        public void ConvertOracleParameterToBaseType_OracleTimeStampLTZ_Null_Throws()
        {
            var oracleParameter = new OracleParameter
            {
                Value = new OracleTimeStampLTZ()
            };
            var metadata = new MetadataOracle();
            var type = typeof(DateTime);

            var exception = Assert.Throws<InvalidCastException>(() => metadata.ConvertOracleParameterToBaseType(type, oracleParameter));

            Assert.Equal($"Can't cast a null value to {type.FullName}", exception.Message);
        }

        [Fact]
        public void ConvertOracleParameterToBaseType_OracleTimeStampLTZ_Null_AndNullable_Throws()
        {
            var oracleParameter = new OracleParameter
            {
                Value = new OracleTimeStampLTZ()
            };
            var metadata = new MetadataOracle();

            var type = typeof(DateTime?);

            var exception = Assert.Throws<InvalidCastException>(() => metadata.ConvertOracleParameterToBaseType(type, oracleParameter));

            Assert.Equal($"Can't cast a null value to {type.FullName}", exception.Message);
        }

        [Theory, CustomAutoData]
        public void ConvertOracleParameterToBaseType_OracleTimeStampTZ(DateTime d1, DateTime? d2)
        {
            var metadata = new MetadataOracle();
            var nul = new OracleTimeStampTZ();

            Assert.Equal(d1, metadata.ConvertOracleParameterToBaseType(d1.GetType(), new OracleParameter { Value = new OracleTimeStamp(d1) }));
            Assert.Equal(d2, metadata.ConvertOracleParameterToBaseType(d2.GetType(), new OracleParameter { Value = d2.HasValue ? new OracleTimeStampTZ(d2.Value.ToString("MM'/'dd'/'yyyy HH:mm:ss.fffffff")) : nul }));
        }

        [Fact]
        public void ConvertOracleParameterToBaseType_OracleTimeStampTZ_Null_Throws()
        {
            var oracleParameter = new OracleParameter
            {
                Value = new OracleTimeStampTZ()
            };
            var metadata = new MetadataOracle();
            var type = typeof(DateTime);

            var exception = Assert.Throws<InvalidCastException>(() => metadata.ConvertOracleParameterToBaseType(type, oracleParameter));

            Assert.Equal($"Can't cast a null value to {type.FullName}", exception.Message);
        }

        [Fact]
        public void ConvertOracleParameterToBaseType_OracleTimeStampTZ_Null_AndNullable_Throws()
        {
            var oracleParameter = new OracleParameter
            {
                Value = new OracleTimeStampTZ()
            };
            var metadata = new MetadataOracle();

            var type = typeof(DateTime?);

            var exception = Assert.Throws<InvalidCastException>(() => metadata.ConvertOracleParameterToBaseType(type, oracleParameter));

            Assert.Equal($"Can't cast a null value to {type.FullName}", exception.Message);
        }

        [Theory, CustomAutoData]
        public void ConvertOracleParameterToBaseType_InvalidType_ReturnsNull(TestClass test)
        {
            var metadata = new MetadataOracle();

            Assert.Null(metadata.ConvertOracleParameterToBaseType(typeof(int?), test));
        }

        #endregion ConvertOracleParameterToBaseType

        #region GetOracleParameter

        [Theory, CustomAutoData]
        public void GetOracleParameter_IsValueType_Char(char p1, ParameterDirection direction, string name)
        {
            var metadata = new MetadataOracle();

            var parameter = metadata.GetOracleParameter(p1.GetType(), direction, name, p1);

            Assert.NotNull(parameter);
            Assert.Equal(OracleDbType.Char, parameter.OracleDbType);
            Assert.Equal(direction, parameter.Direction);
            Assert.Equal(p1, parameter.Value);
        }

        [Theory, CustomAutoData]
        public void GetOracleParameter_IsValueType_Short(short p1, ParameterDirection direction, string name)
        {
            var metadata = new MetadataOracle();

            var parameter = metadata.GetOracleParameter(p1.GetType(), direction, name, p1);

            Assert.NotNull(parameter);
            Assert.Equal(OracleDbType.Int16, parameter.OracleDbType);
            Assert.Equal(direction, parameter.Direction);
            Assert.Equal(p1, parameter.Value);
        }

        [Theory, CustomAutoData]
        public void GetOracleParameter_IsValueType_Int(int p1, ParameterDirection direction, string name)
        {
            var metadata = new MetadataOracle();

            var parameter = metadata.GetOracleParameter(p1.GetType(), direction, name, p1);

            Assert.NotNull(parameter);
            Assert.Equal(OracleDbType.Int32, parameter.OracleDbType);
            Assert.Equal(direction, parameter.Direction);
            Assert.Equal(p1, parameter.Value);
        }

        [Theory, CustomAutoData]
        public void GetOracleParameter_IsValueType_Long(long p1, ParameterDirection direction, string name)
        {
            var metadata = new MetadataOracle();

            var parameter = metadata.GetOracleParameter(p1.GetType(), direction, name, p1);

            Assert.NotNull(parameter);
            Assert.Equal(OracleDbType.Int64, parameter.OracleDbType);
            Assert.Equal(direction, parameter.Direction);
            Assert.Equal(p1, parameter.Value);
        }

        [Theory, CustomAutoData]
        public void GetOracleParameter_IsValueType_Float(float p1, ParameterDirection direction, string name)
        {
            var metadata = new MetadataOracle();

            var parameter = metadata.GetOracleParameter(p1.GetType(), direction, name, p1);

            Assert.NotNull(parameter);
            Assert.Equal(OracleDbType.Single, parameter.OracleDbType);
            Assert.Equal(direction, parameter.Direction);
            Assert.Equal(p1, parameter.Value);
        }

        [Theory, CustomAutoData]
        public void GetOracleParameter_IsValueType_Double(double p1, ParameterDirection direction, string name)
        {
            var metadata = new MetadataOracle();

            var parameter = metadata.GetOracleParameter(p1.GetType(), direction, name, p1);

            Assert.NotNull(parameter);
            Assert.Equal(OracleDbType.Double, parameter.OracleDbType);
            Assert.Equal(direction, parameter.Direction);
            Assert.Equal(p1, parameter.Value);
        }

        [Theory, CustomAutoData]
        public void GetOracleParameter_IsValueType_Decimal(decimal p1, ParameterDirection direction, string name)
        {
            var metadata = new MetadataOracle();

            var parameter = metadata.GetOracleParameter(p1.GetType(), direction, name, p1);

            Assert.NotNull(parameter);
            Assert.Equal(OracleDbType.Decimal, parameter.OracleDbType);
            Assert.Equal(direction, parameter.Direction);
            Assert.Equal(p1, parameter.Value);
        }

        [Theory, CustomAutoData]
        public void GetOracleParameter_IsValueType_DateTime(DateTime p1, ParameterDirection direction, string name)
        {
            var metadata = new MetadataOracle();

            var parameter = metadata.GetOracleParameter(p1.GetType(), direction, name, p1);

            Assert.NotNull(parameter);
            Assert.Equal(OracleDbType.Date, parameter.OracleDbType);
            Assert.Equal(direction, parameter.Direction);
            Assert.Equal(p1, parameter.Value);
        }

        [Theory, CustomAutoData]
        public void GetOracleParameter_IsValueType_Bool(bool p1, ParameterDirection direction, string name)
        {
            var metadata = new MetadataOracle();

            var parameter = metadata.GetOracleParameter(p1.GetType(), direction, name, p1);

            Assert.NotNull(parameter);
            Assert.Equal(OracleDbType.Boolean, parameter.OracleDbType);
            Assert.Equal(direction, parameter.Direction);
            Assert.Equal(p1, parameter.Value);
        }

        [Theory, CustomAutoData]
        public void GetOracleParameter_IsValueType_ByteArray(byte[] p1, ParameterDirection direction, string name)
        {
            var metadata = new MetadataOracle();

            var parameter = metadata.GetOracleParameter(p1.GetType(), direction, name, p1);

            Assert.NotNull(parameter);
            Assert.Equal(OracleDbType.Blob, parameter.OracleDbType);
            Assert.Equal(direction, parameter.Direction);
            Assert.Equal(p1, parameter.Value);
        }

        [Theory, CustomAutoData]
        public void GetOracleParameter_IsValueType_String(string p1, ParameterDirection direction, string name)
        {
            var fixture = new Fixture();
            var metadata = new MetadataOracle();

            var parameter = metadata.GetOracleParameter(p1.GetType(), direction, name, p1);

            Assert.NotNull(parameter);
            Assert.Equal(OracleDbType.Varchar2, parameter.OracleDbType);
            Assert.Equal(direction, parameter.Direction);
            Assert.Equal(p1, parameter.Value);

            if (direction != ParameterDirection.Input)
                Assert.Equal(32000, parameter.Size);
        }

        [Theory, CustomAutoData]
        public void GetOracleParameter_IsValueType_String_OutputSize(string p1, string name)
        {
            var fixture = new Fixture();
            var metadata = new MetadataOracle();

            var parameter = metadata.GetOracleParameter(p1.GetType(), ParameterDirection.Output, name, p1);

            Assert.NotNull(parameter);
            Assert.Equal(OracleDbType.Varchar2, parameter.OracleDbType);
            Assert.Equal(ParameterDirection.Output, parameter.Direction);
            Assert.Equal(p1, parameter.Value);
            Assert.Equal(32000, parameter.Size);
                
        }

        [Theory, CustomAutoData]
        public void GetOracleParameter_IsValueType_String_Clob(char value, ParameterDirection direction, string name)
        {
            var fixture = new Fixture();
            var metadata = new MetadataOracle();
            var str = new string(value, 32001);

            var parameter = metadata.GetOracleParameter(str.GetType(), direction, name, str);

            Assert.NotNull(parameter);
            Assert.Equal(OracleDbType.Clob, parameter.OracleDbType);
            Assert.Equal(direction, parameter.Direction);
            Assert.Equal(str, parameter.Value);
            Assert.Equal(default(int), parameter.Size);
        }

        [Theory, CustomAutoData]
        public void GetOracleParameter_IsValueType_NotHandledType(ParameterDirection direction, string name)
        {
            var fixture = new Fixture();
            var metadata = new MetadataOracle();

            var parameter = metadata.GetOracleParameter(typeof(object), direction, name, DBNull.Value);

            Assert.NotNull(parameter);
            Assert.Equal(OracleDbType.Varchar2, parameter.OracleDbType);
            Assert.Equal(direction, parameter.Direction);
        }

        #endregion GetOracleParameter

        #region GetValueFromOracleXML

        [Fact]
        public void GetValueFromOracleXML_ParametersNull_ReturnsNull()
        {
            var metadata = new MetadataOracle();

            Assert.Null(metadata.GetValueFromOracleXML(typeof(string), null));
            Assert.Null(metadata.GetValueFromOracleXML(typeof(string), OracleXmlType.Null));
            Assert.Null(metadata.GetValueFromOracleXML(null, OracleXmlType.Null));
        }

        [Theory, CustomAutoData]
        public void GetValueFromOracleXML_Object(string expected)
        {
            var metadata = new MetadataOracle();
            var connection = new OracleConnection();
            var type = typeof(OracleXmlType);

            var typeData = FormatterServices.GetUninitializedObject(
            Assembly.GetAssembly(type).GetType("OracleInternal.ServiceObjects.OraXmlTypeData"));

            typeData.GetType().GetField("m_typeOfXmlData", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(typeData, 2);

            typeData.GetType().GetField("m_xmlStr", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(typeData, $"<e>{expected}</e>");

            var xmlImpl = FormatterServices.GetUninitializedObject(
            Assembly.GetAssembly(connection.GetType()).GetType("OracleInternal.ServiceObjects.OracleXmlTypeImpl"));

            xmlImpl.GetType().GetField("m_syncLock", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(xmlImpl, new object());
            xmlImpl.GetType().GetField("m_xmlTypeData", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(xmlImpl, typeData);

            var xmlType = (OracleXmlType)FormatterServices.GetUninitializedObject(typeof(OracleXmlType));
            type.GetField("m_xmlTypeImpl", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(xmlType, xmlImpl);
            type.GetField("m_bClosed", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(xmlType, false);
            type.GetField("m_bNotNull", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(xmlType, true);


            var result = metadata.GetValueFromOracleXML(typeof(string), xmlType);

            Assert.Equal(expected, result);
        }

        #endregion GetValueFromOracleXML

        [Theory, CustomAutoData]
        public void GetOracleParameterForRefCursor(int startNumber)
        {
            var metadata = new MetadataOracle();

            var refCursor = metadata.GetOracleParameterForRefCursor(startNumber);

            Assert.NotNull(refCursor);
            Assert.Equal($":{startNumber}", refCursor.ParameterName);
            Assert.Equal(OracleDbType.RefCursor, refCursor.OracleDbType);
            Assert.Null(refCursor.Value);
        }
    }
}
