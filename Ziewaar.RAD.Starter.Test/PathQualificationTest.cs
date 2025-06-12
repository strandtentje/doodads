namespace Ziewaar.RAD.Starter.Test;

[TestClass]
public class PathQualificationTest
{
    [TestMethod]
    public void QualifyNothing()
    {
        var input = "";
        var result = input.QualifyFullPath("C:\\Windows\\System32");
        Assert.AreEqual("C:\\Windows\\System32", result);
    }
    [TestMethod]
    public void QualifyRelative()
    {
        var input = "shell32.dll";
        var result = input.QualifyFullPath("C:\\Windows\\System32");
        Assert.AreEqual("C:\\Windows\\System32\\shell32.dll", result);
    }
    [TestMethod]
    public void QualifyAbsolute()
    {
        var input = "C:\\Windows\\System32\\shell32.dll";
        var result = input.QualifyFullPath("C:\\Windows\\System32");
        Assert.AreEqual(input, result);
    }
    [TestMethod]
    public void QualifyAbsoluteTrailing()
    {
        var input = "C:\\Windows\\System32\\shell32.dll";
        var result = input.QualifyFullPath("C:\\Windows\\System32\\");
        Assert.AreEqual(input, result);
    }
    [TestMethod]
    public void QualifyNotMismatching()
    {
        var input = "C:\\Windows\\System32\\shell32.dll";
        var result = input.QualifyFullPath("C:\\Program Files\\");
        Assert.AreEqual(input, result);
    }
}
