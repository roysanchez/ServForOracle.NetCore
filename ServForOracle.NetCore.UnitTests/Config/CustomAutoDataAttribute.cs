using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging;
using Moq;
using Oracle.ManagedDataAccess.Types;
using ServForOracle.NetCore.Cache;
using ServForOracle.NetCore.Metadata;
using ServForOracle.NetCore.OracleAbstracts;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace ServForOracle.NetCore.UnitTests.Config
{
    public class CustomAutoDataAttribute : AutoDataAttribute
    {
        public CustomAutoDataAttribute()
            : base(() => new Fixture()
            .Customize(new UdtInfoCustomization())
            .Customize(new AutoMoqCustomization()))
        { }
    }

    public class UdtInfoCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register<string, string, string, string, OracleUdtInfo>((p1, p2, p3, p4) => new OracleUdtInfo(p1, p2, p3, p4));

            fixture.Register<string, string, string, string, OracleUdtAttribute>((p1, p2, p3, p4) => new OracleUdtAttribute(p1, p2, p3, p4));

            fixture.Register<string, string, Type>((aName, typeName) =>
            {
                var assemblyName = new AssemblyName(aName);
                var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

                var module = assembly.DefineDynamicModule(assemblyName.Name);

                var typeDef = module.DefineType(fixture.Create<string>(), TypeAttributes.Public);

                var newType = typeDef.CreateType();
                return newType;
            });

            fixture.Register<ServForOracleCache, Type, MetadataOracleTypeDefinition, UdtPropertyNetPropertyMap[], MetadataOracleNetTypeDefinition>((cache, type, typedef, propMap) =>
            {
                return new MetadataOracleNetTypeDefinition(cache, type, typedef, propMap, true);
            });

            fixture.Register<double, OracleDecimal>((p) => new OracleDecimal(p));

            fixture.Register<Mock<DbConnection>, ServForOracleCache, ILogger, Mock<MetadataBuilder>>((connection, cache, logger) =>
            {
                connection.SetupGet(c => c.ConnectionString).Returns(fixture.Create<string>());
                return new Mock<MetadataBuilder>(connection.Object, cache, logger);
            });
        }
    }
}
