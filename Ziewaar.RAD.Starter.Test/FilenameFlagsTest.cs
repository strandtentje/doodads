namespace Ziewaar.RAD.Starter.Test;

[TestClass]
public class FilenameFlagsTest
{
    [TestMethod]
    public void NoFlagsTest()
    {
        string[] input = [];
        var result = input.ToMultipleFilenameFlags("c");
        Assert.IsTrue(Enumerable.SequenceEqual(result, []));
    }
    [TestMethod]
    public void MismatchFlagTest()
    {
        string[] input = ["shell32.dll", "cake.exe"];
        var result = input.ToMultipleFilenameFlags("c");
        Assert.IsTrue(Enumerable.SequenceEqual(result, ["-c0000", @"""shell32.dll""", "-c0001", @"""cake.exe"""]));
    }
}
