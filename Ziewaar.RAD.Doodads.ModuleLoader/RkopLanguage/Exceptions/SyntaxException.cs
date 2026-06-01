using Ziewaar.RAD.Doodads.ModuleLoader.RkopLanguage.Text;

namespace Ziewaar.RAD.Doodads.ModuleLoader.RkopLanguage.Exceptions;

[Serializable]
public class SyntaxException(CursorText text, string message) : ExceptionAtPositionInFile(text, message);