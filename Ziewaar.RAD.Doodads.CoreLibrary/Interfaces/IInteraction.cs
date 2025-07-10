﻿#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
public interface IInteraction
{
    IInteraction Stack { get; }
    object Register { get; }
    IReadOnlyDictionary<string, object> Memory { get; }
}
public interface ICsrfTokenSourceInteraction : IInteraction
{
    public ICsrfFields Fields { get; }
}
