#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;

[Category("Call Definition Return")]
[Title("The opposite of call")]
[Description(""""
    "For when you don't want whatever's running, to run anymore.
    """")]
public class Annihilate : Call
{
    protected override bool Destroyer => true;
}
