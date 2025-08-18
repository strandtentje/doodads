#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Bridge;

public record struct ServiceIdentity
{
    public string Typename, Filename;
    public int Line, Position;
    public override string ToString() => $"{Typename} in {Filename}@{Line}:{Position}";
}