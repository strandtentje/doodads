using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class SingleValueEnumeratorTests
{
    [TestMethod]
    public void Enumerator_ProvidesSingleValue_AndThenStops()
    {
        var enumerator = new SingleValueEnumerator<int>(42);
        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreEqual(42, enumerator.Current);
        Assert.IsFalse(enumerator.MoveNext());
    }

    [TestMethod]
    public void Enumerator_Dispose_DoesNotThrow()
    {
        var enumerator = new SingleValueEnumerator<int>(99);
        enumerator.Dispose(); // should be a no-op
    }
}