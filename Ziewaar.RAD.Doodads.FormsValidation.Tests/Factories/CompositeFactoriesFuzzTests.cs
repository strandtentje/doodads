using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Composite;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Operators;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Factories;
[TestClass]
public class CompositeFactoriesFuzzTests
{
    // ---------- TypeValidatingCollectionFactory unhappy paths ----------
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

    [TestMethod]
    public void TypeFactory_NullArguments_ThrowOrFallback()
    {
        // fieldType == "file" short-circuits regardless of tag; null tag should not be used
        var fFile = new TypeValidatingCollectionFactory(fieldTag: null, fieldType: "file");
        var cFile = fFile.Create();
        var fi = new FileInfo(Path.GetTempFileName());
        cFile.Add(fi);
        Assert.IsTrue(cFile.IsSatisfied);
        Assert.AreSame(fi, cFile.ValidItems.Cast<object>().Single());
    }

    [TestMethod]
    public void TypeFactory_WeirdTypeNames_DefaultsToNonValidating()
    {
        var f = new TypeValidatingCollectionFactory("input", "datetime");        // not "datetime-local"
        var c = f.Create();
        c.Add("anything");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual("anything", c.ValidItems.Cast<object>().Single());

        f = new TypeValidatingCollectionFactory("button", "number");             // tag != input
        c = f.Create();
        c.Add("whatever");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual("whatever", c.ValidItems.Cast<object>().Single());
    }

    [TestMethod]
    public void TypeFactory_CorrectMappings_Sanity()
    {
        var f1 = new TypeValidatingCollectionFactory("input", "time");
        var c1 = f1.Create();
        c1.Add("09:00");
        Assert.IsTrue(c1.IsSatisfied);
        Assert.AreEqual(new TimeOnly(9,0), c1.ValidItems.Cast<TimeOnly>().Single());

        var f2 = new TypeValidatingCollectionFactory("input", "week");
        var c2 = f2.Create();
        c2.Add("2020-W25");
        Assert.IsTrue(c2.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020,6,15), c2.ValidItems.Cast<DateOnly>().Single());

