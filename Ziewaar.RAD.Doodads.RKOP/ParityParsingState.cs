namespace Ziewaar.RAD.Doodads.RKOP;

[Flags]
public enum ParityParsingState
{ New = 0b010, Changed = 0b011, Unchanged = 0b001, Void = 0b000 };
