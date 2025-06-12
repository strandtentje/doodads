namespace Ziewaar.RAD.Starter.Test;

[TestClass]
public class IsolateFlagsTest
{
    [TestMethod]
    public void NoValuesStartWithNoInput()
    {
        SortedList<string, object> input = new SortedList<string, object>();
        var result = input.GetValuesStartingWith<string>("c");
        Assert.AreEqual(0, result.Length);
    }
    [TestMethod]
    public void NoValuesStartWithMismatchedInput()
    {
        SortedList<string, object> input = new SortedList<string, object>()
        {
            { "d0000", "cake" },
            { "e0000", "pie" }
        };
        var result = input.GetValuesStartingWith<string>("c");
        Assert.AreEqual(0, result.Length);
    }
    [TestMethod]
    public void SomeValuesStartWith()
    {
        SortedList<string, object> input = new SortedList<string, object>()
        {
            { "c0000", "cake" },
            { "e0000", "pie" }
        };
        var result = input.GetValuesStartingWith<string>("c");
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual("cake", result[0]);
    }
    [TestMethod]
    public void AllValuesStartWithAndKeepSorting()
    {
        SortedList<string, object> input = new SortedList<string, object>()
        {
            { "c0000", "cake" },
            { "c0001", "pie" }
        };
        var result = input.GetValuesStartingWith<string>("c");
        Assert.AreEqual(2, result.Length);
        Assert.AreEqual("cake", result[0]);
        Assert.AreEqual("pie", result[1]);
    }

    [TestMethod]
    public void RemoveNoValuesStartWithNoInput()
    {
        SortedList<string, object> input = new SortedList<string, object>();
        var result = input.RemoveValuesStartingWith("c");
        Assert.AreEqual(0, result.Count);
    }
    [TestMethod]
    public void RemoveNoValuesStartWithMismatchedInput()
    {
        SortedList<string, object> input = new SortedList<string, object>()
        {
            { "d0000", "cake" },
            { "e0000", "pie" }
        };
        var result = input.RemoveValuesStartingWith("f");
        Assert.AreEqual(2, result.Count);
    }
    [TestMethod]
    public void RemoveSomeValuesStartWith()
    {
        SortedList<string, object> input = new SortedList<string, object>()
        {
            { "c0000", "cake" },
            { "e0000", "pie" }
        };
        var result = input.RemoveValuesStartingWith("c");
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("pie", result["e0000"]);
    }
    [TestMethod]
    public void RemoveAllValuesStartWithAndKeepSorting()
    {
        SortedList<string, object> input = new SortedList<string, object>()
        {
            { "c0000", "cake" },
            { "f0001", "pie" }
        };
        var result = input.RemoveValuesStartingWithAny("c", "f");
        Assert.AreEqual(0, result.Count);
    }

}
