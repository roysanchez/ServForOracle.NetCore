using AutoFixture;
using ServForOracle.NetCore.OracleAbstracts;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace ServForOracle.NetCore.UnitTests
{
    public class UDTInfoAttributeTests
    {
        public class TestClass
        { }

        public Type GetTypeWithAttribute(string schema, string name, string collectionSchema, string collectionName)
        {
            var fixture = new Fixture();
            var type = typeof(TestClass);
            var assemblyName = new AssemblyName(fixture.Create<string>());

            
            var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            var module = assembly.DefineDynamicModule(assemblyName.Name);

            var typeDef = module.DefineType(fixture.Create<string>(),  TypeAttributes.Public, type);

            var attrCtorParams = new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) };
            var attrCtorInfo = typeof(OracleUdtAttribute).GetConstructor(attrCtorParams);

            var attrBuilder = new CustomAttributeBuilder(attrCtorInfo, new object[] { schema, name, collectionSchema, collectionName });
            typeDef.SetCustomAttribute(attrBuilder);

            var newType = typeDef.CreateType();
            return newType;
        }
    }
}
