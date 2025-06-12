namespace Ziewaar.RAD.Starter.Test;

[TestClass]
public class PathTruncationTest

{
    [TestMethod]
    public void TruncateNothing()
    {
        var input = "";
        var result = input.TruncatePathStart("C:\\Windows\\System32");
        Assert.AreEqual(input, result);
    }
    [TestMethod]
    public void TruncateRelative()
    {
        var input = "shell32.dll";
        var result = input.TruncatePathStart("C:\\Windows\\System32");
        Assert.AreEqual(input, result);
    }
    [TestMethod]
    public void TruncateAbsolute()
    {
        var input = "C:\\Windows\\System32\\shell32.dll";
        var result = input.TruncatePathStart("C:\\Windows\\System32");
        Assert.AreEqual("shell32.dll", result);
    }
    [TestMethod]
    public void TruncateAbsoluteTrailing()
    {
        var input = "C:\\Windows\\System32\\shell32.dll";
        var result = input.TruncatePathStart("C:\\Windows\\System32\\");
        Assert.AreEqual("shell32.dll", result);
    }
    [TestMethod]
    public void TruncateNotMismatching()
    {
        var input = "C:\\Windows\\System32\\shell32.dll";
        var result = input.TruncatePathStart("C:\\Program Files\\");
        Assert.AreEqual(input, result);
    }
}
