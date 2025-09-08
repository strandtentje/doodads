using System.Runtime.Serialization;
using System.Web;
using Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;

namespace Ziewaar.RAD.Doodads.Testkit;
[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        var expectHi = new RkopTestingHarness(
            $"""
             <<>> : Print("HI");
             """).GetRequest("/", new Dictionary<string, string>(0)).ResponseAsString();
        Assert.AreEqual("HI", expectHi);
    }
}