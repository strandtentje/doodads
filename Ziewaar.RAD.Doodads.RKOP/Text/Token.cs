namespace Ziewaar.RAD.Doodads.RKOP.Text;

public class Token(TokenDescription description, string text, bool valid)
{
    public TokenDescription Description => description;
    public string Text => text;
    public bool IsValid => valid;
}
