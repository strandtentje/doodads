#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.FileNumbering;

public class Fraction(int numerator, Denominator denom)
{
    public int NumberPrefix => numerator * denom.Scale;
    public int Numerator => numerator;
    public Denominator Denom => denom;
}
