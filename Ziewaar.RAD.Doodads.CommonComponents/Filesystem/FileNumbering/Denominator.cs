#pragma warning disable 67
using Define.Doodads.Expo.Timeline;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.FileNumbering;

public class Denominator(int value)
{
    public int Value => value;
    public void Increment() => value++;

    public int Scale => field > 0 ? field : field = Value switch
    {
        < 10 => 100,
        < 20 => 50,
        < 50 => 20,
        < 100 => 100,
        < 200 => 50,
        < 500 => 20,
        < 1000 => 100,
        < 2000 => 50,
        < 5000 => 20,
        < 10000 => 100,
        < 20000 => 50,
        < 50000 => 20,
        < 100000 => 10,
        _ => throw new BasicException("denominator too big.")
    };
    public string Format => field != null ? field : field = Value switch
    {
        < 1000 => "000",
        < 10000 => "0000",
        < 100000 => "00000",
        _ => throw new BasicException("denominator too big"),
    };
}
