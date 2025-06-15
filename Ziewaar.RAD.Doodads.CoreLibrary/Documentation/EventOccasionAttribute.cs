namespace Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
[AttributeUsage(AttributeTargets.Event)]
public class EventOccasionAttribute(string eventOccasion) : Attribute
{
    public string EventOccasion => eventOccasion;
}

[AttributeUsage(AttributeTargets.Event)]
public class NeverHappensAttribute : Attribute
{
    
}