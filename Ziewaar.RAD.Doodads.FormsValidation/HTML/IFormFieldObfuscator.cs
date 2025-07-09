namespace FormHandling.Interfaces
{
    public interface IFormFieldObfuscator
    {
        string Obfuscate(string originalName);
        string Deobfuscate(string obfuscatedName);
    }
}