        var f3 = new TypeValidatingCollectionFactory("input", "range"); // should route to number
        var c3 = f3.Create();
        c3.Add("3.14");
        Assert.IsTrue(c3.IsSatisfied);
        Assert.AreEqual(3.14m, c3.ValidItems.Cast<decimal>().Single());
    }

    [TestMethod]
    public void TypeFactory_Fuzz_MismatchedValues_Inval()
    {
        var dateF = new TypeValidatingCollectionFactory("input", "date").Create();
        dateF.Add("not-a-date");
        Assert.IsFalse(dateF.IsSatisfied);

        var emailF = new TypeValidatingCollectionFactory("input", "email").Create();
        emailF.Add("missing-at.example.com");
        Assert.IsFalse(emailF.IsSatisfied);

        var colorF = new TypeValidatingCollectionFactory("input", "color").Create();
        colorF.Add("#zzzzzz");
        Assert.IsFalse(colorF.IsSatisfied);

        var timeF = new TypeValidatingCollectionFactory("input", "time").Create();
        timeF.Add("24:61");
        Assert.IsFalse(timeF.IsSatisfied);

        var weekF = new TypeValidatingCollectionFactory("input", "week").Create();
        weekF.Add("2020-25");
        Assert.IsFalse(weekF.IsSatisfied);

        var numberF = new TypeValidatingCollectionFactory("input", "number").Create();
        numberF.Add("1.234.5"); // depending on implementation it should fail (ValidateNumber uses InvariantCulture in your earlier class)
        Assert.IsFalse(numberF.IsSatisfied);
    }

    // ---------- BoundsValidatingCollectionFactory unhappy paths ----------

    [TestMethod]
    public void BoundsWrapper_UnknownType_DefaultsToNonValidating_IgnoresBadBounds()
    {
        var bw = new BoundsValidatingCollectionFactory("not-a-real-type", new[] { "garbage" }, new[] { "still-garbage" });
        var c = bw.Create();
        c.Add("anything");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual("anything", c.ValidItems.Cast<object>().Single());
    }

    [TestMethod]
    public void BoundsWrapper_FieldTypeMismatch_ThrowsOnConstruction()
    {
        // number factory uses decimal.Parse; should throw on non-numeric
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingCollectionFactory("number", new[] { "NaN" }, new[] { "10" }));

        // date factory uses DateOnly.Parse; should throw bad format
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingCollectionFactory("date", new[] { "2020-13-01" }, new[] { "2020-12-31" }));

        // datetime-local factory uses DateTime.Parse; "nope" should throw
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingCollectionFactory("datetime-local", new[] { "nope" }, new[] { "2020-01-01T00:00:00" }));

        // time factory uses TimeOnly.Parse; wrong separator
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingCollectionFactory("time", new[] { "08-00" }, new[] { "17:30" }));

        // week factory: bounds must be ISO week strings; wrong format should throw in WeekOnly.Parse inside factory
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingCollectionFactory("week", new[] { "2020-25" }, new[] { "2020-W53" }));

    }

    [TestMethod]
    public void BoundsWrapper_Constructors_NullOrWhitespaceBounds_Throw()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new BoundsValidatingCollectionFactory("number", null, new[] { "10" }));

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new BoundsValidatingCollectionFactory("date", new[] { "2020-01-01" }, null));

        // Whitespace strings should bubble into Parse and throw FormatException
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingCollectionFactory("time", new[] { "   " }, new[] { "17:00" }));
    }

    // ---------- Compositing with Any/All using the composite factories ----------

    [TestMethod]
    public void All_Of_TypePlusBounds_StrictPipeline_UnhappyValues()
    {
        // input type number + bounds [-5,5]
        var tf = new TypeValidatingCollectionFactory("input", "number");
        var bf = new BoundsValidatingCollectionFactory("number", new[] { "-5" }, new[] { "5" });
        var all = new AllValidCollectionsFactory(tf, bf).Create();

        all.Add("#ff00aa"); // wrong type before bounds
        Assert.IsFalse(all.IsSatisfied);

        all = new AllValidCollectionsFactory(tf, bf).Create();
        all.Add("6"); // correct type, but out of bounds
        Assert.IsFalse(all.IsSatisfied);

        all = new AllValidCollectionsFactory(tf, bf).Create();
        all.Add("3"); // happy path
        Assert.IsTrue(all.IsSatisfied);
        CollectionAssert.AreEqual(new object[] { 3m }, all.ValidItems.Cast<object>().ToArray());
    }

    [TestMethod]
    public void Any_Of_TypedValidators_FailsWhenAllFail()
    {
        var numberType = new TypeValidatingCollectionFactory("input", "number");
        var colorType  = new TypeValidatingCollectionFactory("input", "color");
        var any = new AnyValidCollectionFactory(numberType, colorType).Create();

        any.Add("not-a-number-and-not-a-color");
        Assert.IsFalse(any.IsSatisfied);
    }

    [TestMethod]
    public void Any_Of_TypeAndBounds_MixedBadBoundsAndInputs()
    {
        // Month bounds factory with strict DateOnly.Parse bounds
        var monthBounds = new BoundsValidatingCollectionFactory("month", new[] { "2020-01-01" }, new[] { "2020-12-31" });
        var numberType = new TypeValidatingCollectionFactory("input", "number");

        var any = new AnyValidCollectionFactory(monthBounds, numberType).Create();
        any.Add("2020-13"); // invalid month value; month validator should flip to false, number remains not attempted for this input
        Assert.IsFalse(any.IsSatisfied);

        any = new AnyValidCollectionFactory(monthBounds, numberType).Create();
        any.Add("3.14"); // satisfies number branch
        Assert.IsTrue(any.IsSatisfied);
        CollectionAssert.AreEqual(new object[] { 3.14m }, any.ValidItems.Cast<object>().ToArray());
    }

    // ---------- “Fuzzier” content: lots of malformed values through the Type factory ----------

    [TestMethod]
    public void TypeFactory_FuzzValues_AcrossTypes_ManyUnhappy()
    {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        var cases = new (string tag, string type, object value, bool expectOk, Type expectStoredType, object expectValue)[]
        {
            ("input","color", "#abc", true, typeof(string), "#abc"),
            ("input","color", "abc", false, null, null),
            ("input","date", "2020-02-30", false, null, null),               // invalid date
            ("input","date", "2020-02-29", true, typeof(DateOnly), new DateOnly(2020,2,29)),
            ("input","datetime-local","2020-01-01T00:00:00", true, typeof(DateTime), new DateTime(2020,1,1,0,0,0)),
            ("input","datetime-local","2020-01-01T00:00:00", true, typeof(DateTime), new DateTime(2020,1,1,0,0,0)), // lenient parser
            ("input","email","user@example", false, null, null),
            ("input","month","2020-07", true, typeof(DateOnly), new DateOnly(2020,7,1)),
            ("input","number","-12.5", true, typeof(decimal), -12.5m),
            ("input","number","-12.0.5", false, null, null),                   // commas not allowed by invariant
            ("input","time","24:00", false, null, null),
            ("input","time","23:59", true, typeof(TimeOnly), new TimeOnly(23,59)),
            ("input","week","2020-W00", false, null, null),                   // invalid week 0
            ("input","week","2020-W01", true, typeof(DateOnly), new DateOnly(2019,12,30)),
        };
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

        foreach (var (tag,type,value,ok,storedType,storedValue) in cases)
        {
            var f = new TypeValidatingCollectionFactory(tag, type);
            var c = f.Create();
            c.Add(value);
            if (ok)
            {
                Assert.IsTrue(c.IsSatisfied, $"{tag}/{type} should have accepted '{value}'");
                var it = c.ValidItems.Cast<object>().Single();
                Assert.AreEqual(storedType, it.GetType(), $"{tag}/{type} stored type mismatch for '{value}'");
                Assert.AreEqual(storedValue, it);
            }
            else
            {
                Assert.IsFalse(c.IsSatisfied, $"{tag}/{type} should have rejected '{value}'");
            }
        }
    }
}