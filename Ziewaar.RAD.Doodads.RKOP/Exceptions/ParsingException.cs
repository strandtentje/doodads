using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Exceptions;

[Serializable]
public class ParsingException(CursorText text, string message) : ExceptionAtPositionInFile(text, message);