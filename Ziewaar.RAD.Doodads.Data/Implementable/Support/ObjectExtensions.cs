#nullable enable
using System;

namespace Ziewaar.RAD.Doodads.Data.Implementable.Support;
#pragma warning disable 67
internal static class ObjectExtensions
{
    public static Exception? SloppyDispose(this IDisposable disposable)
    {
        try
        {
            disposable.Dispose();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}