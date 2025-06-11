using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Exceptions;

[Serializable]
public class SyntaxException(CursorText text, string message) : ExceptionAtPositionInFile(text, message);