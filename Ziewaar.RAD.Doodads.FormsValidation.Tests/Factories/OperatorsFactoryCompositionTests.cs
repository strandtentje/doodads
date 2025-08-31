using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Bounding;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.FieldType;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Operators;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Factories;
[TestClass]
public class OperatorsFactoryCompositionTests
{
    [TestMethod]
    public void AllValid_Collections_Compose_AsPipelineForStrictInput()
    {
        // Username must be 3..12 chars AND match pattern AND be one of allowed options
        var lengthF = new LengthValidatorCollectionFactory(3, 12);
        var patternF = new PatternValidationCollectionFactory(@"[a-z0-9_]+");
        var optionsF = new OptionsValidatingCollectionFactory(new[] { "user_1", "guest", "admin_01" });

        var allFactory = new AllValidCollectionsFactory(lengthF, patternF, optionsF);
        var coll = allFactory.Create();

        coll.Add("admin_01");
        Assert.IsTrue(coll.IsSatisfied);
        // AndValidatingCollection exposes ValidItems of the last precedent
        CollectionAssert.AreEqual(new object[] { "admin_01" }, coll.ValidItems.Cast<object>().ToArray());

        coll = allFactory.Create();
        coll.Add("too-long-username");
        Assert.IsFalse(coll.IsSatisfied);
    }

    [TestMethod]
    public void AnyValid_Collections_Compose_ForAlternativeInput()
    {
        // Accept either a decimal number OR a CSS hex color (exact match via pattern)
        var numberF = new ValidatingNumberCollectionFactory();
        var colorF = new PatternValidationCollectionFactory(@"#[0-9A-Fa-f]{6}");
        var anyFactory = new AnyValidCollectionFactory(numberF, colorF);
        var coll = anyFactory.Create();

        coll.Add("#1a2b3c");
        Assert.IsTrue(coll.IsSatisfied);
        // AnyValidatingCollection returns ValidItems of the last satisfied sub-collection
        CollectionAssert.AreEqual(new object[] { "#1a2b3c" }, coll.ValidItems.Cast<object>().ToArray());

        coll = anyFactory.Create();
        coll.Add("not-a-number-and-not-a-color");
        Assert.IsFalse(coll.IsSatisfied);
    }

    [TestMethod]
    public void AnyValid_WithBoundsAndTypeFactories_MixedAcceptance()
    {
        // Either a bounded number in [-5,5] OR a valid month
        var boundedNumbers = new BoundsValidatingNumberCollectionFactory(new[] { "-5" }, new[] { "5" });
        var month = new ValidatingMonthCollectionFactory();
        var anyFactory = new AnyValidCollectionFactory(month, boundedNumbers);

        var coll = anyFactory.Create();
        coll.Add("3.14");
        Assert.IsTrue(coll.IsSatisfied);
        CollectionAssert.AreEqual(new object[] { 3.14m }, coll.ValidItems.Cast<object>().ToArray());

        coll = anyFactory.Create();
        coll.Add("2024-07");
        Assert.IsTrue(coll.IsSatisfied);
        CollectionAssert.AreEqual(new object[] { new DateOnly(2024, 7, 1) }, coll.ValidItems.Cast<object>().ToArray());
    }

    [TestMethod]
    public void AllValid_WithTypeAndPattern_FiltersInputs()
    {
        // Email input that must also match a simple domain pattern
        var emailF = new ValidatingEmailCollectionFactory();
        var domainPatternF = new PatternValidationCollectionFactory(@".+@example\.com");

        var allFactory = new AllValidCollectionsFactory(emailF, domainPatternF);
        var coll = allFactory.Create();

        coll.Add("alice@example.com");
        Assert.IsTrue(coll.IsSatisfied);

        coll = allFactory.Create();
        coll.Add("bob@other.com"); // pattern fails (even if EmailValidator would accept)
        Assert.IsFalse(coll.IsSatisfied);
    }
}