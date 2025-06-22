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
    public static object ConvertDecimalToType(this decimal value, Type targetType)
    {
        if (targetType == null) throw new ArgumentNullException(nameof(targetType));

        if (targetType == typeof(byte)) return (byte)value;
        if (targetType == typeof(sbyte)) return (sbyte)value;
        if (targetType == typeof(short)) return (short)value;
        if (targetType == typeof(ushort)) return (ushort)value;
        if (targetType == typeof(int)) return (int)value;
        if (targetType == typeof(uint)) return (uint)value;
        if (targetType == typeof(long)) return (long)value;
        if (targetType == typeof(ulong)) return (ulong)value;
        if (targetType == typeof(float)) return (float)value;
        if (targetType == typeof(double)) return (double)value;
        if (targetType == typeof(decimal)) return value;

        throw new InvalidCastException($"Cannot cast decimal to unsupported type '{targetType.Name}'.");
    }
}
