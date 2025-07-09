using FormHandling.Interfaces;

namespace FormHandling.Obfuscation
{
    public class NullFormFieldObfuscator : IFormFieldObfuscator
    {
        public string Obfuscate(string originalName) => originalName;
        public string Deobfuscate(string obfuscatedName) => obfuscatedName;
    }
}