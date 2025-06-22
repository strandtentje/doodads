#nullable enable
#pragma warning disable 67

using Ziewaar;

namespace Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

public static class ObjectExtensions
{
    public static object ConvertNumericToDecimal(this object obj) => obj switch
    {
        byte b => (decimal)b,
        sbyte sb => (decimal)sb,
        short s => (decimal)s,
        ushort us => (decimal)us,
        int i => (decimal)i,
        uint ui => (decimal)ui,
        long l => (decimal)l,
        ulong ul => (decimal)ul,
        float f => (decimal)f,
        double d => (decimal)d,
        decimal dec => dec,
        _ => obj,
    };
}
