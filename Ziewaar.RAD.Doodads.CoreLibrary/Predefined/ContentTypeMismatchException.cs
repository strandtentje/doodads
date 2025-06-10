namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class ContentTypeMismatchException(string expected, string actual)
    : Exception($"Expected content type {expected} but got {actual}");