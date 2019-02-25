using AutoFixture;
using AutoFixture.Xunit2;
using ServForOracle.NetCore.OracleAbstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServForOracle.NetCore.UnitTests.Config
{
    public class CustomAutoDataAttribute : AutoDataAttribute
    {
        public CustomAutoDataAttribute()
            : base(() => new Fixture()
            .Customize(new UdtInfoCustomization()))
        { }
    }

    public class UdtInfoCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register<string, string, string, string, OracleUdtInfo>((p1, p2, p3, p4) => new OracleUdtInfo(p1, p2, p3, p4));
        }
    }
}
