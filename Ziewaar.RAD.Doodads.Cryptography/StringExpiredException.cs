namespace Ziewaar.RAD.Doodads.Cryptography;
public class StringExpiredException(string originalName) : Exception($"Attempted to read twice from value that was originally under {originalName}");