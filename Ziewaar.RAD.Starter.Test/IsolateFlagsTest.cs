namespace Ziewaar.RAD.Starter.Test;

[TestClass]
public class IsolateFlagsTest
{
    [TestMethod]
    public void NoValuesStartWithNoInput()
    {
        SortedList<string, object> input = new SortedList<string, object>();
        var result = input.GetValuesStartingWith<string>("c");
        Assert.IsEmpty(result);
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
        Assert.IsEmpty(result);
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
        Assert.HasCount(1, result);
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
        Assert.HasCount(2, result);
        Assert.AreEqual("cake", result[0]);
        Assert.AreEqual("pie", result[1]);
    }

    [TestMethod]
    public void RemoveNoValuesStartWithNoInput()
    {
        SortedList<string, object> input = new SortedList<string, object>();
        var result = input.RemoveValuesStartingWith("c");
        Assert.IsEmpty(result);
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
        Assert.HasCount(2, result);
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
        Assert.HasCount(1, result);
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
        Assert.HasCount(0, result);
    }

}
