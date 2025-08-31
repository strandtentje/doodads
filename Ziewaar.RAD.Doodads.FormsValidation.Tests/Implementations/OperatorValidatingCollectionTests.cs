using System.Text.RegularExpressions;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.FieldType;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.Operators;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Interfaces;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class OperatorValidatingCollectionTests
{
    [TestMethod]
    public void And_Realistic_UsernameRules_AllMustPass()
    {
        // Simulate: username must be 3..12 chars AND match pattern AND be in whitelist
        var length = new LengthValidatorCollection(3, 12);
        var pattern = new PatternValidatingCollection(new Regex(@"^[a-z0-9_]+$"));
        var options = new OptionsValidatingCollection(new[] { "user_1", "guest", "admin_01" });

        var and = new AndValidatingCollection(new IValidatingCollection[] { length, pattern, options });

        and.Add("admin_01");

        Assert.IsTrue(and.IsSatisfied, "All three validators should be satisfied.");
        // And.ValidItems returns the last precedent's ValidItems (options)
        Assert.AreEqual("admin_01", and.ValidItems.Cast<object>().Single());
    }

    [TestMethod]
    public void And_WhenOneFails_OverallFails()
    {
        var length = new LengthValidatorCollection(3, 6);
        var pattern = new PatternValidatingCollection(new Regex(@"^[a-z]+$"));
        var options = new OptionsValidatingCollection(new[] { "ok", "alsook" });

        var and = new AndValidatingCollection(new IValidatingCollection[] { length, pattern, options });

        and.Add("toolong"); // length fails -> pattern/options may not even run effectively

        Assert.IsFalse(and.IsSatisfied);
    }

    [TestMethod]
    public void Any_Realistic_NumberOrHex_ColorInput()
    {
        // Accept either a decimal number OR a CSS hex color.
        var numbers = new ValidatingNumberCollection();
        var colors = new ValidatingColorCollection();

        var any = new AnyValidatingCollection(new IValidatingCollection[] { numbers, colors });

        any.Add("#1a2b3c"); // valid color

        Assert.IsTrue(any.IsSatisfied, "At least one sub-validator should be satisfied.");
        // Any.ValidItems returns ValidItems of the last satisfied collection.
        // Here, both collections were tried: numbers fails, colors passes, so colors is the last satisfied.
        Assert.AreEqual("#1a2b3c", any.ValidItems.Cast<object>().Single());
    }

    [TestMethod]
    public void Any_WhenNonePass_OverallFails_ValidItemsAccessWouldThrowIfUsed()
    {
        var numbers = new ValidatingNumberCollection();
        var colors = new ValidatingColorCollection();
        var any = new AnyValidatingCollection(new IValidatingCollection[] { numbers, colors });

        any.Add("not-a-number-and-not-a-color");

        Assert.IsFalse(any.IsSatisfied);
        // Do NOT access any.ValidItems here; implementation would throw since no satisfied sub-collection exists.
    }
}