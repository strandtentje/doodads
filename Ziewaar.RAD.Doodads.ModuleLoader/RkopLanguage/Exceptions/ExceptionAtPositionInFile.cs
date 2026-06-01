using Ziewaar.RAD.Doodads.ModuleLoader.RkopLanguage.Text;

namespace Ziewaar.RAD.Doodads.ModuleLoader.RkopLanguage.Exceptions;

public class ExceptionAtPositionInFile(CursorText text, string message) : Exception($"""
    In File[Position]: "{text.BareFile}"[{text.GetCurrentLine()}:{text.GetCurrentCol()}] 
    {message}
    Dir: "{text.WorkingDirectory}"
    """);
