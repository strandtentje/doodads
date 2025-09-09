namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support;

public static class NodeExtensions
{
    public static HtmlNode RemapValuesForAttributes(this HtmlNode node, string[] attributeNames,
        IReadOnlyDictionary<string, string> map)
    {
        foreach (HtmlAttribute attribute in node.Attributes)
            if (attributeNames.Contains(attribute.Name))
                attribute.Value = map[attribute.Name];
        foreach (HtmlNode child in node.ChildNodes)
            child.RemapValuesForAttributes(attributeNames, map);
        return node;
    }

    public static string? GetAttributeOrDefault(this HtmlNode node, string name) =>
        node.GetAttributes().SingleOrDefault(x => x.Name == name)?.Value;

    public static decimal? GetNumericAttributeOrDefault(this HtmlNode node, string name, decimal defaultValue) =>
        decimal.TryParse(node.GetAttributeOrDefault(name), out decimal result) ? result : defaultValue;

    public static uint GetUnsignedAttributeOrDefault(this HtmlNode node, string name, uint defaultValue) =>
        uint.TryParse(node.GetAttributeOrDefault(name), out uint result) ? result : defaultValue;

    public static InputClass[] GetInputClasses(
        this IEnumerable<HtmlNode> nodes) =>
    [
        ..nodes.Select(node => new InputClass(
            node.Name, node.GetAttributeValue("type", "text")))
    ];

    public static string[] GetInputValues(this IEnumerable<HtmlNode> nodes) =>
    [
        ..nodes.NotDisabled()
            .Select(x => x.GetAttributeOrDefault("value"))
            .OfType<string>().Distinct()
    ];

    public static string[] GetAcceptAttributes(this IEnumerable<HtmlNode> nodes) =>
    [
        ..nodes.NotDisabled()
            .Select(x => x.GetAttributeOrDefault("accept"))
            .OfType<string>().Distinct()
    ];

    public static IEnumerable<HtmlNode> NotDisabled(this IEnumerable<HtmlNode> nodes) =>
        nodes.Where(x => x.Attributes.All(attr => attr.Name != "disabled"));

    public static string[] GetOptionValues(this IEnumerable<HtmlNode> nodes) =>
    [
        ..nodes.NotDisabled().SelectMany(subnode => subnode.ChildNodes.NotDisabled().Where(opt => opt.Name == "option"))
            .Select(x => x.GetAttributeOrDefault("value")).OfType<string>().Distinct()
    ];

    public static string[] GetValidLiteralValues(this IEnumerable<HtmlNode> nodes)
    {
        var htmlNodes = nodes as HtmlNode[] ?? nodes.ToArray();
        return [..htmlNodes.GetInputValues().Concat(htmlNodes.GetOptionValues())];
    }

    public static bool IsOptionType(this IEnumerable<HtmlNode> nodes)
    {
        var classes = nodes.GetInputClasses();
        if (classes.All(x => x.Type == "radio" || x.Type == "checkbox" || x.Tag == "select"))
            return true;
        if (nodes.All(x => !string.IsNullOrWhiteSpace(x.GetAttributeOrDefault("readonly"))))
            return true;
        return false;
    }

    public static uint GetMinLength(this IEnumerable<HtmlNode> nodes)
    {
        return nodes.NotDisabled().Select(x => x.GetUnsignedAttributeOrDefault("minlength", uint.MinValue))
            .Concat([(uint)0])
            .OrderByDescending(x => x).First();
    }

    public static uint GetMaxLength(this IEnumerable<HtmlNode> nodes)
    {
        return nodes.NotDisabled().Select(x => x.GetUnsignedAttributeOrDefault("maxlength", uint.MaxValue))
            .OrderBy(x => x).Concat([(uint)1000]).First();
    }

    public static string[] GetMinValues(this IEnumerable<HtmlNode> nodes) =>
        nodes.NotDisabled()
            .Select(groupedInputNode => groupedInputNode.GetAttributeOrDefault("min")).OfType<string>()
            .Distinct().ToArray();

    public static string[] GetMaxValues(this IEnumerable<HtmlNode> nodes) =>
        nodes.NotDisabled()
            .Select(groupedInputNode => groupedInputNode.GetAttributeOrDefault("max")).OfType<string>()
            .Distinct().ToArray();

    public static string[] GetPatterns(this IEnumerable<HtmlNode> nodes) =>
        nodes.NotDisabled()
            .Select(groupedInputNode => groupedInputNode.GetAttributeOrDefault("pattern")).OfType<string>()
            .Distinct().ToArray();

    public static int GetMinExpectedValueCount(this IEnumerable<HtmlNode> nodes)
    {
        int requirementScore = 0;
        var requiredRadiosInGroupCount = nodes.Count(groupedInputNode =>
            groupedInputNode.GetInputTypeName() == "radio" &&
            groupedInputNode.Attributes.Any(x => x.Name == "required"));
        if (requiredRadiosInGroupCount > 0) requirementScore = 1;
        var requiredCheckboxesInGroup = nodes.Count(groupedInputNode =>
            groupedInputNode.GetInputTypeName() == "checkbox" &&
            groupedInputNode.Attributes.Any(x => x.Name == "required"));
        if (requiredCheckboxesInGroup > 0) requirementScore += requiredCheckboxesInGroup;
        var minLengthsInGroup = nodes.Count(groupedInputNode =>
            groupedInputNode.GetNumericAttributeOrDefault("minlength", Decimal.MinValue) > 0);
        requirementScore += minLengthsInGroup;
        return requirementScore;
    }

    public static int GetMaxExpectedValueCount(this IEnumerable<HtmlNode> nodes)
    {
        var htmlNodes = (nodes as HtmlNode[] ?? nodes.ToArray()).NotDisabled().ToArray();
        var selects = htmlNodes.Where(selectCandidate => selectCandidate.Name == "select").ToArray();
        var nonSelectCount = htmlNodes.Count(selectCandidate => selectCandidate.Name != "select");
        var singleSelectCount = selects.Count(selectNode =>
            selectNode.Attributes.All(singleCandidate => singleCandidate.Name != "multiple"));
        var multiSelectCount = selects.Where(selectNode =>
                selectNode.Attributes.Any(multipleCandidate => multipleCandidate.Name == "multiple"))
            .Select(multiSelectMembers =>
                multiSelectMembers.ChildNodes.Count(candidateOption => candidateOption.Name == "option")).Sum();
        return nonSelectCount + singleSelectCount + multiSelectCount;
    }
}

public record InputClass(string Tag, string Type);