using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Exceptions;

public class ExceptionAtPositionInFile(CursorText text, string message) : Exception($"""
    In File[Position]: "{text.BareFile}"[{text.GetCurrentLine()}:{text.GetCurrentCol()}] 
    {message}
    Dir: "{text.WorkingDirectory}"
    """);
